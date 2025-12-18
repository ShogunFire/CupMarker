using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CupMarker.AutoItScript;
using CupMarker.Helpers;
using CupMarker.Models;
using CupMarker.Services;
using CupMarker.Services.Interfaces;

using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Xml.Linq;


namespace CupMarker.ViewModels
{
    public partial class CupControlViewModel : BaseViewModel
    {
        private readonly INetSuiteApiService _apiService;
        private readonly ISessionService _sessionService;



        [ObservableProperty] private double canvasHeight;


        public string SvgLocalPath { get; set; } = "";

        public string DxfLocalPath { get; set; } = "";

        public string PreviewLocalPath { get; set; } = "";

        private double heightCupMark;
        private double heightCupMarkMM;

        [ObservableProperty]
        private string heightCupMarkString = "";

        private double centerYMark;
        private double centerYMarkDifferenceMM;

        [ObservableProperty]
        private string centerYMarkString = "";


        [ObservableProperty]
        private string barcodeSelected = "";

        [ObservableProperty]
        private string barcode = "";

        [ObservableProperty]
        private bool hasOrderInfoLoaded = false;

        [ObservableProperty]
        private double y1;

        [ObservableProperty]
        private double y2;

        [ObservableProperty]
        private OrderInfo? cupOrderInfo;

        [ObservableProperty]
        private CupConfig? activeConfig;

        [ObservableProperty]
        private BitmapImage? previewImage;

        public IAsyncRelayCommand GetOrderCommand { get; }

        private readonly HttpClient httpClient = new HttpClient();
        public CupControlViewModel(INetSuiteApiService apiService, ISessionService sessionService)
        {
            GetOrderCommand = new AsyncRelayCommand(GetOrderCommandAsync);
            _apiService = apiService;
            _sessionService = sessionService;

        }

        private async Task GetOrderCommandAsync()
        {
           

            if (Barcode == null || Barcode.Length == 0) return;

            await FetchOrderInfo();
            if (CupOrderInfo == null) return;



            await Task.WhenAll(
                DownloadSvgAsync(),
                DownloadDxfAsync(),
                DownloadPreviewAsync()
            );
            //Y1 = 0;
            //Y2 = CanvasHeight;
            recalculateHeightAndCenterY();


            HasOrderInfoLoaded = true;
        }

        private async Task FetchOrderInfo()
        {
            string jsonUrl = $"https://pir-prod.pirani.life/co/{Barcode}";

            // 1. Get the JSON
            try
            {
                var jsonString = await httpClient.GetStringAsync(jsonUrl);
                CupOrderInfo = JsonSerializer.Deserialize<OrderInfo>(jsonString);
            }
            catch { }
        }


        private async Task DownloadSvgAsync()
        {
            if (CupOrderInfo?.SvgUrl == null)
                return;
            
            SvgLocalPath = await DownloadFileAsync(CupOrderInfo.SvgUrl);
            GetHeightSvg(SvgLocalPath);
        }

        private async Task DownloadDxfAsync()
        {
            if (CupOrderInfo?.DxfUrl == null)
                return;

            DxfLocalPath = await DownloadFileAsync(CupOrderInfo.DxfUrl);
        }

        private async Task DownloadPreviewAsync()
        {
            if (CupOrderInfo?.PreviewUrl == null)
                return;

            PreviewLocalPath = await DownloadFileAsync(CupOrderInfo.PreviewUrl);
            if (PreviewLocalPath != null)
            {
                PreviewImage = new BitmapImage();
                PreviewImage.BeginInit();
                PreviewImage.UriSource = new Uri(PreviewLocalPath, UriKind.Absolute);
                PreviewImage.CacheOption = BitmapCacheOption.OnLoad;
                PreviewImage.EndInit();
            }
        }

