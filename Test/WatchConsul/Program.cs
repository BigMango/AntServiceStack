using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatchConsul
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                File.CreateText("0.txt");
            }
            else
            {
                File.CreateText("1.txt");
            }
        }
    }
}
