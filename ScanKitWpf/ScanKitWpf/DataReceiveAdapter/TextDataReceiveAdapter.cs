using HPSocket.Adapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanKitWpf.DataReceiveAdapter
{
    public class TextDataReceiveAdapter : TerminatorDataReceiveAdapter<string>
    {
        public TextDataReceiveAdapter() 
            : base(terminator:Encoding.UTF8.GetBytes("\r\n"))
        {
        }

        public override string ParseRequestBody(byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }
    }
}
