using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CupMarker.Models;
using System;
using System.Collections.Generic;
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
            Y1 = 0;
            Y2 = 592;
        }
        [ObservableProperty]
        private double heightCupMark;

        [ObservableProperty]
        private double centerYMark;


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

        private OrderInfo? CupOrderInfo { get; set; } = null;

        [ObservableProperty]
        private BitmapImage previewImage;

        public IAsyncRelayCommand GetOrderCommand { get; }

        private async Task GetOrderCommandAsync()
        {
            await FetchOrderInfo();

            await DownloadSvgAsync();

            await DownloadPreviewAsync();

            HasOrderInfoLoaded = true;
        }

        private async Task FetchOrderInfo()
        {
            string jsonUrl = $"https://pir-prod.pirani.life/co/{Barcode}";

            using HttpClient client = new HttpClient();

            // 1. Get the JSON
            var jsonString = await client.GetStringAsync(jsonUrl);
            CupOrderInfo = JsonSerializer.Deserialize<OrderInfo>(jsonString);
        }


        private async Task DownloadSvgAsync()
        {
            if (CupOrderInfo?.SvgUrl == null)
                return;
            
            CupOrderInfo.SvgLocalPath = await DownloadFileAsync(CupOrderInfo.SvgUrl);
        }

        private async Task DownloadPreviewAsync()
        {
            if (CupOrderInfo?.PreviewUrl == null)
                return;

            CupOrderInfo.PreviewLocalPath = await DownloadFileAsync(CupOrderInfo.PreviewUrl);
            if (CupOrderInfo.PreviewLocalPath != null)
            {
                PreviewImage = new BitmapImage();
                PreviewImage.BeginInit();
                PreviewImage.UriSource = new Uri(CupOrderInfo.PreviewLocalPath, UriKind.Absolute);
                PreviewImage.CacheOption = BitmapCacheOption.OnLoad;
                PreviewImage.EndInit();
            }
        }

        private async Task<string?> DownloadFileAsync(string url)
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
            string folder = AppDomain.CurrentDomain.BaseDirectory + "/tmp_images/";
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
        }

        partial void OnBarcodeSelectedChanged(string value)
        {
            this.Barcode = value;
        }
        partial void OnY1Changed(double value)
        {
            recalculateHeight();
        }
        partial void OnY2Changed(double value)
        {
            recalculateHeight();
        }
        public void recalculateHeight()
        {
            HeightCupMark = Y2 - Y1;
            CenterYMark = (Y1 + Y2)/2;
        }
    }
}
