using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BluePlayer
{
	public partial class App : Application
	{
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			_ = WaitForApplicationLaunchFully(e.Args);
		}

		private async Task WaitForApplicationLaunchFully(string[] args)
		{
			await AsyncExtension.WaitUntil(() => Current.MainWindow == null);

			string[] foundSoundFiles = ((MainWindow)Current.MainWindow).CheckSoundFiles(args);

			foreach (string path in foundSoundFiles)
			{
				((MainWindow)Current.MainWindow).AddFile(path);
			}
		}
	}
}
