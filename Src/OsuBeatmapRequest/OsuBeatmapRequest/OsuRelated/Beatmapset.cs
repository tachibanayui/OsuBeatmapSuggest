using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace OsuBeatmapRequest.OsuRelated
{
    public class Beatmapset : INotifyPropertyChanged
    {
        public int id { get; set; }
        public string title { get; set; }
        public string artist { get; set; }
        public int play_count { get; set; }
        public int favourite_count { get; set; }
        public bool has_favourited { get; set; }
        public DateTime submitted_date { get; set; }
        public DateTime last_updated { get; set; }
        public DateTime ranked_date { get; set; }
        public string creator { get; set; }
        public int user_id { get; set; }
        public float bpm { get; set; }
        public string source { get; set; }
        public Covers covers { get; set; }
        public string preview_url { get; set; }
        public string tags { get; set; }
        public bool video { get; set; }
        public bool storyboard { get; set; }
        public int ranked { get; set; }
        public string status { get; set; }
        public bool has_scores { get; set; }
        public bool discussion_enabled { get; set; }
        public bool discussion_locked { get; set; }
        public bool can_be_hyped { get; set; }
        public Hype hype { get; set; }
        public Nominations nominations { get; set; }
        public string legacy_thread_url { get; set; }
        public Beatmap[] beatmaps { get; set; }

        //Custom
        private int _VoteCount = 1;
        public int VoteCount
        {
            get => _VoteCount;
            set
            {
                if(_VoteCount != value)
                {
                    _VoteCount = value;
                    OnPropertyChanged();
                }
            }
        }

        private SolidColorBrush _BorderBrush;
        public SolidColorBrush BorderBrush
        {
            get => _BorderBrush;
            set
            {
                if(_BorderBrush != value)
                {
                    _BorderBrush = value;
                    OnPropertyChanged();
                }
            }
        }

        public Beatmapset()
        {
            SetHighlighting(false);
        }

        public void SetHighlighting(bool isHighlight)
        {
            BorderBrush = isHighlight ? new SolidColorBrush(Colors.Orange) : new SolidColorBrush(Colors.Transparent);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName]string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
