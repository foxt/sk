using Foundation;
using ObjCRuntime;
using ScriptingBridge;

namespace sk.Players.Mac.AppleMusic {

    [BaseType(typeof(SBApplication))]
    public interface iTunesApplication {
        [Export("playerPosition")]
        double playerPosition {
            get;
        }
    }
}


