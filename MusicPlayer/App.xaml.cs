using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MusicPlayer
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length == 1)
            {
                // MessageBox.Show(e.Args[0]);

                // ((MainWindow)Current.MainWindow).pathToFirstSong = e.Args[0];
            }
        }
    }
}
