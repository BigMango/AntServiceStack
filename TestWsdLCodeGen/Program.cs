using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CTrip.Tools.SOA.WsdlWizard;

namespace TestWsdLCodeGen
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new WsdlWizardForm(@"H:\Csharp\yuzd\Ant.soa\Test\TestContract\"));
        }
    }
}
