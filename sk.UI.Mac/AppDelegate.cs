using AppKit;
using WebKit;
using Foundation;
using CoreGraphics;
using sk.Players.Generic;
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
			var player = new sk.Players.Mac.AppleMusic.SkMacAppleMusicPlayer();
			var sk = new SkScrobblerCore(player);
			var lfm = new SkLastFMScrobbler(sk, new MacKeychainSecretStore());
			var ui = new UI(lfm);

			lfm.api.OnAuthRequired += Api_OnAuthRequired;
			lfm.api.EnsureAuthenticated(true);
		}

		private void Api_OnAuthRequired(object? sender, OnAuthRequiredArgs e)
		{
			var controller = new AuthWebViewController(e);
			controller.Window.MakeKeyAndOrderFront(controller);
		}

		public override void WillTerminate (NSNotification notification)
		{
			// Insert code here to tear down your application
		}
	}
}

