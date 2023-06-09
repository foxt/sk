using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace sk
{
	public class SkMacAppleMusicPlayer : SkPlayer
	{
		public SkMacAppleMusicPlayer()
		{
			this.Name = "Apple Music";
			startProcess();
		}
		private Process? process;
		private void startProcess() {
			if (process != null && !process.HasExited) return;
			if (process != null) process.Dispose();
			Console.WriteLine("Starting sk-mac-applemusic...");
			process = Process.Start(new ProcessStartInfo {
				FileName = Path.Combine(
					Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
					"sk-mac-applemusic"
				),
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				UseShellExecute = false,
				CreateNoWindow = true,
			});
			process.EnableRaisingEvents = true;
			process!.OutputDataReceived += (sender, e) => {
				if (e.Data == null) return;
				var args = e.Data.Substring(1);

				switch (e.Data[0]) {
					case 's':
						State = (PlayerState)Enum.Parse(typeof(PlayerState), args);
						break;
					case 't':
						Track = JsonConvert.DeserializeObject<PlayerTrack>(args);
						break;
					case 'p':
						Position = ((int)float.Parse(args));
						break;
				}
			};
			process.BeginOutputReadLine();
			process.Exited += (sender, e) => {
				process = null;
				startProcess();
			};

		}
	}
}