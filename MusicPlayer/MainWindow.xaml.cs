using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MusicPlayer
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<MusicTrack> musicTracks = new ObservableCollection<MusicTrack>();

        public MainWindow()
        {
            InitializeComponent();

            musicTracks = new ObservableCollection<MusicTrack>() { new MusicTrack("Arctic Monkeys", "I Wanna Be Yours"), new MusicTrack("Radiohead", "Creep") };

            musicTracks.Add(new MusicTrack("Test", "Just a Test"));

            AllMusicTracks.ItemsSource = musicTracks;

            DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            musicTracks.Add(new MusicTrack("Trine", "Eat my wine"));
        }
    }
}
