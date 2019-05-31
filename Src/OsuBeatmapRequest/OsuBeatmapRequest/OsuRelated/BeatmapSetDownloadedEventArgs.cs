using System;

namespace OsuBeatmapRequest.OsuRelated
{
    public class BeatmapSetDownloadedEventArgs : EventArgs
    {
        public string FileLocation { get; set; }
        public int BeatmapSetID { get; set; }
    }
}
