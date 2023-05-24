using System;
using AppKit;
using CoreGraphics;
using Foundation;
using WebKit;
namespace sk.UI.Mac
{
	class AuthWebViewDelegate : WKNavigationDelegate {
		public event EventHandler<string> OnAuthResult;

		[Export("webView:decidePolicyForNavigationAction:decisionHandler:")]
		public override void DecidePolicy(WKWebView webView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler) {
			Console.WriteLine(navigationAction.Request.Url);
			decisionHandler(WKNavigationActionPolicy.Allow);
		}
	}
	class AuthWebView : NSWindow {
		AuthWebViewDelegate delagate = new AuthWebViewDelegate();
		public WKWebView webview;

		public AuthWebView(IntPtr hwnd) : base(hwnd) { }
		[Export("initWithCoder:")]
		public AuthWebView(NSCoder coder) : base(coder) { }
		public AuthWebView(CGRect contentRect, NSWindowStyle aStyle, NSBackingStore bufferingType, bool deferCreation) : base(contentRect, aStyle, bufferingType, deferCreation) {
			Title = "sk: Log in to last.fm";
			Level = NSWindowLevel.Floating;
			webview = new WKWebView(Frame, new WKWebViewConfiguration() {
				ApplicationNameForUserAgent = "sk (me@foxt.dev; https://www.last.fm/user/foxtay) sk-ui-mac",
				UpgradeKnownHostsToHttps = true,
				AllowsAirPlayForMediaPlayback = false
			});
			webview.NavigationDelegate = delagate;
			ContentView = webview;
		}
	}
	class AuthWebViewController : NSWindowController {
		public AuthWebViewController(IntPtr handle) : base(handle) {
		}

		[Export("initWithCoder:")]
		public AuthWebViewController(NSCoder coder) : base(coder) {
		}


		public AuthWebViewController(OnAuthRequiredArgs args) : base("AuthWebView") {
			// Construct the window from code here
			CGRect contentRect = new CGRect(0, 0, 1000, 750);
			var win = new AuthWebView(contentRect, (NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Utility | NSWindowStyle.Resizable), NSBackingStore.Buffered, false);
			win.webview.LoadRequest(new NSUrlRequest(new Uri(args.Url)));

			base.Window = win;
			
			// Simulate Awaking from Nib
			Window.AwakeFromNib();
		}

		public override void AwakeFromNib() {
			base.AwakeFromNib();
		}

		public new AuthWebView Window {
			get {
				return (AuthWebView)base.Window;
			}
		}
	}
}

