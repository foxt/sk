using System;
using sk.Players.Generic;
namespace sk
{

    public class SkScrobblerCore
    {

        // If the song goes back to the first X seconds, it will be scrobbled again.
        public int GracePeriod = 5;
        // Song % at which it will be scrobbled.
        public float ScrobblePercent = 0.5f;
        // or, if the position is greater than this, it will be scrobbled.
        public int ScrobblePosition = 240;




        public SkPlayer player;

        public event EventHandler<OnScrobbleArgs>? OnScrobble;
        public event EventHandler<OnNowPlayingArgs>? OnNowPlaying;

        public Boolean HasScrobbled = false;
        private Boolean HasExceededGracePeriod = false;
        private DateTimeOffset TrackStartedAt = DateTime.Now;
        private int CurSongScrobbleAt = -1;
        private string? _currentSongId = null;
        public string? CurrentSongId {
            get => _currentSongId;
            set {
                if (_currentSongId == value) return;
                _currentSongId = value;
                HasScrobbled = false;
                this.OnNowPlaying?.Invoke(this, 
                    new OnNowPlayingArgs { Track = this.CurrentSongId == null ? null : player.Track }
                );
            }
        }


        public SkScrobblerCore(SkPlayer player)
        {
            Console.WriteLine("SKScrobbler initialing...");
            this.player = player;
            player.OnStateChanged += OnPlayerStateChange;
            player.OnTrackChanged += OnPlayerTrackChange;
            player.OnPlayerPositionChanged += OnPlayerPositionChange;
        }

        public int GetTimeUntilScrobble()
        {
            if (this.player.Track == null) return -1;
            return CurSongScrobbleAt - this.player.Position;
        }

        private void OnPlayerStateChange(object? sender, OnStateChangedArgs e)
        {
            Console.WriteLine($"State changed: {e.State}");
            switch (e.State)
            {
                case PlayerState.Playing:
                    break;
                case PlayerState.Paused:
                    break;
                case PlayerState.Stopped:
                    this.CurrentSongId = null;
                    break;
            }
        }
        private void OnPlayerTrackChange(object? sender, OnTrackChangedArgs e)
        {
            if (e.Track == null) {
                this.CurrentSongId = null;
                return;
            };
            var percentThreshold = (int)(e.Track.Duration * this.ScrobblePercent);
            CurSongScrobbleAt = Math.Min(percentThreshold, this.ScrobblePosition);
            this.CurrentSongId = e.Track.ID;
            this.TrackStartedAt = DateTime.UtcNow;
            Console.WriteLine($"Track changed: {e.Track.Title} - {e.Track.Artist} ({e.Track.ID})");
        }
        private void OnPlayerPositionChange(object? sender, OnPlayerPositionChangedArgs e)
        {
            if (this.CurrentSongId == null) return;
            if (e.Position < this.GracePeriod && this.HasExceededGracePeriod) {
                this.HasExceededGracePeriod = false;
                this.HasScrobbled = false;
                return;
            }
            if (e.Position > this.GracePeriod) this.HasExceededGracePeriod = true;
            if (this.HasScrobbled || this.player.Track == null) return;
            var TTS = this.GetTimeUntilScrobble();
            if (TTS < 0) {
                Console.WriteLine("Scrobbling!");
                this.HasScrobbled = true;
                this.OnScrobble?.Invoke(this, new OnScrobbleArgs { Track = player.Track, Time = this.TrackStartedAt.ToUnixTimeSeconds() });
                return;
            }

        }
    }
}