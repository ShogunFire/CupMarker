using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CupMarker.Models
{
    public class AutoScriptParam
    {
        public AutoScriptParam(string dxfFilePath, double height, double centerY, double partDiameter)
        {
            this.DxfFilePath = dxfFilePath;
            this.Height = height;
            this.CenterY = centerY;
            this.PartDiameter = partDiameter;
        }
        public string DxfFilePath;
        public double Height;
        public double CenterY;

        public double PartDiameter;

    }
}
