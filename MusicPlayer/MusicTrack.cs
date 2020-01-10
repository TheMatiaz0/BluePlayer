using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MusicPlayer
{
    [XmlRoot("Music")]
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

        public MusicTrack()
        {

        }

		public int ID { get; }
        public string Artist { get; }
        public string SongName { get; }
		public string Path { get; set; }
		public string Extension { get; }
    }
}
