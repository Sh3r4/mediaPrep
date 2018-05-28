namespace mediaPrep.Model
{
    public class MediaPrepFile
    {
        public string Directory { get; set; }
        public string FileName { get; set; }
        public string ShowOrMovieName { get; set; }
        public int? SeasonNumber { get; set; }
        public int? EpisodeNumber { get; set; }
        public int? CompactedEpisodesEndNumber { get; set; }
        public int? PartNumber { get; set; }
        public string EpisodeTitle { get; set; }
    }
}