using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MusicPlayer
{
	/// <summary>
	/// Logika interakcji dla klasy MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public ObservableCollection<MusicTrack> musicTracks = new ObservableCollection<MusicTrack>();
		private readonly MediaPlayer mediaPlayer = new MediaPlayer();
		private int maxSongs = 0;

		private int currentSongNumber = 0;

		public MainWindow()
		{
			InitializeComponent();

			DispatcherTimer timer = new DispatcherTimer
			{
				Interval = TimeSpan.FromSeconds(1)
			};

			timer.Tick += Timer_Tick;
			timer.Start();

			AllMusicTracks.ItemsSource = musicTracks;

			DataContext = this;
		}

		private void Timer_Tick(object sender, EventArgs e)
		{
			if (mediaPlayer.Source != null && mediaPlayer.NaturalDuration.HasTimeSpan)
				lblStatus.Content = $"{(int)mediaPlayer.Position.TotalMinutes}:{mediaPlayer.Position.Seconds:00}/{(int)mediaPlayer.NaturalDuration.TimeSpan.TotalMinutes}:{mediaPlayer.NaturalDuration.TimeSpan.Seconds:00}";
			else
				lblStatus.Content = "No file selected...";
		}

		private void BtnPlay_Click(object sender, RoutedEventArgs e)
		{
			mediaPlayer.Play();
		}

		private void BtnPause_Click(object sender, RoutedEventArgs e)
		{
			mediaPlayer.Pause();
		}

		private void BtnStop_Click(object sender, RoutedEventArgs e)
		{
			mediaPlayer.Stop();
		}

		private void Files_Click(object sender, RoutedEventArgs e)
		{
		}

		private void Playlist_Click(object sender, RoutedEventArgs e)
		{

		}

		private void UpdateMusic(MusicTrack music, bool shouldPlay = false)
		{
			if (music == null)
			{
				return;
			}

			mediaPlayer.Open(new Uri(music.Path));

			if (shouldPlay)
			{
				mediaPlayer.Play();
			}
		}

		private void OkBtn_Click(object sender, RoutedEventArgs e)
		{
			CommonOpenFileDialog dialog = new CommonOpenFileDialog
			{
				IsFolderPicker = true
			};
			CommonFileDialogResult result = dialog.ShowDialog();

			if (result == CommonFileDialogResult.Ok)
			{
				GetFiles(dialog.FileName);
				PathInput.Text = dialog.FileName;

				try
				{
					UpdateMusic(musicTracks[currentSongNumber]);
				}

				catch (Exception f)
				{
					MessageBox.Show(f.Message);
				}

			}

		}

		public void GetFiles(string input)
		{
			musicTracks = new ObservableCollection<MusicTrack>();

			AllMusicTracks.ItemsSource = musicTracks;

			DataContext = this;

			string[] foundSoundFiles = (from item in Directory.GetFiles(input, "*", SearchOption.AllDirectories)
										let ext = System.IO.Path.GetExtension(item)
										where ext == ".mp3" || ext == ".wav"
										select item).ToArray();

			maxSongs = 0;
			AddFiles(foundSoundFiles);
		}

		private void AddFiles (string[] collection)
		{
			foreach (string item in collection)
			{
				string[] creators = new string[0];
				string songName = string.Empty;
				string finalCreator = "";

				ShellObject shellFile = ShellObject.FromParsingName(item);
				creators = GetValues(shellFile.Properties.GetProperty(SystemProperties.System.Music.Artist));
				songName = GetValue(shellFile.Properties.GetProperty(SystemProperties.System.Title));

				if (string.IsNullOrEmpty(songName))
				{
					int pos = item.LastIndexOf("\\") + 1;
					string result = new string(item.Skip(pos).ToArray());

					songName = result.Replace(System.IO.Path.GetExtension(result), "");
				}

				if (creators != null)
				{
					foreach (string item2 in creators)
					{
						if (creators.Length > 1)
						{
							finalCreator += $"{item2}, ";
						}

						else
						{
							finalCreator = item2;
						}
					}
				}

				else
				{
					finalCreator = "NaN";
				}


				musicTracks.Add(new MusicTrack(maxSongs, finalCreator, songName, item, System.IO.Path.GetExtension(item)));

				maxSongs += 1;
			}
		}


		private static string GetValue(IShellProperty value)
		{
			if (value == null || value.ValueAsObject == null)
			{
				return String.Empty;
			}

			return value.ValueAsObject.ToString();
		}

		private static string[] GetValues(IShellProperty value)
		{
			if (value == null || value.ValueAsObject == null)
			{
				return null;
			}

			return (string[])value.ValueAsObject;
		}

		private void BtnSkip_Click(object sender, RoutedEventArgs e)
		{
			currentSongNumber += 1;

			if (currentSongNumber > musicTracks.Count - 1)
			{
				currentSongNumber = 0;
				return;
			}

			UpdateMusic(musicTracks[currentSongNumber], true);
		}

		private void BtnBack_Click(object sender, RoutedEventArgs e)
		{
			currentSongNumber -= 1;

			if (currentSongNumber < 1)
			{
				currentSongNumber = musicTracks.Count - 1;
				return;
			}
			UpdateMusic(musicTracks[currentSongNumber], true);
		}

		private void AllMusicTracks_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (((FrameworkElement)e.OriginalSource).DataContext is MusicTrack item)
			{
				currentSongNumber = item.ID;
				UpdateMusic(item, true);
			}
		}

		private void AllMusicTracks_Drop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

				AddFiles(files);
			}
		}

		/*
		private void AllMusicTracks_SelectedItem(object sender, RoutedEventArgs e)
		{
			if (Keyboard.IsKeyDown(Key.Enter))
			{
				if (((FrameworkElement)e.OriginalSource).DataContext is MusicTrack item)
				{
					currentSongNumber = item.ID;
					UpdateMusic(item, true);
				}
			}
		}
		*/

		
	}
}
