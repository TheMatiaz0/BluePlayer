﻿using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace BluePlayer
{
	public class MainPlayer
	{
		public MediaPlayer MusicPlayer { get; } = new MediaPlayer();

		public bool IsPlaying { get; private set; }
		public bool IsLooped { get; private set; }
		public bool IsRandomized { get; private set; }

		public int CurrentSongNumber { get; private set; }
		public ObservableCollection<MusicTrack> MusicTracks { get; set; } = new ObservableCollection<MusicTrack>();

		public event EventHandler<SimpleArgs<bool>> OnPlaySwitch = delegate { };
		public event EventHandler<SimpleArgs<bool>> OnLoopSwitch = delegate { };
		public event EventHandler<SimpleArgs<bool>> OnRandomizeSwitch = delegate { };

		public MusicTrack GetCurrentSong()
		{
			if (MusicTracks.Count <= 0)
			{
				return null;
			}

			return MusicTracks[CurrentSongNumber];
		}

		public void SetupForLoad(Settings settings, System.Windows.Controls.Slider slider)
		{
			MusicPlayer.Volume = settings.Volume;
			slider.Value = MusicPlayer.Volume;
			IsLooped = settings.IsLooped;
			IsRandomized = settings.IsRandomized;
			OnLoopSwitch.Invoke(this, IsLooped);
			OnRandomizeSwitch.Invoke(this, IsRandomized);
		}

		public void ChangeLoop()
		{
			IsLooped = !IsLooped;
			OnLoopSwitch.Invoke(this, IsLooped);
		}

		public void ChangeRandomization()
		{
			IsRandomized = !IsRandomized;
			OnRandomizeSwitch.Invoke(this, IsRandomized);
		}

		public void PlaySameMusic(bool shouldPlay = true)
		{
			ChangeMusicTrack(GetCurrentSong(), shouldPlay);
		}

		public void PlaySelectedMusicTrack(MusicTrack track)
		{
			CurrentSongNumber = track.ID;
			ChangeMusicTrack(track, true);
		}

		public void Skip()
		{
			if (MusicTracks.Count <= 0)
			{
				return;
			}

			if (IsRandomized)
			{
				PlayRandomTrack();
				return;
			}

			CurrentSongNumber += 1;

			if (CurrentSongNumber > MusicTracks.Count - 1)
			{
				CurrentSongNumber = 0;
			}

			ChangeMusicTrack(GetCurrentSong(), true);
		}

		public void Back()
		{
			if (MusicTracks.Count <= 0)
			{
				return;
			}

			if (IsRandomized)
			{
				PlayRandomTrack();
				return;
			}

			CurrentSongNumber -= 1;

			if (CurrentSongNumber < 0)
			{
				CurrentSongNumber = MusicTracks.Count - 1;
			}
			ChangeMusicTrack(GetCurrentSong(), true);
		}


		/// <summary>
		/// Switch between Pause and Play.
		/// </summary>
		public void SwitchPlayPause()
		{
			IsPlaying = MusicPlayer.PlayWithPause(IsPlaying);
			OnPlaySwitch.Invoke(this, IsPlaying);
		}

		private void MediaPlayer_MediaEnded(object sender, EventArgs e)
		{
			TryLoop();
		}

		private void TryLoop()
		{
			if (!IsLooped)
			{
				Skip();
			}

			else
			{
				PlaySameMusic();
			}
		}

		public MainPlayer()
		{
			MusicPlayer.MediaEnded += MediaPlayer_MediaEnded;
		}

		private void PlayRandomTrack()
		{
			int index = MainWindow.RND.Next(MusicTracks.Count);
			CurrentSongNumber = index;
			ChangeMusicTrack(MusicTracks[index], true);
		}

		public void ChangeMusicTrack(MusicTrack music, bool shouldPlay = false)
		{
			if (music == null)
			{
				return;
			}

			MusicPlayer.Open(new Uri(music.Path));

			MainWindow.albumArtPlace.Source = GetImageFromAlbumArtProperty(music.Path);
			MainWindow.currentSongPlaying.Text = music.AlbumTitle;

			if (shouldPlay)
			{
				MusicPlayer.Play();
				IsPlaying = true;
				OnPlaySwitch.Invoke(this, IsPlaying);
			}
		}

		public ImageSource GetImageFromAlbumArtProperty(string path)
		{
			ShellObject shellFile = ShellObject.FromParsingName(path);
			Bitmap thumbnailFromShell = shellFile.Thumbnail.LargeBitmap;
			return thumbnailFromShell.BitmapToImageSource();

		}
	}
}
