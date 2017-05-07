using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GrdApi;

namespace TestApi
{
    class Program
    {
        static void Main(string[] args)
        {
            GrdMap map = new GrdMap(@"C:\Users\Mixon\GRD\relief.grd");
            Console.WriteLine("Text Data: {0}", map.TextData);
            Console.WriteLine("ZMin: {0}", map.ZMin);
            Console.WriteLine("ZMax: {0}", map.ZMax);
            Console.WriteLine("XMin: {0}", map.XMin);
            Console.WriteLine("XMax: {0}", map.XMax);
            Console.WriteLine("YMin: {0}", map.YMin);
            Console.WriteLine("YMax: {0}", map.YMax);
            Console.ReadKey();
        }
    }
}
