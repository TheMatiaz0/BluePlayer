using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        private bool isPlaying;
        private bool isDraggingSlider;

		private int currentSongNumber = 0;

        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;

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
			if (mediaPlayer.Source != null && mediaPlayer.NaturalDuration.HasTimeSpan && isDraggingSlider == false)
            {
                sliProgress.Minimum = 0;
                sliProgress.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                sliProgress.Value = mediaPlayer.Position.TotalSeconds;
            }
		}

		private void BtnPlay_Click(object sender, RoutedEventArgs e)
		{
            mediaPlayer.PlayWithPause(ref isPlaying);
		}

		private void BtnStop_Click(object sender, RoutedEventArgs e)
		{
            isPlaying = false;
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
                isPlaying = true;
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
            if (musicTracks.Count < 0)
            {
                return;
            }

			currentSongNumber += 1;

			if (currentSongNumber > musicTracks.Count - 1)
			{
				currentSongNumber = 0;
			}

			UpdateMusic(musicTracks[currentSongNumber], true);
		}

		private void BtnBack_Click(object sender, RoutedEventArgs e)
		{
            if (musicTracks.Count < 0)
            {
                return;
            }

			currentSongNumber -= 1;

			if (currentSongNumber < 0)
			{
				currentSongNumber = musicTracks.Count - 1;
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

        private void AllMusicTracksColumn_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                AllMusicTracks.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            AllMusicTracks.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));

        }

        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lblStatus.Content = $"{(int)mediaPlayer.Position.TotalMinutes}:{mediaPlayer.Position.Seconds:00}/{(int)mediaPlayer.NaturalDuration.TimeSpan.TotalMinutes}:{mediaPlayer.NaturalDuration.TimeSpan.Seconds:00}";
        }

        private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            isDraggingSlider = false;
            mediaPlayer.Position = TimeSpan.FromSeconds(sliProgress.Value);
        }

        private void sliProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            isDraggingSlider = true;
        }

        private void AllMusicTracks_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            mediaPlayer.Volume += (e.Delta > 0) ? 0.1 : -0.1;
        }
    }
}
