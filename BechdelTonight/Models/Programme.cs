using System;

namespace BechdelTonight.Models
{
    public class Programme
    {
        public int? Score { get; set; }
        public string ScoreExplanation { get; set; }

        public string Title { get; internal set; }
        public DateTime Time { get; set; }
    }
}