using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CupMarker.AutoItScript;
using CupMarker.Helpers;
using CupMarker.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CupMarker.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {



        public MainViewModel()
        {
            GetOrderCommand = new AsyncRelayCommand(GetOrderCommandAsync);
            
        }
        [ObservableProperty] private double canvasHeight;


        public string SvgLocalPath { get; set; } = "";

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

        private async Task GetOrderCommandAsync()
        {
            if(Barcode == null || Barcode.Length == 0) return;

            await FetchOrderInfo();
            if (CupOrderInfo == null) return;
            await DownloadSvgAsync();

            await DownloadPreviewAsync();
            Y1 = 0;
            Y2 = CanvasHeight;
            recalculateHeightAndCenterY();


            HasOrderInfoLoaded = true;
        }

        private async Task FetchOrderInfo()
        {
            string jsonUrl = $"https://pir-prod.pirani.life/co/{Barcode}";

            using HttpClient client = new HttpClient();

            // 1. Get the JSON
            try
            {
                var jsonString = await client.GetStringAsync(jsonUrl);
                CupOrderInfo = JsonSerializer.Deserialize<OrderInfo>(jsonString);
            }
            catch { }
        }


        private async Task DownloadSvgAsync()
        {
            if (CupOrderInfo?.SvgUrl == null)
                return;
            
            SvgLocalPath = await DownloadFileAsync(CupOrderInfo.SvgUrl);
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
            string extension = System.IO.Path.GetExtension(uri.AbsolutePath);
            string prefix = extension == ".svg" ? "SVG_" : "PREVIEW_";
            // Split the barcode on '/' and take second part if exists
            string filename = Barcode;
            var parts = Barcode.Split('/');
            if (parts.Length > 1)
                filename = parts[1];
            filename = $"{prefix}{filename}{extension}";
            string folder = AppDomain.CurrentDomain.BaseDirectory + "tmp_images\\";
            Directory.CreateDirectory(folder);

            string path =  folder + filename;

            using HttpClient client = new HttpClient();
            var svgBytes = await client.GetByteArrayAsync(url);
            await File.WriteAllBytesAsync(path, svgBytes);


            return path;
        }





        [RelayCommand]
        private void StartJob()
        {
            EzCadAutoItScript.DoTheJob(new AutoScriptParam(SvgLocalPath,heightCupMarkMM, centerYMarkDifferenceMM));
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
