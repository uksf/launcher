using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UKSF.Launcher.Network {
    public static class ApiWrapper {
        private static string token;

        public static async Task Login(string email, string password) {
            using (HttpClient client = new HttpClient()) {
                HttpContent content = new StringContent(JsonConvert.SerializeObject(new {email, password}), Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PostAsync($"{Global.URL}/login", content).Result;
                string responseString = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode) {
                    throw new LoginFailedException(JObject.Parse(responseString)["message"].ToString());
                }

                token = responseString.Replace("\"", "");
            }
        }

        public static async Task<string> Get(string path) {
            using (HttpClient client = new HttpClient()) {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.GetAsync($"{Global.URL}/{path}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }

        public static async Task<Stream> GetFile(string path) {
            using (HttpClient client = new HttpClient()) {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.GetAsync($"{Global.URL}/{path}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStreamAsync();
            }
        }

        public static async Task<string> Post(string path, object body) {
            using (HttpClient client = new HttpClient()) {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpContent content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync($"{Global.URL}/{path}", content);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }
    }

    public class LoginFailedException : Exception {
        public readonly string Reason;

        public LoginFailedException(string reason) : base(reason) => Reason = reason;
    }
}
