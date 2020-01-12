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
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<MusicTrack> musicTracks = new ObservableCollection<MusicTrack>();
        private readonly MediaPlayer mediaPlayer = new MediaPlayer();
        // private int maxSongs = 0;
        private bool isPlaying;
        private bool isDraggingSlider;
        private bool isLooped = false;

        private int currentSongNumber = 0;

        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;

        public MainWindow()
        {
            InitializeComponent();
            _ = Update();

            AllMusicTracks.ItemsSource = musicTracks;

            mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;

            DataContext = this;
        }

        private void MediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            if (!isLooped)
            {
                Skip();
            }
            else
            {
                PlaySameMusic();
            }

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

            foreach (string item in foundSoundFiles)
            {
                AddFile(item);
            }
        }

        private void AddFile(string path)
        {
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

				foreach (string item2 in creators)
				{
					builder.Append($"{item2},");
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
            UpdateMusic(musicTracks[currentSongNumber], true);
        }

        private void Skip()
        {
            if (musicTracks.Count <= 0)
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

        private void BtnSkip_Click(object sender, RoutedEventArgs e)
        {
            if (!isLooped)
            {
                Skip();
            }

            else
            {
                PlaySameMusic();
            }

        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (musicTracks.Count <= 0)
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
                foreach (string item in files)
                {
                    AddFile(item);
                }
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

        private void ClipProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TimeSpan x = TimeSpan.FromSeconds(e.NewValue);
            ClipProgress.ToolTipContent = $"{(int)x.Minutes}:{x.Seconds:00}";

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

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            Playlist playlist = new Playlist
            {
                musicTracks = new List<MusicTrack>()
            };

            foreach (MusicTrack item in musicTracks)
            {
                playlist.musicTracks.Add(new MusicTrack(item.ID, item.Artist, item.SongName, item.Path, item.Extension));
            }

            XmlSerializer xsl = new XmlSerializer(typeof(Playlist));
            using (FileStream fs = new FileStream(@"D:\test.xml", FileMode.Create))
            {
                xsl.Serialize(fs, playlist);
            }
        }

        private void LoadBtn_Click(object sender, RoutedEventArgs e)
        {
			XmlSerializer xsl = new XmlSerializer(typeof(Playlist));
            StreamReader stream = new StreamReader(@"D:\test.xml");

			Playlist playlist = (Playlist)xsl.Deserialize(stream);
			stream.Close();

            foreach (MusicTrack item in playlist.musicTracks)
            {
                AddFile(item.Path);
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.Volume = e.NewValue;
        }

        private void BtnLoop_Click(object sender, RoutedEventArgs e)
        {
            isLooped = !isLooped;
        }
    }
}
