using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CupMarker.Models
{
    public class CupConfig
    {
        public string? Size { get; set; }
        public double HeightInMM { get; set; }
        public double CenterYInMM { get; set; }
    }
}
