using System;
using sk.Core;
using Security;
namespace sk.UI.Mac
{
    public class MacKeychainSecretStore : BaseSecretStore
    {
        // Clean all keychain entries -- we do this to prevent duplicate entries causing undefined behaviour.
        private void KeychainClean(string service) {
            var query = new SecRecord(SecKind.GenericPassword) {
                Service = service,
                Label = "sk stored secret"
            };
            var status = SecKeyChain.Remove(query);

            Console.WriteLine("Keychain Clean: " + status);
        }
        private void KeychainAdd(string service, string secret, string username) {
            KeychainClean(service);
            var record = new SecRecord(SecKind.GenericPassword) {
                Service = service,
                Label = "sk stored secret",
                Account = username,
                ValueData = secret,
                //Accessible = SecAccessible.AfterFirstUnlock,
                //UseDataProtectionKeychain = true,
                //Synchronizable = true
            };
            var status =  SecKeyChain.Add(record) ;
            Console.WriteLine("Keychain Write: " + status);
            if (status != SecStatusCode.Success)
                throw new SecurityException(status);
            
        }
        private string? KeychainGet(string service) {
            var query = new SecRecord(SecKind.GenericPassword) {
                Service = service,
                Label = "sk stored secret"
            };
            SecStatusCode status;
            var data = SecKeyChain.QueryAsData(query, false, out status);

            Console.WriteLine("Keychain Get: " + status);
            if (status == SecStatusCode.Success)
                return data?.ToString();
            else if (status == SecStatusCode.ItemNotFound)
                return null;
            else
                throw new SecurityException(status);
        }

        override public string? GetSecret(string service) {
            return KeychainGet(service);
        }
        override public bool SetSecret(string service, string secret, string username) {
            KeychainAdd(service,secret,username);
            return true;
        }
    }
}

