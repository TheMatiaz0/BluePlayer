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

namespace MusicPlayer
{
	public partial class MainWindow : Window
	{
		public ObservableCollection<MusicTrack> musicTracks = new ObservableCollection<MusicTrack>();
		private readonly MediaPlayer mediaPlayer = new MediaPlayer();

		public static Random RND = new Random(); 

		private bool isPlaying;
		private bool isDraggingSlider;
		private bool isLooped = false;
		private bool isRandomized = false;

		public string pathToFirstSong;

		private int currentSongNumber = 0;

		private GridViewColumnHeader listViewSortCol = null;
		private SortAdorner listViewSortAdorner = null;

		public MainWindow()
		{
			InitializeComponent();
			_ = Update();

			mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
			mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;

			if (!string.IsNullOrWhiteSpace(pathToFirstSong))
			{
				_ = AddFileAsync(pathToFirstSong);
			}

			ClearPlaylist();
		}

		private void TryLoop()
		{
			if (!isLooped)
			{
				// BtnSkip.Foreground = Brushes.Purple;
				Skip();
			}

			else
			{
				// BtnSkip.Foreground = Brushes.LightPink;
				PlaySameMusic();
			}
		}


		private void MediaPlayer_MediaEnded(object sender, EventArgs e)
		{
			TryLoop();
		}

		private void MediaPlayer_MediaOpened(object sender, EventArgs e)
		{
			ClipProgress.Minimum = 0;
			ClipProgress.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
		}

		private async Task Update()
		{
			while (true)
			{
				await Task.Delay(TimeSpan.FromMilliseconds(500));


				if (mediaPlayer.Source != null && mediaPlayer.NaturalDuration.HasTimeSpan)
				{
					if (isDraggingSlider == false)
					{
						ClipProgress.Value = mediaPlayer.Position.TotalSeconds;
					}
					lblStatus.Content = $"{(int)mediaPlayer.Position.TotalMinutes}:{mediaPlayer.Position.Seconds:00}/{(int)mediaPlayer.NaturalDuration.TimeSpan.TotalMinutes}:{mediaPlayer.NaturalDuration.TimeSpan.Seconds:00}";
				}
			}
		}

		private void BtnPlay_Click(object sender, RoutedEventArgs e)
		{
			mediaPlayer.PlayWithPause(ref isPlaying);
		}
		private void ChangeMusicTrack(MusicTrack music, bool shouldPlay = false)
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

		private void GetFileBtn_Click(object sender, RoutedEventArgs e)
		{
			CommonOpenFileDialog dialog = new CommonOpenFileDialog
			{
				IsFolderPicker = true
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
			FindAndAddFiles(PathInput.Text);
		}

		public void FindAndAddFiles(string input)
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
				ClearPlaylist();

				foreach (string path in foundSoundFiles)
				{
					AddFile(path);
				}

				try
				{
					ChangeMusicTrack(musicTracks[currentSongNumber]);
				}

				catch (Exception f)
				{
					MessageBox.Show(f.Message);
				}
			}

		}

		public async Task AddFileAsync(string path)
		{
			await Task.Delay(TimeSpan.FromSeconds(3));
			AddFile(path);
		}

		public void AddFile(string path)
		{
			if (!File.Exists(path))
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
					builder.Append($"{creatorName},");
				}

				builder.Remove(builder.Length - 1, 1);
				finalCreator = builder.ToString();
			}

			else
			{
				finalCreator = "NaN";
			}


			musicTracks.Add(new MusicTrack(musicTracks.Count, finalCreator, songName, path, System.IO.Path.GetExtension(path)));
		}

		private void PlaySameMusic()
		{
			ChangeMusicTrack(musicTracks[currentSongNumber], true);
		}

		private void Skip()
		{
			if (musicTracks.Count <= 0)
			{
				return;
			}

			if (isRandomized)
			{
				PlayRandomTrack();
				return;
			}

			currentSongNumber += 1;

			if (currentSongNumber > musicTracks.Count - 1)
			{
				currentSongNumber = 0;
			}

			ChangeMusicTrack(musicTracks[currentSongNumber], true);
		}

		private void Back ()
		{
			if (musicTracks.Count <= 0)
			{
				return;
			}

			if (isRandomized)
			{
				PlayRandomTrack();
				return;
			}

			currentSongNumber -= 1;

			if (currentSongNumber < 0)
			{
				currentSongNumber = musicTracks.Count - 1;
			}
			ChangeMusicTrack(musicTracks[currentSongNumber], true);
		}

		private void BtnSkip_Click(object sender, RoutedEventArgs e)
		{
			Skip();
		}

		private void BtnBack_Click(object sender, RoutedEventArgs e)
		{
			Back();
		}

		private void AllMusicTracks_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (((FrameworkElement)e.OriginalSource).DataContext is MusicTrack item)
			{
				currentSongNumber = item.ID;
				ChangeMusicTrack(item, true);
			}
		}

		private void AllMusicTracks_Drop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] foundSoundFiles = CheckSoundFiles((string[])e.Data.GetData(DataFormats.FileDrop));


				foreach (string path in foundSoundFiles)
				{
					AddFile(path);
				}
			}
		}

		private string[] CheckSoundFiles (string[] paths)
		{
			return (from item in paths
					let ext = System.IO.Path.GetExtension(item)
					where ext == ".mp3" || ext == ".wav"
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
			mediaPlayer.Position = TimeSpan.FromSeconds(ClipProgress.Value);
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

			foreach (MusicTrack item in musicTracks)
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
				FilterIndex = 2
			};

			if (saveFileDialog.ShowDialog() == true)
			{
				SavePlaylist(saveFileDialog.FileName);
			}
		}

		private void LoadPlaylist(string path)
		{
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
				Multiselect = true
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
			mediaPlayer.Volume = e.NewValue;
		}

		private void BtnLoop_Click(object sender, RoutedEventArgs e)
		{
			isLooped = !isLooped;
		}

		private void MenuItem_Click_1(object sender, RoutedEventArgs e)
		{
			if (AllMusicTracks.SelectedIndex == -1)
			{
				return;
			}

			// dodać sprawdzanie usuwania - np. jeśli usuwasz track to zmieniaj currentTrack i zmień nutę.
			RemoveMusicTrack(AllMusicTracks.SelectedIndex);
		}

		private void RemoveMusicTrack (int index)
		{
			musicTracks.RemoveAt(index);
		}

		private void ClearPlaylistMenuBtn_Click(object sender, RoutedEventArgs e)
		{
			ClearPlaylist();
		}

		private void ClearPlaylist ()
		{
			musicTracks = new ObservableCollection<MusicTrack>();

			AllMusicTracks.ItemsSource = musicTracks;

			DataContext = this;
		}

		private void BtnRandom_Click(object sender, RoutedEventArgs e)
		{
			isRandomized = !isRandomized;
		}

		private void PlayRandomTrack ()
		{
			int index = RND.Next(musicTracks.Count + 1);
			ChangeMusicTrack(musicTracks[index], true);
		}
	}
}
