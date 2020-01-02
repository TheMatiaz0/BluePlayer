using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer
{
    public class MusicTrack
    {
        public MusicTrack(string artist, string songName)
        {
            Artist = artist;
            SongName = songName;
        }

        public string Artist { get; }
        public string SongName { get; set; }
    }
}
