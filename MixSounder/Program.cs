using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MixSounder
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            bool mutex = false;

            if (mutex)
            {
                MutexHandle.Handle("KemaMixSounder", new Form1());
            } else
            {
                Application.Run(new Form1());
            }
        }
    }
}
