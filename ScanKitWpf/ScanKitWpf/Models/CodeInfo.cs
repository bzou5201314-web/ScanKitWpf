using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanKitWpf.Models
{
    public struct CodeInfo
    {
        public string CodeType { get; set; }
        public string CodeValue { get; set; }
        public double CenterX { get; set; }
        public double CenterY { get; set; }
        public double Angle { get;set; }
    }
}
