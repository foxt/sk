using System;
namespace sk.Players.Generic
{
    public enum PlayerState {
        Playing,
        Paused,
        Stopped
    }
    public class PlayerTrack {
        public string ID {
            get; set;
        }
        public string Title {
            get; set;
        }
        public string Artist {
            get; set;
        }
        public string Album {
            get; set;
        }
        public string AlbumArtist {
            get; set;
        }
        public int Duration {
            get; set;
        }

    }
    public class OnStateChangedArgs : EventArgs {
        public PlayerState State {
            get; set;
        }
    }
    public class OnTrackChangedArgs : EventArgs {
        public PlayerTrack Track {
            get; set;
        }
    }
    public class OnPlayerPositionChangedArgs : EventArgs {
        public int Position {
            get; set;
        }
    }
    public class OnScrobbleArgs : EventArgs {
        public PlayerTrack Track {
            get; set;
        }
        public long Time {
            get; set;
        }
    }
    public class OnNowPlayingArgs : EventArgs {
        public PlayerTrack Track {
            get; set;
        }
    }

}

