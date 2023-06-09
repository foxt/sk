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
			var url = navigationAction.Request.Url;
			Console.WriteLine("DecidePolicy: " + url.Host + " - " + url.Path);
			if (url.Host == "foxt.dev" && url.Path == "/sk/auth") {
				var token = url.Query.Replace("token=", "");
				Console.WriteLine("URL auth ");

				// TODO: parse this properly
				OnAuthResult.Invoke(this, token);
				//decisionHandler(WKNavigationActionPolicy.Cancel);
			} else if (
				(url.Host == "www.last.fm" && url.Path == "/api/auth") ||
				(url.Host == "www.last.fm" && url.Path == "/login")) {

				Console.WriteLine("URL allowed");
				decisionHandler(WKNavigationActionPolicy.Allow);
			} else {
				Console.WriteLine("URL not allowed");
				decisionHandler(WKNavigationActionPolicy.Cancel);
			}
			
		}
	}
	class AuthWebView : NSWindow {
		public AuthWebViewDelegate delagate = new AuthWebViewDelegate();
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
			win.delagate.OnAuthResult += (object sender, string e) => {
				win.Close();
				args.Callback(e);
			};

			base.Window = win;
			
			// Simulate Awaking from Nib
			win.AwakeFromNib();
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

