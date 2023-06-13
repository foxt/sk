using System;


namespace sk.Players.Generic
{
    public class SkPlayer
    {
        public string Name = "None";
        public event EventHandler<OnStateChangedArgs> OnStateChanged;
        public event EventHandler<OnTrackChangedArgs> OnTrackChanged;
        public event EventHandler<OnPlayerPositionChangedArgs> OnPlayerPositionChanged;

        private PlayerState _state = PlayerState.Stopped;
        private PlayerTrack _track;
        private int _position = 0;

        public PlayerState State
        {
            get => _state;
            set
            {
                if (_state == value) return;
                _state = value;
                OnStateChanged?.Invoke(this, new OnStateChangedArgs { State = value });
            }
        }
        public PlayerTrack Track
        {
            get => _track;
            set
            {
                if (_track == value) return;
                if (_track != null && value != null && _track.ID == value.ID) {
                    _track = value;
                    return;
                };
                _track = value;
                this.Position = 0;
                OnTrackChanged?.Invoke(this, new OnTrackChangedArgs { Track = value });
            }
        }
        public int Position
        {
            get => _position;
            set
            {
                if (_position == value) return;
                _position = value;
                OnPlayerPositionChanged?.Invoke(this, new OnPlayerPositionChangedArgs { Position = value });
            }
        }
    }
}