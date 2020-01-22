using MaterialDesignThemes.Wpf;
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
using System.Xml.Serialization;

namespace BluePlayer
{
	public partial class MainWindow : Window
	{
		public MainPlayer MusicController { get; } = new MainPlayer();

		public static Random RND = new Random();

		private bool isDraggingSlider;

		private GridViewColumnHeader listViewSortCol = null;
		private SortAdorner listViewSortAdorner = null;

		public MainWindow()
		{
			InitializeComponent();
			_ = Update();

			MusicController.MusicPlayer.MediaOpened += MediaPlayer_MediaOpened;
			MusicController.OnPlaySwitch += MusicController_OnPlaySwitch;
			MusicController.OnLoopSwitch += MusicController_OnLoopSwitch;
			MusicController.OnRandomizeSwitch += MusicController_OnRandomizeSwitch;	

			ClearPlaylist();

			Application.Current.Resources["playPauseIcon"] = PackIconKind.Play;
			Application.Current.Resources["playBtnToolTip"] = "Play";
		}

		private void MusicController_OnRandomizeSwitch(object sender, SimpleArgs<bool> e)
		{
			SetVisibility(randomEnableDot, e.Value);
		}

		private void MusicController_OnLoopSwitch(object sender, SimpleArgs<bool> e)
		{
			SetVisibility(loopEnableDot, e.Value);
		}

		private void MusicController_OnPlaySwitch(object sender, SimpleArgs<bool> e)
		{
			CheckPlayStatus(e.Value);
		}

		private void MediaPlayer_MediaOpened(object sender, EventArgs e)
		{
			ClipProgress.Minimum = 0;
			ClipProgress.Maximum = MusicController.MusicPlayer.NaturalDuration.TimeSpan.TotalSeconds;
		}


		private async Task Update()
		{
			while (true)
			{

				await Task.Delay(TimeSpan.FromMilliseconds(500));


				if (MusicController.MusicPlayer.Source != null && MusicController.MusicPlayer.NaturalDuration.HasTimeSpan)
				{
					if (isDraggingSlider == false)
					{
						ClipProgress.Value = MusicController.MusicPlayer.Position.TotalSeconds;
					}
					lblStatus.Content = $"{(int)MusicController.MusicPlayer.Position.TotalMinutes}:{MusicController.MusicPlayer.Position.Seconds:00}/{(int)MusicController.MusicPlayer.NaturalDuration.TimeSpan.TotalMinutes}:{MusicController.MusicPlayer.NaturalDuration.TimeSpan.Seconds:00}";
				}
			}
		}

		private void BtnPlay_Click(object sender, RoutedEventArgs e)
		{
			MusicController.SwitchPlayPause();		
		}

		private void CheckPlayStatus(bool isPlaying)
		{
			if (isPlaying)
			{
				Application.Current.Resources["playPauseIcon"] = PackIconKind.Pause;
				Application.Current.Resources["playBtnToolTip"] = "Pause";
			}
			else
			{
				Application.Current.Resources["playPauseIcon"] = PackIconKind.Play;
				Application.Current.Resources["playBtnToolTip"] = "Play";
			}
		}

		private void GetFileBtn_Click(object sender, RoutedEventArgs e)
		{
			CommonOpenFileDialog dialog = new CommonOpenFileDialog
			{
				IsFolderPicker = true,
				Title = "Select path to a folder that contains a bunch of songs (.mp3, .m4a, .wav)"
			};
			CommonFileDialogResult result = dialog.ShowDialog();

			if (result == CommonFileDialogResult.Ok)
			{
				PathInput.Text = dialog.FileName;
			}

			dialog.Dispose();
		}

		private void OkBtn_Click(object sender, RoutedEventArgs e)
		{
			_ = FindAndAddFiles(PathInput.Text);
		}

		public async Task FindAndAddFiles(string input)
		{
			string[] foundSoundFiles = null;

			try
			{
				foundSoundFiles = CheckSoundFiles(Directory.GetFiles(input, "*", SearchOption.AllDirectories));
			}

			catch (Exception f)
			{
				MessageBox.Show(f.Message);
			}

			if (foundSoundFiles != null)
			{
				// ClearPlaylist();

				foreach (string path in foundSoundFiles)
				{
					await Task.Delay(50);
					AddFile(path);
				}

				try
				{
					MusicController.PlaySameMusic(false);
				}

				catch (Exception f)
				{
					MessageBox.Show(f.Message);
				}
			}

		}
		public void AddFile(string path)
		{
			if (!File.Exists(path))
			{
				return;
			}

			if (MusicController.MusicTracks.Any(item => item.Path == path) == true)
			{
				return;
			}

			ShellObject shellFile = ShellObject.FromParsingName(path);
			string[] creators = PropertyHandler.GetValues(shellFile.Properties.GetProperty(SystemProperties.System.Music.Artist));
			string songName = PropertyHandler.GetValue(shellFile.Properties.GetProperty(SystemProperties.System.Title));

			if (string.IsNullOrEmpty(songName))
			{
				int pos = path.LastIndexOf("\\") + 1;
				string result = new string(path.Skip(pos).ToArray());

				songName = result.Replace(System.IO.Path.GetExtension(result), "");
			}

			string finalCreator;
			if (creators != null)
			{
				StringBuilder builder = new StringBuilder();

				foreach (string creatorName in creators)
				{
					builder.Append($"{creatorName}, ");
				}

				builder.Remove(builder.Length - 2, 1);
				finalCreator = builder.ToString();
			}

			else
			{
				finalCreator = "NaN";
			}


			MusicController.MusicTracks.Add(new MusicTrack(MusicController.MusicTracks.Count, finalCreator, songName, path, System.IO.Path.GetExtension(path)));
		}

