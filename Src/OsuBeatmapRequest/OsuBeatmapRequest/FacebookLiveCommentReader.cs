using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OsuBeatmapRequest
{
    public class FacebookLiveCommentReader
    {
        private string _RequestUrl;
        private WebResponse _ApiRespone;
        private Stream _ReceivedStream;

        public string VideoID { get; set; }
        public string AccessToken { get; set; }
        public List<Comment> Comments { get; set; }

        public FacebookLiveCommentReader(string videoID, string accessToken)
        {
            VideoID = videoID;
            AccessToken = accessToken;
            Comments = new List<Comment>();
            _RequestUrl = $"https://streaming-graph.facebook.com/{videoID}/live_comments?access_token={accessToken}&comment_rate=ten_per_second";
        }

        public async void StartRead()
        {
            var req = (HttpWebRequest)WebRequest.Create(_RequestUrl);
            _ApiRespone = await req.GetResponseAsync();
            _ReceivedStream = _ApiRespone.GetResponseStream();

            while (true)
            {
                var newComment = await Task.Run(() => GetComment());
                Comments.Add(newComment);
                OnNewCommentReceived(newComment);
                await Task.Delay(10);
            }
        }

        private Comment GetComment()
        {
            //Temp buffer
            var readBytes = new List<byte>();
            while (true)
            {
                int rByte = _ReceivedStream.ReadByte();
                if (rByte != -1)
                {
                    readBytes.Add((byte)rByte);
                    if (rByte == 10)
                    {
                        var receivedConent = Encoding.UTF8.GetString(readBytes.ToArray()).Replace("data: ", "").Trim();
                        if (receivedConent != ": ping" && !receivedConent.Contains("\n") && !string.IsNullOrEmpty(receivedConent))
                        {
                            return JsonConvert.DeserializeObject<Comment>(receivedConent);
                        }
                        readBytes.Clear();
                    }
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
        }

        public async Task<string> PostComment(string comment)
        {
            var req = (HttpWebRequest)WebRequest.Create($"https://graph.facebook.com/v3.3/{VideoID}/comments?access_token={AccessToken}");
            req.Method = "POST";
            req.Headers.Add("message", comment);
            using(var resp = await req.GetResponseAsync())
            {
                using (var stream = resp.GetResponseStream())
                {
                    var reader = new StreamReader(stream);
                    return await reader.ReadToEndAsync();
                }
            }
        }

        public event EventHandler<NewCommentReceivedEventArgs> NewCommentReceived;
        protected virtual void OnNewCommentReceived(Comment c) => NewCommentReceived?.Invoke(this, new NewCommentReceivedEventArgs { Comment = c });
    }

    public class NewCommentReceivedEventArgs : EventArgs
    {
        public Comment Comment { get; set; }
    }

    public interface ICommentReader
    {
        event EventHandler<NewCommentReceivedEventArgs> NewCommentReceived;
    }


    public class Comment
    {
        public string id { get; set; }
        public string message { get; set; }
        public From from { get; set; }
        public DateTime created_time { get; set; }
    }

    public class From
    {
        public string id { get; set; }
        public string name { get; set; }
    }
}
