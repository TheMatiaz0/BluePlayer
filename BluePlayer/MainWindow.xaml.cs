using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
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

		#region Global Hotkey for Media keys

		[DllImport("user32.dll")]
		private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

		[DllImport("user32.dll")]
		private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

		private const int HOTKEY_ID = 9000;

		private const uint MOD_NONE = 0x0000;
		private const uint VK_MEDIA_PLAY_PAUSE = 0xB3;
		private const uint VK_MEDIA_NEXT_TRACK = 0xB0;
		private const uint VK_MEDIA_PREV_TRACK = 0xB1;

		private IntPtr _windowHandle;
		private HwndSource _source;

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			_windowHandle = new WindowInteropHelper(this).Handle;
			_source = HwndSource.FromHwnd(_windowHandle);
			_source.AddHook(HwndHook);

			RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_NONE, VK_MEDIA_PLAY_PAUSE);
			RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_NONE, VK_MEDIA_NEXT_TRACK);
			RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_NONE, VK_MEDIA_PREV_TRACK);
		}

		private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			const int WM_HOTKEY = 0x0312;
			switch (msg)
			{
				case WM_HOTKEY:
					switch (wParam.ToInt32())
					{
						case HOTKEY_ID:
							int vkey = (((int)lParam >> 16) & 0xFFFF);

							switch ((uint)vkey)
							{
								case VK_MEDIA_PLAY_PAUSE:
									MusicController.SwitchPlayPause();
									break;

								case VK_MEDIA_NEXT_TRACK:
									MusicController.Skip();
									break;

								case VK_MEDIA_PREV_TRACK:
									MusicController.Back();
									break;
							}

							handled = true;
							break;
					}
					break;
			}
			return IntPtr.Zero;
		}

		protected override void OnClosed(EventArgs e)
		{
			_source.RemoveHook(HwndHook);
			UnregisterHotKey(_windowHandle, HOTKEY_ID);
			base.OnClosed(e);
		}

		#endregion


		public string PathToSettings => $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/Settings.cfg";

		public static System.Windows.Controls.Image albumArtPlace;
		public static TextBlock currentSongPlaying;

		public MainPlayer MusicController { get; } = new MainPlayer();

		public static Random RND = new Random();

		private bool isDraggingSlider;

		private GridViewColumnHeader listViewSortCol = null;
		private SortAdorner listViewSortAdorner = null;

		public MainWindow()
		{
			InitializeComponent();
			_ = Update();

			albumArtPlace = AlbumArtImg;
			currentSongPlaying = PlayingSongName;

			MusicController.MusicPlayer.MediaOpened += MediaPlayer_MediaOpened;
			MusicController.OnPlaySwitch += MusicController_OnPlaySwitch;
			MusicController.OnLoopSwitch += MusicController_OnLoopSwitch;
			MusicController.OnRandomizeSwitch += MusicController_OnRandomizeSwitch;

			if (File.Exists(PathToSettings))
			{
				Settings settings = (Settings)SerializationXML.LoadFile<Settings>(PathToSettings);
				MusicController.SetupForLoad(settings, VolumeSlider);
			}

			ClearPlaylist();
			CheckPlayStatus(false);
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
			CheckPlayStatusForThumbnail(e.Value);
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
					lblStatus.Content = $"Playing: {MusicController.GetCurrentSong().Artist} - {MusicController.GetCurrentSong().SongName} | {(int)MusicController.MusicPlayer.Position.TotalMinutes}:{MusicController.MusicPlayer.Position.Seconds:00}/{(int)MusicController.MusicPlayer.NaturalDuration.TimeSpan.TotalMinutes}:{MusicController.MusicPlayer.NaturalDuration.TimeSpan.Seconds:00}";
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

		private void CheckPlayStatusForThumbnail(bool isPlaying)
		{
			MusicTrack currentTrack = MusicController.GetCurrentSong();

			if (isPlaying)
			{
				ThumbnailPlayBtn.ImageSource = new BitmapImage(new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, @"Graphics\pause.png")));
				ThumbnailPlayBtn.Description = "Pause";

				if (currentTrack != null)
				{
					this.Title = $"{currentTrack.Artist} - {currentTrack.SongName}";
				}
			}

			else
			{
				ThumbnailPlayBtn.ImageSource = new BitmapImage(new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, @"Graphics\right.png")));
				ThumbnailPlayBtn.Description = "Play";
				this.Title = $"BluePlayer";
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
			string albumName = PropertyHandler.GetValue(shellFile.Properties.GetProperty(SystemProperties.System.Music.AlbumTitle));
			uint? trackYear = PropertyHandler.GetNumber(shellFile.Properties.GetProperty(SystemProperties.System.Media.Year));
			TimeSpan songDuration = PropertyHandler.GetDuration(shellFile.Properties.GetProperty(SystemProperties.System.Media.Duration));

			Bitmap thumbnailFromShell = shellFile.Thumbnail.LargeBitmap;
			ImageSource albumArt = thumbnailFromShell.BitmapToImageSource();


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


			MusicController.MusicTracks.Add(new MusicTrack(MusicController.MusicTracks.Count, finalCreator, songName, path, songDuration, albumName, trackYear, albumArt, System.IO.Path.GetExtension(path)));
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

				_ = AddFiles(foundSoundFiles);

				foreach (string path in playlistFiles)
				{
					_ = LoadPlaylist(path);
				}
			}
		}

		private async Task AddFiles(string[] paths)
		{
			foreach (string path in paths)
			{
				await Task.Delay(50);
				AddFile(path);
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
				playlist.musicTracks.Add(item);
			}

			SerializationXML.SaveFile(path, playlist);
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

		public async Task LoadPlaylist(string path)
		{
			if (!File.Exists(path))
			{
				return;
			}

			Playlist playlist = (Playlist)SerializationXML.LoadFile<Playlist>(path);

			foreach (MusicTrack item in playlist.musicTracks)
			{
				await Task.Delay(50);
				AddFile(item.Path);
			}
		}

		private void LoadBtn_Click(object sender, RoutedEventArgs e)
		{
			CommonOpenFileDialog loadFileDialog = new CommonOpenFileDialog
			{
				Multiselect = true,
				DefaultFileName = "BluePlayer_Playlist.playlist"
			};
			CommonFileDialogFilter filter = new CommonFileDialogFilter("Playlist files", "*.playlist");
			loadFileDialog.Filters.Add(filter);

			CommonFileDialogResult result = loadFileDialog.ShowDialog();

			if (result == CommonFileDialogResult.Ok)
			{
				foreach (string item in loadFileDialog.FileNames)
				{
					_ = LoadPlaylist(item);
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
			System.Diagnostics.Process.Start("mailto:TheMatiaz0@protonmail.com?subject=Subject&amp;body=Typesomethinghere");
		}

		private void ThumbnailPlayBtn_Click(object sender, EventArgs e)
		{
			MusicController.SwitchPlayPause();
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			SerializationXML.SaveFile(PathToSettings, new Settings(MusicController));
		}

		private void ThumbnailPreviousBtn_Click(object sender, EventArgs e)
		{
			MusicController.Back();
		}

		private void ThumbnailNextBtn_Click(object sender, EventArgs e)
		{
			MusicController.Skip();
		}
	}
}
