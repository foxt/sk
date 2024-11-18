using System;
using System.Timers;
using System.Diagnostics;
using ScriptingBridge;
using sk.Players.Generic;
using Timer = System.Timers.Timer;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.InteropServices;
using ObjCRuntime;
using Foundation;

namespace sk.Players.Mac.AppleMusic {
    public class SkMacAppleMusicPlayer : SkPlayer {
        [DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
        static extern IntPtr objc_msgSend_uint(
            IntPtr target,
            IntPtr selector,
            uint code
        );
        [DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
        static extern IntPtr objc_msgSend(
            IntPtr target,
            IntPtr selector
        );

        private NSDistributedNotificationCenter dnc = NSDistributedNotificationCenter.DefaultCenter;
        public SkMacAppleMusicPlayer() {
            this.Name = "Apple Music";
            startListener();

        }
        // We need to know which bundle ID to request data from (com.apple.iTunes or
        // com.apple.Music).
        // On older systems with iTunes, we recieve only the
        // com.apple.iTunes.playerInfo event, and thus need to send AppleEvents to
        // iTunes bundle ID.
        // However, on newer systems, *both* com.apple.iTunes.playerInfo &
        // com.apple.Music.playerInfo are recieved, but we should filter out the
        // duplicate iTunes events, and note down that we should send events to the
        // Music bundle ID (sending to iTunes bundle ID will fail on new systems).
        // We can get away with having a boolean here, as we will only ever send events
        // when isPlaying is true, meaning we have already recieved a Notification. The
        // first notification will be duplicated as AM sends the legacy iTunes event
        // first, but this isn't too big a deal.
        bool isAppleMusic = false;
        bool isPlaying = false;
        private Timer timer;

        private void startListener() {
            dnc.AddObserver(new NSString("com.apple.Music.playerInfo"), this.NotificationHandler);
            dnc.AddObserver(new NSString("com.apple.iTunes.playerInfo"), this.NotificationHandler);
            this.timer = new Timer(3000);
            timer.Elapsed += PullElapsedTime;


        }

        SBApplication app;
        SBObject prop;

        private void PullElapsedTime(object? sender, ElapsedEventArgs e)
        {
            if (!this.isPlaying) return;
            try {
            
                if (this.app == null)
                    this.app = SBApplication.GetApplication<SBApplication>(this.isAppleMusic ? "com.apple.Music" : "com.apple.iTunes");
                if (this.app == null) {
                    Console.WriteLine("tried to pull app but is still null, assuming the app has been quat(??)");
                    this.isPlaying = false;
                    return;
                }

                if (this.prop == null) {
                    this.prop = (SBObject)Runtime.GetNSObject(
                        objc_msgSend_uint(app.Handle, new Selector("propertyWithCode:").Handle, 1884319603)
                    )!;
                }
                //Debugger.Break();
                Console.WriteLine(this.prop);
                var value = this.prop.Get;

                this.Position = (int)(NSNumber)value;

                
                Console.WriteLine(this.Position);
            } catch(Exception err) {
                Console.WriteLine("Couldn't PullElapsedTime!!! " + err.ToString());
                this.app = null;
                this.prop = null;
            }
        }

        private string lastId = "";
        private void NotificationHandler(NSNotification notif) {
            if (notif.Name == "com.apple.Music.playerInfo") isAppleMusic = true;
            else if (notif.Name == "com.apple.iTunes.playerInfo" && isAppleMusic) return;

            var data = notif.UserInfo;
            if (data == null)
                return;
            var state = data["Player State"].ToString();
            this.isPlaying = (state == "Playing");
            if (this.isPlaying)
                this.timer.Start();
            else
                this.timer.Stop();

            switch (state) {
                case "Playing":
                    this.State = PlayerState.Playing;
                    break;
                case "Paused":
                    this.State = PlayerState.Paused;
                    break;
                default:
            try {
                    this.State = PlayerState.Stopped;
                    break;
            }

                lastId = idS;
                this.Track = new PlayerTrack() {
                    ID = idS,
                    Title = data["Name"]?.ToString() ?? "Unknown Track",
                    Artist = data["Artist"]?.ToString() ?? "Unknown Artist",
                    Album = data["Album"]?.ToString() ?? "Unknown Album",
                    AlbumArtist = (data["Album Artist"] ?? data["Artist"])?.ToString() ?? "Unknown Artist",
                    Duration = ((NSNumber)data["Total Time"]).Int32Value / 1000
                };

            } catch (Exception err) {
                Console.WriteLine("Error processing DNC event!!!  " + err.ToString());    
            }
        }
    }
}

