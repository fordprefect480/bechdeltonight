using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BechdelTonight.Cache
{
    public static class ScoreCache
    {
        public static int? GetScore(string title)
        {
            var scores = (Dictionary<string, int?>)HttpRuntime.Cache.Get("scores");
            if (scores == null)
            {
                return null;
            }

            if (scores.ContainsKey(title))
            {
                return scores[title];
            }
            else
            {
                return null;
            }
        }

        public static void UpsertScore(string title, int? score)
        {
            var scores = (Dictionary<string, int?>)HttpRuntime.Cache.Get("scores");
            if (scores == null)
            {
                scores = new Dictionary<string, int?>();
            }

            if (scores.ContainsKey(title))
            {
                scores[title] = score;
            }
            else
            {
                scores.Add(title, score);
            }

            HttpRuntime.Cache.Insert("scores", scores);
        }
    }
}