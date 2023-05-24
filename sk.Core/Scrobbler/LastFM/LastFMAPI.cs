using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace sk
{
        public class LastFMAPI {
                public string BaseURL = "https://ws.audioscrobbler.com/2.0/";
                public string AuthUrl = "http://www.last.fm/api/auth/";

		public string APIKey = "2fcff9153873989abc1d8b4743d7b4e3";
                public string SharedSecret = "a1832fe0e3b4b14e7df5525e22ce0e64";
                public Task? AuthHold;
		public event EventHandler<OnAuthRequiredArgs>? OnAuthRequired;

		public string? SessionKey;
        
                private HttpClient client = new HttpClient();

                public LastFMAPI() {
                        var entryAssembly = System.Reflection.Assembly.GetEntryAssembly();
                        var entryAssemblyName = entryAssembly == null ? "unknown" : entryAssembly.FullName;
                        Console.WriteLine($"SK Core LFM API: sk dotnet (https://www.last.fm/user/foxtay; {entryAssemblyName}) ");
			client.DefaultRequestHeaders.Add("User-Agent", $"sk dotnet (https://www.last.fm/user/foxtay; {entryAssemblyName})");
                }

                public async Task<bool> CheckAuthenticated(bool deep = false) {
                        if (AuthHold != null)
                                await AuthHold;
                        if (this.SessionKey == null)
                                return false;
                        if (!deep)
                                return true;
                        var user = await MakeGET("user.getInfo", false, new Dictionary<string, string>() { { "sk", this.SessionKey } });
                        dynamic userInfo = JObject.Parse(user);
                        if (userInfo.error != null) {
                                Console.WriteLine("user isn't authed - " + user);
                                return false;
                        } else {
                                Console.WriteLine("hello " + userInfo.user.name);
                                return true;
                        }
                }

                public async Task EnsureAuthenticated(bool deep = false) {
                        if (await CheckAuthenticated(deep))
                                return;
                        var tcs = new TaskCompletionSource<bool>();
                        this.AuthHold = tcs.Task;
                        this.OnAuthRequired!.Invoke(this, new OnAuthRequiredArgs() {
                                Url = AuthUrl + "?api_key=" + this.APIKey,
                                Callback = (async token => {
                                        var sess =await MakePOST("auth.getSession",false, new Dictionary<string, string>() { { "token", token } });
                                        dynamic session = JObject.Parse(sess);
                                        if (session.session == null || session.session.key == null) {
                                                Console.Error.WriteLine("FATAL: " + sess);
                                                return false;
                                        }
                                        this.SessionKey = session.session.key;
                                        return tcs.TrySetResult(true);
                                })

                        });

                        await tcs.Task;
                        await CheckAuthenticated(true);
                }

                public async Task<string> MakeGET(string method, bool authed, Dictionary<string,string> args) {
                        args.Add("method", method);
                        args.Add("api_key", this.APIKey);
                        args.Add("format", "json");
                        if (authed) {
                                await EnsureAuthenticated(false);
                                args.Add("sk", this.SessionKey!);
                        }

			var url = new Uri(BaseURL + "?" + await (new FormUrlEncodedContent(args).ReadAsStringAsync()));
                        Console.WriteLine("GET " + url);
                        var response = client.GetAsync(url).Result;
                        Console.WriteLine("GET " + method + ": " + response.StatusCode);
                        if (
                                authed &&
                                response.StatusCode == System.Net.HttpStatusCode.Unauthorized &&
                                !await CheckAuthenticated(true)
                                ) {
                                await EnsureAuthenticated();
                                return await MakeGET(method, authed, args);
                        }

                        var responseString = await response.Content.ReadAsStringAsync();
                        return responseString;
                }
                public Task<string> MakeGET(string method, bool authed) { return MakeGET(method, authed, new Dictionary<string, string>()); }

                public async Task<string> MakePOST(string method, bool authed, Dictionary<string,string>? args) {
                        if (args == null) args = new Dictionary<string, string>();
                        args.Add("method", method);
                        args.Add("api_key", this.APIKey);
                        args.Add("format", "json");
                        if (authed && await CheckAuthenticated(false)) args.Add("sk", this.SessionKey!);

                        var SignatureKeys = "";
                        var Keys = new List<string>(args.Keys);
                        Keys.Sort();
                        foreach (var key in Keys) {
                                if (key == "format") continue;
                                SignatureKeys += key + args[key];
                        }
                        SignatureKeys += this.SharedSecret;
                        var Signature = System.Security.Cryptography.MD5.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(SignatureKeys));
                        var SignatureString = "";
                        foreach (var b in Signature) SignatureString += b.ToString("x2");
                        args.Add("api_sig", SignatureString);

                        var url = new Uri(BaseURL);
                        var response = client.PostAsync(url, new FormUrlEncodedContent(args)).Result;
                        Console.WriteLine("POST " + method + ": " + response.StatusCode);
			if (
				authed &&
				response.StatusCode == System.Net.HttpStatusCode.Unauthorized &&
				!await CheckAuthenticated(true)
				) {
				await EnsureAuthenticated();
				return await MakePOST(method, authed, args);
			}

			var responseString = response.Content.ReadAsStringAsync().Result;
                        return responseString;
                }
                public Task<string> MakePOST(string method,bool authed) { return MakePOST(method, authed, new Dictionary<string, string>()); }

        }
}