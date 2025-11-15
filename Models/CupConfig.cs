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

        public double CupMarkableHeightInMM { get; set; }
        public double TopDiameterInMM { get; set; }

        public double BottomDiameterInMM { get; set; }


        public double CupMarkerZeroHeightInMM { get; set; }


       
    }
}
