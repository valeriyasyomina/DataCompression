using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCompression
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Input file name:");
                String fileName = Console.ReadLine();

                FileReader fileReader = new FileReader();
                byte[] byteData = fileReader.Read(fileName);

                if (byteData.Length == 0)
                {
                    FileWriter fileWriter = new FileWriter();
                    fileWriter.Write(fileName + ".cmps", byteData);
                }
                else
                {
                    LZW lzw = new LZW();
                    Console.WriteLine("Compressing...");
                    byte[] compressedData = lzw.Compress(byteData);
                    Console.WriteLine("Expanding...");
                    byte[] decompressedData = lzw.Expand(compressedData);

                    FileWriter fileWriter = new FileWriter();
                    fileWriter.Write(fileName + ".cmps", compressedData);

                    Console.WriteLine("Input new file name:");
                    String decompressedFileName = Console.ReadLine();
                    fileWriter.Write(decompressedFileName, decompressedData);
                }
                Console.WriteLine("Process finished");
                Console.ReadLine();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                Console.ReadLine();
            }

           
        }
    }
}
