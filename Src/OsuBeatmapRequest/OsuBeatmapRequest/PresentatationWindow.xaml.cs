using OsuBeatmapRequest.OsuRelated;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace OsuBeatmapRequest
{
    /// <summary>
    /// Interaction logic for PresentatationWindow.xaml
    /// </summary>
    public partial class PresentatationWindow : Window , INotifyPropertyChanged
    {
        public string Username;
        public string Password;
        public string FBAccess;
        public string VideoID;
        public int Interval;
        public bool CanShow = true;

        private int currentTime;
        private DispatcherTimer timer;
        public OsuHelper OsuHelper;
        public FacebookLiveCommentReader CommentSource;
        public ObservableCollection<Beatmapset> BeatmapsetPool { get; set; }

        public PresentatationWindow()
        {
            BeatmapsetPool = new ObservableCollection<Beatmapset>();
            DataContext = this;
            InitializeComponent();
        }

        public async void InitBot()
        {
            if(string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(FBAccess) || string.IsNullOrEmpty(VideoID) || Interval < 3)
            {
                MessageBox.Show("Please Fill in the credential!");
                CanShow = false;
                Close();
            }

            OsuHelper = new OsuHelper(Username, Password);
            CommentSource = new FacebookLiveCommentReader(VideoID, FBAccess);
            CommentSource.StartRead();
            //await CommentSource.PostComment("Hello, this is Osu! beatmap suggestor. Please use !help to see a list of commands");

            CommentSource.NewCommentReceived += ProcessCommand;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            currentTime = Interval;
            timer.Start();
        }

        private async void Timer_Tick(object sender, EventArgs e)
        {
            if(currentTime > 0)
            {
                currentTime--;
                txbStatus.Text = $"Next beatmapset selecting in {currentTime} second(s)";
            }
            else
            {
                timer.Stop();
                txbStatus.Text = "Rolling...";
                var selectedBeatmap = GetRandomBeatmap();
                await AnimateRandomizing(selectedBeatmap);

                if(selectedBeatmap != null)
                {
                    OsuHelper.DownloadBeatmapSetAsync(selectedBeatmap.id);
                    OsuHelper.BeatmapsetDownloadCompleted += (s, ee) =>
                        Process.Start(ee.FileLocation);
                }
                BeatmapsetPool.Clear();
                currentTime = Interval;
                txbStatus.Text = $"Next beatmapset selecting in {currentTime} second(s)";
                timer.Start();

            }
        }

        private async Task AnimateRandomizing(Beatmapset selectedBeatmap)
        {
            int index = FindObservableCollectionIndex(BeatmapsetPool, p => p == selectedBeatmap);

            //Animating
            for (int i = 0; i < 5; i++)
            {
                foreach (var item in BeatmapsetPool)
                {
                    item.SetHighlighting(true);
                    await Task.Delay(200 * (i + 1));
                    item.SetHighlighting(false);
                }
            }

            for (int i = 0; i < index ; i++)
            {
                var item = BeatmapsetPool[i];

                item.SetHighlighting(true);
                await Task.Delay(1000 * (i + 1));
                item.SetHighlighting(false);
            }

            //Choosing animation
            selectedBeatmap.SetHighlighting(true);
            await Task.Delay(2000);
            for (int i = 0; i < 5; i++)
            {
                selectedBeatmap.SetHighlighting(false);
                await Task.Delay(250);
                selectedBeatmap.SetHighlighting(true);
                await Task.Delay(250);
            }
            await Task.Delay(2000);
            return;
        }

        private Beatmapset GetRandomBeatmap()
        {
            if (BeatmapsetPool.Count < 1)
                return null;

            Random rand = new Random();
            int total = 0;
            foreach (Beatmapset item in BeatmapsetPool)
            {
                total += item.VoteCount;
            }

            int target = rand.Next(1, total);
            int sum = BeatmapsetPool[0].VoteCount;
            for (int i = 1; i < BeatmapsetPool.Count; i++)
            {
                sum += BeatmapsetPool[i].VoteCount;
                if(sum > target)
                {
                    return BeatmapsetPool[i - 1];
                }
            }

            return BeatmapsetPool[0];
        }

        private async void ProcessCommand(object sender, NewCommentReceivedEventArgs e)
        {
            string comment = e.Comment.message;
            if(comment.StartsWith("!"))
            {
                switch (comment.Substring(1, comment.IndexOf(' ') - 1))
                {
                    case "BmReq":
                        await RequestBeatmapSet(comment);
                        break;
                    default:
                        break;
                }
            }
        }

        private async Task RequestBeatmapSet(string comment)
        {
            try
            {
                string keyword = string.Empty;
                if (comment.Substring(comment.IndexOf(' ') + 1).StartsWith("\""))
                {
                    //Search case
                    var trimStart = comment.Substring(comment.IndexOf("\"") + 1);
                    keyword = trimStart.Substring(0, trimStart.LastIndexOf("\""));
                }
                else
                {
                    //ID case
                    keyword = comment.Split(' ')[1];
                }

                var res = await OsuHelper.GetBeatmapsetsAsync(keyword);
                //null check
                if(res != null)
                {
                    var first = res.FirstOrDefault();
                    if(first != null)
                    {
                        var existingBeatmap = BeatmapsetPool.FirstOrDefault(p => p.id == first.id);
                        if (existingBeatmap != null)
                            existingBeatmap.VoteCount++;
                        else
                            BeatmapsetPool.Add(first);
                    }
                }
            }
            catch
            {

            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName]string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private async void Event_Test(object sender, RoutedEventArgs e)
        {
            var res = await OsuHelper.GetBeatmapsetsAsync("No title");
            BeatmapsetPool.Add(res[0]);
        }

        public static int FindObservableCollectionIndex<T>(ObservableCollection<T> collection, Func<T, bool> predicate)
        {
            for (int i = 0; i < collection.Count; i++)
            {
                if (predicate(collection[i]))
                    return i;
            }

            return -1;
        }
    }
}
