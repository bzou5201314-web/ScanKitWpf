using HPSocket.Adapter;
using Microsoft.Extensions.Options;
using ScanKitWpf.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanKitWpf.DataReceiveAdapter
{
    public class TextDataReceiveAdapter : TerminatorDataReceiveAdapter<string>
    {
        public TextDataReceiveAdapter(IOptionsMonitor<AppConfig> config) 
            : base(terminator:Encoding.UTF8.GetBytes(config.CurrentValue.EndMark))
        {
        }

        public override string ParseRequestBody(byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }
    }
}
