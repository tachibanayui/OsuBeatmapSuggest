namespace OsuBeatmapRequest.OsuRelated
{
    public class BeatmapRequest
    {
        public Beatmapset[] beatmapsets { get; set; }
        public Cursor cursor { get; set; }
        public float recommended_difficulty { get; set; }
        public int total { get; set; }
    }
}
