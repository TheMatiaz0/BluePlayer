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
				_ = WaitForApplicationLaunchFully(e.Args[0]);

			}
        }

		private async Task WaitForApplicationLaunchFully (string args)
		{
			await AsyncExtension.WaitUntil(() => Current.MainWindow == null);


			((MainWindow)Current.MainWindow).AddFile(args);
		}
    }
}
