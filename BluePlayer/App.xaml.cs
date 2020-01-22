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

		/// <summary>
		/// Gather files from args if application is loaded fully (that means Current.MainWindow isn't null).
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		private async Task WaitForApplicationLaunchFully(string[] args)
		{
			await AsyncExtension.WaitUntil(() => Current.MainWindow == null);

			string[] foundSoundFiles = ((MainWindow)Current.MainWindow).CheckSoundFiles(args);
			string[] playlistFiles = ((MainWindow)Current.MainWindow).CheckPlaylist(args);

			foreach (string path in foundSoundFiles)
			{
				((MainWindow)Current.MainWindow).AddFile(path);
			}

			foreach (string path in playlistFiles)
			{
				((MainWindow)Current.MainWindow).LoadPlaylist(path);
			}
		}

		private void Application_Exit(object sender, ExitEventArgs e)
		{

		}
	}
}
