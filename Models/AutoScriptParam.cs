using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CupMarker.Models
{
    public class AutoScriptParam
    {
        public AutoScriptParam(string svgfilePath, double height, double centerY)
        {
            this.SvgfilePath = svgfilePath;
            this.Height = height;
            this.CenterY = centerY;
        }
        public string SvgfilePath;
        public double Height;
        public double CenterY;

    }
}
