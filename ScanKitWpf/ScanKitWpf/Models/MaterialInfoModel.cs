using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanKitWpf.Models
{
    public class MaterialInfoModel
    {
        public string SN { get; set; } = "";

        public string PN { get; set; } = "";

        public int Qty { get; set; } = 0;

        public string Lot { get; set; } = "";

        public string DC { get; set; } = "";

        public string Supplier { get; set; } = "";

        public string OtherBarcode { get; set; }="";

        public float RotAngle { get; set; } = 0;

    }
}
