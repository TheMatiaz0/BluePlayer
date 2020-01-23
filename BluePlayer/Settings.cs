using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePlayer
{
	public class Settings
	{
		public double Volume { get; set; }
		public bool IsRandomized { get; set; }
		public bool IsLooped { get; set; }

		public Settings ()
		{

		}

		public Settings(double volume, bool isRandomized, bool isLooped)
		{
			Volume = volume;
			IsRandomized = isRandomized;
			IsLooped = isLooped;
		}

		public Settings (MainPlayer player)
		{
			Volume = player.MusicPlayer.Volume;
			IsRandomized = player.IsRandomized;
			IsLooped = player.IsLooped;
		}
	}
}
