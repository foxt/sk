using System;
using System.Timers;
using AppKit;
using Foundation;
using WebKit;

namespace sk.UI.Mac
{
    // this code is ✨ horrible ✨
    public class UI : NSMenuDelegate
	{
        PlayerTrack track;

		NSStatusBar statusBar = NSStatusBar.SystemStatusBar;
		NSStatusItem sbIcon;
		SkLastFMScrobbler scrobbler;
		Timer updateTimer = new Timer(1000);

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
			sbIcon.Image = NSImage.ImageNamed("AppIcon-16.png");
			sbIcon.HighlightMode = true;
			var m = sbIcon.Menu = new NSMenu("sk");
			sbIcon.Menu.Delegate = this;

			m.AddItem(trackTitle);
			m.AddItem(trackArtist);
			m.AddItem(trackAlbum);
			m.AddItem(scrobbleTime);
			m.AddItem(NSMenuItem.SeparatorItem);

            m.AddItem(quitItem);
			quitItem.Activated += (object s, EventArgs e) => { NSApplication.SharedApplication.Terminate(this); };

            scrobbler.scrobbler.OnNowPlaying += (sender, e) => {
                timeTillScrobble = scrobbler.scrobbler.GetTimeUntilScrobble();
                this.track = e.Track;
                UpdateTrackName();
            };
            scrobbler.scrobbler.player.OnPlayerPositionChanged += (sender, e) => {
                timeTillScrobble = scrobbler.scrobbler.GetTimeUntilScrobble();
                UpdateTime();
            };
            scrobbler.scrobbler.player.OnStateChanged += (sender, e) => UpdateTrackName();
            scrobbler.scrobbler.OnScrobble += (sender, e) => UpdateTime();
            this.updateTimer.Elapsed += (sender, e) => UpdateTime();
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


            this.trackTitle.Title = this.track.Title;
            this.trackTitle.Hidden = false;
            this.trackAlbum.Title = this.track.Album;
            this.trackAlbum.Hidden = false;
            this.trackArtist.Title = this.track.Artist;
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
            updateTimer.Stop();
        }
        public override void MenuWillOpen(NSMenu menu)
        {
            updateTimer.Start();
        }

        public override void MenuWillHighlightItem(NSMenu menu, NSMenuItem item) {}
	}
}

