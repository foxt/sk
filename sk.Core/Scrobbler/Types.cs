using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace sk
{
    public enum PlayerState {
        Playing,
        Paused,
        Stopped
    }
    public class PlayerTrack {
        [JsonProperty("id")]
        public string? ID { get; set; }
        [JsonProperty("title")]
        public string? Title { get; set; }
        [JsonProperty("artist")]
        public string? Artist { get; set; }
        [JsonProperty("album")]
        public string? Album { get; set; }
        [JsonProperty("albumArtist")]
        public string? AlbumArtist { get; set; }
        [JsonProperty("duration")]
        public int Duration { get; set; }

    }
    public class OnStateChangedArgs : EventArgs { public PlayerState State { get; set; } }
    public class OnTrackChangedArgs : EventArgs { public PlayerTrack? Track { get; set; } }
    public class OnPlayerPositionChangedArgs : EventArgs { public int Position { get; set; } }
    public class OnScrobbleArgs : EventArgs { public PlayerTrack? Track { get; set; } public long Time { get; set; } }
    public class OnNowPlayingArgs : EventArgs { public PlayerTrack? Track { get; set; } }
    public class OnAuthRequiredArgs : EventArgs { public string? Url { get; set; } public Func<string,Task<bool>>? Callback { get; set; } }

}