        private async Task<string> DownloadFileAsync(string url)
        {
            Uri uri = new Uri(url);
            string extension = System.IO.Path.GetExtension(uri.AbsolutePath)?.ToLowerInvariant();
            string prefix = extension switch
            {
                ".svg" => "SVG_",
                ".dxf" => "DXF_",
                _ => "PREVIEW_"
            };
            // Split the barcode on '/' and take second part if exists
            string filename = Barcode;
            var parts = Barcode.Split('/');
            if (parts.Length > 1)
                filename = parts[1];
            filename = $"{prefix}{filename}{extension}";
            string folder = AppDomain.CurrentDomain.BaseDirectory + "tmp_images\\";
            Directory.CreateDirectory(folder);

            string path =  folder + filename;

            var svgBytes = await httpClient.GetByteArrayAsync(url);
            await File.WriteAllBytesAsync(path, svgBytes);


            return path;
        }
        public void GetHeightSvg(string svgPath)
        {
            double heightViewbox = 0;
            //get height viewbox: 
            // Load SVG from file or string
            var svg = XDocument.Load(svgPath);
            // or: XDocument.Parse(svgString);

            var root = svg.Root;
            if (root != null && root.Name.LocalName == "svg")
            {
                var heightAttr = root.Attribute("height")?.Value;
                if (double.TryParse(heightAttr, out  heightViewbox))
                    Console.WriteLine($"Height: {heightViewbox}");
            }




            //get actual height of the svg
            var settings = new WpfDrawingSettings();
            var reader = new FileSvgReader(settings);
            var drawing = reader.Read(svgPath);

            Rect bounds = drawing.Bounds;
            double height = bounds.Height;
            double y = bounds.Y;

            Y1 = y / 1600 * CanvasHeight;
            Y2 = Y1 + (height / 1600 * CanvasHeight);

            if (Y1 < 10)
                Y1 = 10;
            if (Y2 > CanvasHeight)
                Y2 = CanvasHeight - 10;
        }

        [RelayCommand]
        private async void StartJob()
        {
            var operatorName = _sessionService.CurrentUser.Name;
            await _apiService.SetPersOperatorAsync(this.Barcode, $"{operatorName}, {DateTime.Now:hh:mmtt}, {DateTime.Now:MM/dd/yyyy}");
            var partDiameter = CalculatePartDiameter();
            EzCadAutoItScript.DoTheJob(new AutoScriptParam(DxfLocalPath,heightCupMarkMM, centerYMarkDifferenceMM, partDiameter));
        }

        private double CalculatePartDiameter()
        {
            return  (((ActiveConfig.TopDiameterInMM - ActiveConfig.BottomDiameterInMM) / ActiveConfig.CupMarkableHeightInMM) * (centerYMarkDifferenceMM + ActiveConfig.CupMarkerZeroHeightInMM)) + ActiveConfig.BottomDiameterInMM;
        }

        partial void OnBarcodeSelectedChanged(string value)
        {
            this.Barcode = value;
        }
        partial void OnY1Changed(double value)
        {
            recalculateHeightAndCenterY();
        }
        partial void OnY2Changed(double value)
        {
            recalculateHeightAndCenterY();
        }
        public void recalculateHeightAndCenterY()
        {
            if (ActiveConfig != null)
            {
                heightCupMark = Y2 - Y1;
                heightCupMarkMM = heightCupMark * ActiveConfig.HeightInMM / CanvasHeight;
                centerYMark = (Y1 + Y2) / 2;

                double pixelPerMM = CanvasHeight / ActiveConfig.HeightInMM;
                double selectedCenterYMarkInMM = (CanvasHeight - centerYMark) / pixelPerMM;

                centerYMarkDifferenceMM = selectedCenterYMarkInMM - ActiveConfig.CenterYInMM;

                updateLabels();
            }
        }

        partial void OnCupOrderInfoChanged(OrderInfo? value)
        {
            if (value?.Title is null) return;

            // Extract size from title, e.g. "26oz ..." → "26oz"
            var size = value.Title.Split(' ').FirstOrDefault();
            ActiveConfig = ConfigHelper.GetBySize(size);
        }

        private void updateLabels()
        {
            HeightCupMarkString = $"Height: {heightCupMarkMM:F1}mm";
            CenterYMarkString = $"Center Y: {centerYMarkDifferenceMM:F1}mm";
        }

    }
}
