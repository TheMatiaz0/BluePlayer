using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

public static class MediaPlayerExtension
{
    public static bool PlayWithPause(this MediaPlayer mediaPlayer, bool isPlaying)
    {
        if (!isPlaying)
        {
            mediaPlayer.Play();
            return true;
        }

        else
        {
            mediaPlayer.Pause();
            return false;
        }

    }
}
