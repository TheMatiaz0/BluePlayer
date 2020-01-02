using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer
{
    public class MusicTrack
    {
        public MusicTrack(int id, string artist, string songName, string path, string extension = "MP3")
        {
			ID = id;
            Artist = artist;
            SongName = songName;
			Path = path;
			Extension = extension;
        }

		public int ID { get; }
        public string Artist { get; set; }
        public string SongName { get; set; }
		public string Path { get; }
		public string Extension { get; }
    }
}
