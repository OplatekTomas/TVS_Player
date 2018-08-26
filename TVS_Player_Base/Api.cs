using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TVS_Player_Base {
    public class Api {
        public static event EventHandler Disconnected;
        public static bool IsConnected { get; set; } = false;
        public static string Version { get; set; }
        public static string Ip { get; set; }
        public static int Port { get; set; }
        public static string Token { get; set; }

        static HttpClient Client { get; set; }

        public static async Task<bool> Connect(string ip, int port) {          
            Client = new HttpClient();
            Client.BaseAddress = new Uri("http://" + ip + ":" + port + "/");
            if (IsOpen(ip, port) && await IsReady()) {
                Ip = ip;
                Port = port;
                IsConnected = true;
            } else {
                Client.Dispose();
            }
            return IsConnected;
        }

        private static async Task<bool> IsReady() {
            try {
                var jObject = JObject.Parse(await (await Client.GetAsync("/")).Content.ReadAsStringAsync());
                if (jObject.ContainsKey("Version") && jObject.ContainsKey("Name") && jObject["Name"].ToString() == "TVS_Server") {
                    Version = jObject["Version"].ToString();
                    return true;
                }
            } catch(Exception e) {
                return false;
            }
            return false;
        }

        private static bool IsOpen(string ip, int port) {
            try {
                using (var client = new TcpClient(ip, port))
                    return true;
            } catch (SocketException ex) {
                return false;
            }
        }

        public static async Task<object> Search(string query) {
            return await GetDataArray("/api/Search?query=" + query);
        }

        public static async Task<(bool loggedin, string message, string token)> Login(string username, string password) {
            var result = await SendRequest(HttpMethod.Post, "login/", "{ \"username\":\"" + username + "\",\"password\":\"" + password + "\"}");
            if (result.success) {
                Token = result.data;
                Client.DefaultRequestHeaders.Add("AuthToken", Token);
            }
            return result; 
        }

        public static async Task<(bool registered, string message, string token)> Register(string username, string password) {
            if (username.Length > 3 && password.Length > 3) {
                var result = await SendRequest(HttpMethod.Post, "register/", "{ \"username\":\"" + username + "\",\"password\":\"" + password + "\"}");
                if (result.success) {
                    Token = result.data;
                    Client.DefaultRequestHeaders.Add("AuthToken", Token);
                }
                return result;
            } else {
                return (false, "Username or password is too short", null);
            }
        }

        public static void Login(string token) {
            Token = token;
            Client.DefaultRequestHeaders.Add("AuthToken", Token);
        }

        public static async Task<bool> PostData(string requestUrl, string content) {
            var (success, message, data) = await SendRequest(HttpMethod.Post, requestUrl, content);
            return success;
        }


        public static async Task<JArray> GetDataArray(string requestUrl) {
            var (success, message, data) = await SendRequest(HttpMethod.Get, requestUrl);
            if (success) {
                try {
                    return JArray.Parse(data);
                } catch (JsonException e) { }
            }
            return new JArray();
        }

        public static async Task<JObject> GetDataObject(string requestUrl) {
            var (success, message, data) = await SendRequest(HttpMethod.Get, requestUrl);
            if (success) {
                try {
                    return JObject.Parse(data);
                } catch (JsonException e) { }
            }
            return new JObject();
        }

        public static async Task<(bool success, string message, string data)> SendRequest(HttpMethod method, string requestUrl, string data = "") {
            if (IsConnected) {
                requestUrl = requestUrl.StartsWith("/") ? requestUrl.Remove(0, 1) : requestUrl;
                try {
                    HttpResponseMessage response = null;
                    if (method == HttpMethod.Get) {
                        response = await Client.GetAsync(requestUrl);
                    } else if (method == HttpMethod.Post && !string.IsNullOrEmpty(data)) {
                        response = await Client.PostAsync(requestUrl, new StringContent(data));
                    }
                    if (response.IsSuccessStatusCode) {
                        return (true, response.ReasonPhrase, await response.Content.ReadAsStringAsync());
                    } else {
                        return (false, response.ReasonPhrase, null);
                    }
                } catch (Exception e) {
                    IsConnected = false;
                    if (Disconnected != null) {
                        Disconnected.Invoke("Request - " + requestUrl, new EventArgs());
                    }
                }
            }
            return (false, "API is not connected.", null);
        }

    }
}
