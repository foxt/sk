using System;
using AppKit;

namespace sk.UI.Mac
{
	public class UI
	{
		public UI()
		{
			NSStatusBar statusBar = NSStatusBar.SystemStatusBar;

			var item = statusBar.CreateStatusItem(NSStatusItemLength.Variable);
			item.Image = NSImage.ImageNamed("AppIcon-16.png");
			item.HighlightMode = true;
			item.Menu = new NSMenu("Text");
			//item.Menu.Delegate = this;

			var address = new NSMenuItem("Address");
			address.Hidden = true;
			address.Activated += (sender, e) => {

			};
			item.Menu.AddItem(address);

			var t = new NSMenuItem("test");
			t.Enabled = false;
			t.Activated += (sender, e) => {

			};
			item.Menu.AddItem(t);
		}
	}
}

