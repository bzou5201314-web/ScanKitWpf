using Caliburn.Micro;
using HandyControl.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanKitWpf.Models
{
    public class AppConfig
    {
        public string Title { get; set; }
        public ImgCenterPoint ImgCenter { get; set; }

        public CameraInfo Camera { get; set; }

        public PicConfig PicSaveConfig { get; set;}

        public required string ServerIp { get; set; }

        public ushort ServerPort { get; set; }

        public ObservableCollection<CodeConfig> ScanCodeConfig { get; set; }

        public TriggerConfig Trigger { get; set; }
    }

    public class ImgCenterPoint
    {
        public double Rotate_7_Center_X { get; set; }

        public double Rotate_7_Center_Y { get; set; }

        public double Rotate_13_Center_X { get; set; }

        public double Rotate_13_Center_Y { get; set; }

        public double Rotate_15_Center_X { get; set; }

        public double Rotate_15_Center_Y { get; set; }
    }

    public class CameraInfo
    {
        public string Name { get; set; }

        public float ExposeTime { get; set; }

        public float Scale { get; set; }

        public float Mask { get; set; }

        public float Factor { get; set; }

        //多次识别，提高条形码识别率
        public bool MulPasing { get; set; }
    }

    public class PicConfig
    {
        public string PicType { get; set; }
        public string PicPath { get; set; }
    }

    public class CodeConfig
    {
        public string CodeType { get; set; }
        public bool IsChecked { get; set; }
    }

    public class TriggerConfig
    {
        public string TriggerMode { get; set; }

        public string TriggerSource { get; set; }

        public string TriggerCommand { get; set; }
    }
}
