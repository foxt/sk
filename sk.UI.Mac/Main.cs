using System.Diagnostics;
using AppKit;

namespace sk.UI.Mac
{
	static class MainClass
	{
		static void Main (string [] args)
		{
			Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;
			NSApplication.Init ();
			NSApplication.Main (args);
		}
	}
}
