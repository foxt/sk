using AppKit;
using WebKit;
using Foundation;
using CoreGraphics;

namespace sk.UI.Mac
{
	[Register ("AppDelegate")]
	public class AppDelegate : NSApplicationDelegate
	{
		public AppDelegate ()
		{
		}

		public override void DidFinishLaunching (NSNotification notification)
		{
			var lfm = new SkLastFMScrobbler();
			lfm.scrobbler.OnNowPlaying += Scrobbler_OnNowPlaying;

			lfm.api.OnAuthRequired += Api_OnAuthRequired;
			lfm.api.EnsureAuthenticated(true);
		}

		private void Api_OnAuthRequired(object sender, OnAuthRequiredArgs e)
		{
			var controller = new AuthWebViewController(e);
			controller.Window.MakeKeyAndOrderFront(controller);
		}

		private void Scrobbler_OnNowPlaying(object sender, OnNowPlayingArgs e)
		{
			throw new System.NotImplementedException();
		}

		public override void WillTerminate (NSNotification notification)
		{
			// Insert code here to tear down your application
		}
	}
}

