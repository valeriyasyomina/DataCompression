using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCompression
{
    public class FrequencyTableItem
    {
        public int SymbolCode { get; set; }
        public int Prefics { get; set; }
        public int Suffics { get; set; }
        public FrequencyTableItem() { }
        public FrequencyTableItem(int symbolCode, int prefics, int suffics)
        {
            SymbolCode = symbolCode; ;
            Prefics = prefics;
            Suffics = suffics;
        }
    }
}
