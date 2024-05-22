using HandyControl.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanKitWpf.Models
{
    public class AppConfig
    {
        public ImgCenterPoint ImgCenter { get; set; }

        public CameraInfo Camera { get; set; }

        public BarCodeConfig BarCode { get; set; }

        public short ServerPort { get; set; }
    }

    public class ImgCenterPoint
    {
        public double Center_X { get; set; }

        public double Center_Y { get; set; }
    }

    public class CameraInfo
    {
        public string Name { get; set; }

        public float ExposeTime { get; set; }

        public float Scale { get; set; }

        public float Mask { get; set; }

        public float Factor { get; set; }
    }

    public class BarCodeConfig
    {
        public string[] CodeType { get; set; }
        public List<BarCodeParam> Params { get; set; }

    }

    public class BarCodeParam
    {
        public string Name { get; set; }

        public string Value { get; set; }
    }
}
