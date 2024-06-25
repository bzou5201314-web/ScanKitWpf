using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanKitWpf.Models
{
    public class RequestModel<T>
    {
        public string MsgType { get; set; }

        public T Data { get; set; }
    }

    public class ReelSize
    {
        public int MatSize { get; set; }
    }
}