		private void BtnSkip_Click(object sender, RoutedEventArgs e)
		{
			MusicController.Skip();
		}

		private void BtnBack_Click(object sender, RoutedEventArgs e)
		{
			MusicController.Back();
		}

		private void AllMusicTracks_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (((FrameworkElement)e.OriginalSource).DataContext is MusicTrack item)
			{
				MusicController.PlaySelectedMusicTrack(item);
			}
		}

		private void AllMusicTracks_Drop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] foundSoundFiles = CheckSoundFiles((string[])e.Data.GetData(DataFormats.FileDrop));
				string[] playlistFiles = CheckPlaylist((string[])e.Data.GetData(DataFormats.FileDrop));

				foreach (string path in foundSoundFiles)
				{
					AddFile(path);
				}

				foreach (string path in playlistFiles)
				{
					LoadPlaylist(path);
				}
			}
		}

		public string[] CheckSoundFiles(string[] paths)
		{
			return (from item in paths
					let ext = System.IO.Path.GetExtension(item)
					where ext == ".mp3" || ext == ".wav" || ext == ".m4a"
					select item).ToArray();
		}

		public string[] CheckPlaylist(string[] paths)
		{
			return (from item in paths
					let ext = System.IO.Path.GetExtension(item)
					where ext == ".playlist"
					select item).ToArray();
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

		private void ClipProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			TimeSpan previewTime = TimeSpan.FromSeconds(e.NewValue);
			ClipProgress.ToolTipContent = $"{(int)previewTime.Minutes}:{previewTime.Seconds:00}";

		}

		private void ClipProgress_DragCompleted(object sender, DragCompletedEventArgs e)
		{
			isDraggingSlider = false;
			MusicController.MusicPlayer.Position = TimeSpan.FromSeconds(ClipProgress.Value);
		}

		private void ClipProgress_DragStarted(object sender, DragStartedEventArgs e)
		{
			isDraggingSlider = true;
		}

		private void SavePlaylist(string path)
		{
			Playlist playlist = new Playlist
			{
				musicTracks = new List<MusicTrack>()
			};

			foreach (MusicTrack item in MusicController.MusicTracks)
			{
				playlist.musicTracks.Add(new MusicTrack(item.ID, item.Artist, item.SongName, item.Path, item.Extension));
			}

			SerializationXML.SaveFile<Playlist>(path, playlist);
		}

		private void SaveBtn_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog
			{
				Filter = "Playlist files (*.playlist)| *.playlist",
				RestoreDirectory = true,
				Title = "Browse XML playlist files",
				FilterIndex = 2,
				FileName = "BluePlayer_Playlist"
			};

			if (saveFileDialog.ShowDialog() == true)
			{
				SavePlaylist(saveFileDialog.FileName);
			}
		}

		public void LoadPlaylist(string path)
		{
			if (!File.Exists(path))
			{
				return;
			}

			StreamReader stream = new StreamReader(path);

			Playlist playlist = (Playlist)SerializationXML.LoadFile<Playlist>(stream);
			stream.Close();

			foreach (MusicTrack item in playlist.musicTracks)
			{
				AddFile(item.Path);
			}
		}

		private void LoadBtn_Click(object sender, RoutedEventArgs e)
		{
			CommonOpenFileDialog loadFileDialog = new CommonOpenFileDialog
			{
				Multiselect = true,
				DefaultFileName = "BluePlayer_Playlist"
			};
			CommonFileDialogFilter filter = new CommonFileDialogFilter("Playlist files", "*.playlist");
			loadFileDialog.Filters.Add(filter);

			CommonFileDialogResult result = loadFileDialog.ShowDialog();

			if (result == CommonFileDialogResult.Ok)
			{
				foreach (string item in loadFileDialog.FileNames)
				{
					LoadPlaylist(item);
				}
			}

			loadFileDialog.Dispose();
		}

		private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			MusicController.MusicPlayer.Volume = e.NewValue;
		}

		private void BtnLoop_Click(object sender, RoutedEventArgs e)
		{
			MusicController.ChangeLoop();
		}

		private void SetVisibility(PackIcon packIcon, bool isTrue)
		{
			if (isTrue)
			{
				packIcon.Visibility = Visibility.Visible;
			}

			else
			{
				packIcon.Visibility = Visibility.Collapsed;
			}
		}

		private void MenuItem_Click_1(object sender, RoutedEventArgs e)
		{
			MusicController.MusicPlayer.Stop();
			MusicController.MusicPlayer.Open(null);
			if (AllMusicTracks.SelectedIndex == -1)
			{
				return;
			}
			MusicController.MusicTracks.RemoveAt(AllMusicTracks.SelectedIndex);
		}

		private void ClearPlaylistMenuBtn_Click(object sender, RoutedEventArgs e)
		{
			MusicController.MusicPlayer.Stop();
			MusicController.MusicPlayer.Open(null);
			ClearPlaylist();
		}

		private void ClearPlaylist()
		{
			MusicController.MusicTracks = new ObservableCollection<MusicTrack>();

			AllMusicTracks.ItemsSource = MusicController.MusicTracks;

			DataContext = this;
		}

		private void BtnRandom_Click(object sender, RoutedEventArgs e)
		{
			MusicController.ChangeRandomization();
		}



		private void GitHubBtn_Click(object sender, RoutedEventArgs e)
		{
			System.Diagnostics.Process.Start("https://github.com/TheMatiaz0/BluePlayer");
		}

		private void MailBtn_Click(object sender, RoutedEventArgs e)
		{
			System.Diagnostics.Process.Start("mailto:TheMatiaz0@protonmail.com?subject=Subject&amp;body=Test");
		}

		private void ThumbnailPlayBtn_Click(object sender, EventArgs e)
		{
			MusicController.SwitchPlayPause();
		}
	}
}
