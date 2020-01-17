using BluePlayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

[Serializable]
public class Playlist
{
    [XmlArray("Playlist"), XmlArrayItem(typeof(MusicTrack), ElementName = "MusicTrack")]
    public List<MusicTrack> musicTracks = new List<MusicTrack>();

    public Playlist()
    {

    }

}
