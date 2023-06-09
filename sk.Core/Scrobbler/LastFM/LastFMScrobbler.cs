using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace sk
{
    public class SkLastFMScrobbler {
        public SkScrobblerCore scrobbler;
        public LastFMAPI api = new LastFMAPI();
        public SkLastFMScrobbler() {
            scrobbler = new SkScrobblerCore();
            scrobbler.OnNowPlaying += OnNowPlaying;
            scrobbler.OnScrobble += OnScrobble;
        }
        private async void OnNowPlaying(object? sender, OnNowPlayingArgs e) {
            if (e.Track == null) return;
            await api.MakePOST("track.updateNowPlaying", true, new Dictionary<string, string>() {
                {"artist", e.Track.Artist!},
                {"track", e.Track.Title!},
                {"album", e.Track.Album!},
                {"duration", e.Track.Duration.ToString()},
                {"albumArtist", e.Track.AlbumArtist!},
            });
        }
        private async void OnScrobble(object? sender, OnScrobbleArgs e) {
            if (e.Track == null) return;
            await api.MakePOST("track.scrobble", true, new Dictionary<string, string>() {
                {"artist", e.Track.Artist!},
                {"track", e.Track.Title!},
                {"timestamp", e.Time.ToString()},
                {"album", e.Track.Album!},
                {"duration", e.Track.Duration.ToString()},
                {"albumArtist", e.Track.AlbumArtist!},
            });
        }



        public bool IsLoggedIn {
            get => api.SessionKey != null;
        }
        
    }
}