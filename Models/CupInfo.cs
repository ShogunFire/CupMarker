using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CupMarker.Models
{
    public class OrderInfo
    {

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("variantTitle")]
        public string? VariantTitle { get; set; }

        [JsonPropertyName("sku")]
        public string? Sku { get; set; }

        [JsonPropertyName("customizationType")]
        public string? CustomizationType { get; set; }

        [JsonPropertyName("customizationValue")]
        public string? CustomizationValue { get; set; }

        [JsonPropertyName("customizationFont")]
        public string? CustomizationFont { get; set; }

        [JsonPropertyName("previewUrl")]
        public string? PreviewUrl { get; set; }

        [JsonPropertyName("svgUrl")]
        public string? SvgUrl { get; set; }

        [JsonPropertyName("color")]
        public string? Color { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("barcode")]
        public string? Barcode { get; set; }

        [JsonPropertyName("created")]
        public DateTime Created { get; set; }

        public string? SvgLocalPath { get; set; }

        public string? PreviewLocalPath { get; set; }
    }
}
