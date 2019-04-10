using BechdelTonight.Constants;
using System;

namespace BechdelTonight.Models
{
    public class ProgrammeFactory
    {
        private ProgrammeFactory()
        {}

        public static Programme Create(string title, DateTime time, int? score)
        {
            return new Programme
            {
                Title = title,
                Time = time,
                Score = score,
                ScoreExplanation = score.HasValue ? 
                    string.Format(
                       @"<span class='{0}'><i class='fas fa-{1}'></i></span>{2}<br/>
                         <span class='{3}'><i class='fas fa-{4}'></i></span>{5}<br/>
                         <span class='{6}'><i class='fas fa-{7}'></i></span>{8}<br/>",
                       score > 0 ? "text-success" : "text-danger",
                       score > 0 ? "check" : "times",
                       Bechdel.Rule1,
                       score > 1 ? "text-success" : "text-danger",
                       score > 1 ? "check" : "times",
                       Bechdel.Rule2,
                       score > 2 ? "text-success" : "text-danger",
                       score > 2 ? "check" : "times",
                       Bechdel.Rule3) :
                    "<span/>"
            };
        }
    }
}