using System;

namespace OsuBeatmapRequest.OsuRelated
{
    public class Beatmap
    {
        public int id { get; set; }
        public int beatmapset_id { get; set; }
        public string mode { get; set; }
        public int mode_int { get; set; }
        public object convert { get; set; }
        public float difficulty_rating { get; set; }
        public string version { get; set; }
        public int total_length { get; set; }
        public int hit_length { get; set; }
        public float cs { get; set; }
        public float drain { get; set; }
        public float accuracy { get; set; }
        public float ar { get; set; }
        public int playcount { get; set; }
        public int passcount { get; set; }
        public int count_circles { get; set; }
        public int count_sliders { get; set; }
        public int count_spinners { get; set; }
        public int count_total { get; set; }
        public DateTime last_updated { get; set; }
        public int ranked { get; set; }
        public string status { get; set; }
        public string url { get; set; }
        public object deleted_at { get; set; }
    }
}
