using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LZW
{
    public interface ICompress
    {
        byte[] Compress(byte[] message);
        byte[] Decompress(byte[] message);
    }
}
