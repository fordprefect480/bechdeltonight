using BechdelTonight.Cache;
using BechdelTonight.Extensions;
using BechdelTonight.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Xml;

namespace BechdelTonight.Controllers
{
    public class HomeController : Controller
    {
        private const int MovieLengthMinutes = 90;

        private Random _random = new Random();

        public ActionResult Index()
        {
            ViewBag.CurrentState = State.SA;
            ViewBag.CurrentChannel = Channel.Seven;
            ViewBag.CurrentDate = DateTime.Today;

            return View();
        }

        public async Task<ActionResult> _Guide(State? state, Channel? channel, int? days)
        {
            try
            {
                var model = await GetGuide(
                    state ?? State.SA, 
                    channel ?? Channel.Seven,
                    DateTime.Today.AddDays(days ?? 0));

                return PartialView(model);
            }
            catch (Exception e)
            {
                return Json(new { Success = false, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }       
        
        public async Task<List<Programme>> GetGuide(State state, Channel channel, DateTime date)
        {
            var guide = new XmlDocument();
            var tvXML = await GetGuideXML(state, channel, date);
            guide.LoadXml(tvXML);

            var programmeNodes = guide.SelectNodes("/tv/programme");
            var programmes = new List<Programme>();
            foreach (XmlNode programmeNode in programmeNodes)
            {
                var title = programmeNode.SelectSingleNode("title").InnerText;
                DateTime.TryParseExact(programmeNode.Attributes["start"].Value, "yyyyMMddHHmmss zzz", null, DateTimeStyles.AssumeLocal, out var startTime);
                DateTime.TryParseExact(programmeNode.Attributes["stop"].Value, "yyyyMMddHHmmss zzz", null, DateTimeStyles.AssumeLocal, out var finishTime);

                if (finishTime.Subtract(startTime) < TimeSpan.FromMinutes(MovieLengthMinutes))
                {
                    // Not long enough to be a movie
                    continue;
                }

                var programme = ProgrammeFactory.Create(title, startTime, await GetScore(title));               
                programmes.Add(programme);
            }

            ViewBag.CurrentState = state;
            ViewBag.CurrentChannel = channel;
            ViewBag.CurrentDate = date;

            return programmes;
        }

        private async Task<int?> GetScore(string title)
        {
            var scores = (Dictionary<string, int?>)HttpRuntime.Cache.Get("scores");
            var score = ScoreCache.GetScore(title);
            if (score == null)
            {
                // Get score
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync("http://bechdeltest.com/api/v1/getMoviesByTitle?title=" + HttpUtility.UrlEncode(title));
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new FileNotFoundException(string.Format("Could not find Bechdel score for '{0}'.", title));
                    }

                    var rawJson = await response.Content.ReadAsStringAsync();
                    JArray array = JArray.Parse(rawJson);
                    if (array.Count != 0)
                    {
                        score = array.First.Value<int>("rating");
                    }
                }

                // Put in cache
                ScoreCache.UpsertScore(title, score);
            }

            return (int?) score;
        }

        private async Task<string> GetGuideXML(State state, Channel channel, DateTime date)
        {
            string tvXML;
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(BuildURL(state, channel, date));
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new FileNotFoundException(string.Format("Could not find guide data for {0}.", date.FormatForGuide()));
                }

                var bytes = await response.Content.ReadAsByteArrayAsync();
                tvXML = Unzip(bytes);
            }

            return tvXML;
        }

        private string BuildURL(State state, Channel channel, DateTime date)
        {
            var urlFormat = "http://www.oztivo.net/xmltv/{0}_{1}-{2}-{3}.xml.gz";

            string channelString;
            switch(channel)
            {
                case Channel.Seven:
                    channelString = "Seven-" + state.GetDescription();
                    break;
                case Channel.SevenTwo:
                    channelString = "7TWO-" + state.GetDescription();
                    break;
                case Channel.SevenMate:
                    channelString = "7mate";
                    break;
                case Channel.SevenFlix:
                    channelString = "7flix";
                    break;
                case Channel.SBS:
                    channelString = "SBS-" + state.GetDescription();
                    break;
                case Channel.Nine:
                    channelString = "Nine-" + state.GetDescription();
                    break;
                case Channel.NineLife:
                    channelString = "9Life";
                    break;

                default:
                    throw new ArgumentOutOfRangeException(string.Format("Unexpected channel selected: {0}", channel));
            }

            return string.Format(urlFormat, channelString, date.Year, date.Month.ToString().PadLeft(2, '0'), date.Day.ToString().PadLeft(2, '0'));
        }

        public static string Unzip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    CopyTo(gs, mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }

        public static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }


        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}