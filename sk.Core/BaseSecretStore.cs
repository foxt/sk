using System;
namespace sk.Core
{
    public abstract class BaseSecretStore
    {
        public virtual string? GetSecret(string service) {
            Console.WriteLine("BaseSecretStore#GetSecret called?");
            return null;
        }
        public virtual bool SetSecret(string service, string secret, string username) {
            Console.WriteLine("BaseSecretStore#SetSecret called?");
            return false;
        }
    }
}

