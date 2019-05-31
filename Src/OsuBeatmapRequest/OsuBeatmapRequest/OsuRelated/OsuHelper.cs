using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace OsuBeatmapRequest.OsuRelated
{
    public class OsuHelper
    {
        private string _BeatmapSavedLocation;
        public string BeatmapSavedLocation
        {
            get => _BeatmapSavedLocation;
            set
            {
                if (_BeatmapSavedLocation != value)
                {
                    _BeatmapSavedLocation = value;
                    if (!Directory.Exists(BeatmapSavedLocation))
                        Directory.CreateDirectory(BeatmapSavedLocation);
                }
            }
        }
        private Random _Random;
        public int GetRandom { get => _Random.Next(); }
        public string Username { get; set; }
        public string Password { get; set; }
        public OsuTokenInfo CurrentToken { get; set; }
        public OsuHelper(string usernane, string password)
        {
            Username = usernane;
            Password = password;
            BeatmapSavedLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Beatmapsets");
            _Random = new Random();
            OsuLogin(Username, Password);
        }

        /// <summary>
        /// Get beatmaps by key word using osu! api.
        /// </summary>
        /// <param name="searchName">The key word to search for beatmap set</param>
        /// <returns>The collection of beatmapset, after 2 retries failed return null</returns>
        public async Task<Beatmapset[]> GetBeatmapsetsAsync(string searchName)
        {
            if (CurrentToken == null)
                await OsuLogin(Username, Password);
            int currentTry = 0;
            while (currentTry < 2)
            {
                try
                {
                    var req = (HttpWebRequest)WebRequest.Create($"https://osu.ppy.sh/api/v2/beatmapsets/search?q={HttpUtility.HtmlEncode(searchName)}&m=0&s=0&sort=relevance_desc");
                    req.Headers.Add("Authorization", $"Bearer {CurrentToken.access_token}");
                    using (var resp = await req.GetResponseAsync())
                    {
                        using (var stream = resp.GetResponseStream())
                        {
                            var reader = new StreamReader(stream);
                            string content = reader.ReadToEnd();
                            BeatmapRequest beatmaps = JsonConvert.DeserializeObject<BeatmapRequest>(content);
                            return beatmaps.beatmapsets;
                        }
                    }
                }
                catch
                {
                    await OsuLogin(Username, Password);
                    currentTry++;
                }
            }
            return null;
        }

        public async Task<string> DownloadBeatmapSetAsyncTask(int id)
        {
            if (CurrentToken == null)
                await OsuLogin(Username, Password);
            int currentTry = 0;
            while (currentTry < 2)
            {
                try
                {
                    var req = (HttpWebRequest)WebRequest.Create($"https://osu.ppy.sh/api/v2/beatmapsets/{id}/download");
                    req.Headers.Add("Authorization", $"Bearer {CurrentToken.access_token}");
                    using (var resp = await req.GetResponseAsync())
                    {
                        using (var stream = resp.GetResponseStream())
                        {
                            string savedLoc = Path.Combine(BeatmapSavedLocation, $"{GetRandom}.osz");
                            using (FileStream fs = new FileStream(savedLoc, FileMode.Create))
                            {
                                await stream.CopyToAsync(fs);
                                return savedLoc;
                            }
                        }
                    }
                }
                catch
                {
                    await OsuLogin(Username, Password);
                    currentTry++;
                }
            }
            return null;
        }
        public async void DownloadBeatmapSetAsync(int id)
        {
            var res = await Task.Run(() => DownloadBeatmapSetAsyncTask(id));
            OnBeatmapsetDownloadCompleted(new BeatmapSetDownloadedEventArgs() { BeatmapSetID = id, FileLocation = res ?? "" });
        }

        public async Task OsuLogin(string username, string password)
        {
            int currentTry = 0;
            while (currentTry < 2)
            {
                try
                {
                    var content = new MultipartFormDataContent("-----------------------------28947758029299");
                    content.Add(new StringContent(username), "username");
                    content.Add(new StringContent(password), "password");
                    content.Add(new StringContent("password"), "grant_type");
                    content.Add(new StringContent("5"), "client_id");
                    content.Add(new StringContent("FGc9GAtyHzeQDshWP5Ah7dega8hJACAJpQtw6OXk"), "client_secret");
                    content.Add(new StringContent("*"), "scope");
                    var para = await content.ReadAsStreamAsync();

                    var req = (HttpWebRequest)WebRequest.Create("https://osu.ppy.sh/oauth/token");
                    req.Method = "POST";
                    req.Accept = "application/json";
                    req.UserAgent = "osu!";
                    req.ContentType = "multipart/form-data; boundary=-----------------------------28947758029299";
                    var reqStream = req.GetRequestStream();
                    await para.CopyToAsync(reqStream);
                    using (var resp = await req.GetResponseAsync())
                    {
                        using (var stream = resp.GetResponseStream())
                        {
                            StreamReader reader = new StreamReader(stream);
                            var respStreamText = await reader.ReadToEndAsync();
                            CurrentToken = JsonConvert.DeserializeObject<OsuTokenInfo>(respStreamText);
                            return;
                        }
                    }
                }
                catch
                {
                    currentTry++;
                }
            }
        }

        public event EventHandler<BeatmapSetDownloadedEventArgs> BeatmapsetDownloadCompleted;
        protected virtual void OnBeatmapsetDownloadCompleted(BeatmapSetDownloadedEventArgs e) => BeatmapsetDownloadCompleted?.Invoke(this, e);
    }
}
