using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

public static class MediaPlayerExtension
{
    public static void PlayWithPause(this MediaPlayer mediaPlayer, ref bool isPlaying)
    {
        if (!isPlaying)
        {
            mediaPlayer.Play();
            isPlaying = true;
        }

        else
        {
            mediaPlayer.Pause();
            isPlaying = false;
        }

    }
}
