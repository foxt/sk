using System;
using sk.Core;
using Security;
using Foundation;
namespace sk.UI.Mac {
    public class PreferencesSecretStore : BaseSecretStore {


        override public string? GetSecret(string service) {
            return NSUserDefaults.StandardUserDefaults.StringForKey("token-" + service);
        }
        override public bool SetSecret(string service, string secret, string username) {
            NSUserDefaults.StandardUserDefaults.SetString(secret, "token-" + service);
            return true;
        }
    }
}

