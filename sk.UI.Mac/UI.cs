using System;
using System.Timers;
using AppKit;
using Foundation;
using WebKit;
using sk.Players.Generic;

namespace sk.UI.Mac
{
    public class UI : NSMenuDelegate
	{
        PlayerTrack? track;

		NSStatusBar statusBar = NSStatusBar.SystemStatusBar;
		NSStatusItem sbIcon;
		SkLastFMScrobbler scrobbler;
        NSTimer? updateTimer;


        NSMenuItem trackTitle = new NSMenuItem("🎵 ") { Enabled = false, Hidden = true };
        NSMenuItem trackArtist = new NSMenuItem("🎤 ") { Enabled = false, Hidden = true };
        NSMenuItem trackAlbum = new NSMenuItem("💿 ") { Enabled = false, Hidden = true };
        NSMenuItem scrobbleTime = new NSMenuItem("⏯️ Play some music to get started!") { Enabled = false, Hidden = false };
        NSMenuItem quitItem = new NSMenuItem("❌ Quit");

        int timeTillScrobble = 0;
        public UI(SkLastFMScrobbler scrobbler)
		{
			this.scrobbler = scrobbler;
			sbIcon = statusBar.CreateStatusItem(NSStatusItemLength.Variable);
			sbIcon.Button.Image = NSImage.ImageNamed("logo16@2x.png");
            sbIcon.Button.Image.Template = true;
            sbIcon.HighlightMode = true;
            var m = sbIcon.Menu = new NSMenu("sk");
			sbIcon.Menu.Delegate = this;

			m.AddItem(trackTitle);
			m.AddItem(trackArtist);
			m.AddItem(trackAlbum);
			m.AddItem(scrobbleTime);
			m.AddItem(NSMenuItem.SeparatorItem);

            m.AddItem(quitItem);
			quitItem.Activated += (_,_) => { NSApplication.SharedApplication.Terminate(this); };

            scrobbler.scrobbler.OnNowPlaying += (_, e) => {
                timeTillScrobble = scrobbler.scrobbler.GetTimeUntilScrobble();
                this.track = e.Track;
            };
            scrobbler.scrobbler.player.OnPlayerPositionChanged += (_,_) => {
                timeTillScrobble = scrobbler.scrobbler.GetTimeUntilScrobble();
            };
        }

        public void UpdateTrackName() {
            Console.WriteLine("Track is " + (track == null ? "null" : track.Title));
            Console.WriteLine("State is " + (scrobbler.scrobbler.player.State));

            if (this.track == null || scrobbler.scrobbler.player.State == PlayerState.Stopped) {
                this.trackTitle.Hidden = true;
                this.trackAlbum.Hidden = true;
                this.trackArtist.Hidden = true;

                this.scrobbleTime.Title = "⏹️ Nothing is playing.";
                return;
            }


            this.trackTitle.Title = "🎵 " + this.track.Title;
            this.trackTitle.Hidden = false;
            this.trackAlbum.Title = "💿 " + this.track.Album;
            this.trackAlbum.Hidden = false;
            this.trackArtist.Title = "🎤 " + this.track.Artist;
            this.trackArtist.Hidden = false;

            this.UpdateTime();
        }

		public void UpdateTime() {
            if (this.scrobbler.scrobbler.HasScrobbled) {
                this.scrobbleTime.Title = "🔁 Scrobbled!";
            } else if (this.scrobbler.scrobbler.player.State == PlayerState.Playing) {
                var TTS = timeTillScrobble;
                var MTS = Math.Floor((double)TTS / 60).ToString().PadLeft(2, '0');
                var STS = (TTS % 60).ToString().PadLeft(2, '0');
                timeTillScrobble--;
                this.scrobbleTime.Title = $"▶️ Scrobbling in {MTS}:{STS} seconds...";
                return;
            } else {
                this.scrobbleTime.Title = "⏸️ Playback is paused.";
            }
        }

        public override void MenuDidClose(NSMenu menu)
        {
            updateTimer?.Invalidate();
        }
        public override void MenuWillOpen(NSMenu menu)
        {
            UpdateTrackName();
            updateTimer = NSTimer.CreateRepeatingScheduledTimer(1, (NSTimer timer) => UpdateTrackName());
            NSRunLoop.Current.AddTimer(updateTimer, NSRunLoopMode.Common);
        }

        public override void MenuWillHighlightItem(NSMenu menu, NSMenuItem item) {}
	}
}

