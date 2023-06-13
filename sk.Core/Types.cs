using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace sk
{
    public class OnAuthRequiredArgs : EventArgs { public string Url { get; set; } public Func<string,Task<bool>> Callback { get; set; } }

}