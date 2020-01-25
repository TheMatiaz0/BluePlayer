using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Serialization;

namespace BluePlayer
{
    [XmlRoot("Music")]
    public class MusicTrack
    {
        public MusicTrack(int id, string artist, string songName, string path, TimeSpan duration, string albumTitle = null, uint? trackYear = null, ImageSource albumArt = null, string extension = ".mp3")
        {
			ID = id;
            Artist = artist;
            SongName = songName;
			Path = path;
			Duration = duration;
			AlbumArt = albumArt;
			AlbumTitle = albumTitle;
			Extension = extension;
			TrackYear = trackYear;
        }

        public MusicTrack()
        {

        }

		public int ID { get; }
        public string Artist { get; }
        public string SongName { get; }
		public string Path { get; set; }
		public string Extension { get; }

		public uint? TrackYear { get; }

		public TimeSpan Duration { get; }

		public ImageSource AlbumArt { get; }
		public string AlbumTitle { get; }
    }
}
