using ScanKitWpf.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanKitWpf
{
    public class CodeParsing
    {
        public static MaterialInfoModel ParseCode(IList<CodeInfo> codeInfos)
        {
            /*foreach (var item in codeInfos.Where(p=>p.CodeType== "QR Code"))
            {
                var codes= item.CodeValue.Split('%', StringSplitOptions.RemoveEmptyEntries);
                if (codes.Length == 4)
                {
                    return new MaterialInfoModel
                    {
                        SN = Guid.NewGuid().ToString(),
                        PN = codes[0],
                        Lot = codes[1],
                        Qty = Convert.ToInt32(codes[2]),
                    };
                }
            }*/
            return new MaterialInfoModel
            {
                SN=Guid.NewGuid().ToString(),
            };
        }
    }
}
