using System;
using System.Collections.Generic;

namespace BechdelTonight.Models
{
    public class GuideViewModel
    {
        public List<Programme> Programmes { get; set; }

        public Channel CurrentChannel { get; set; }
        public State CurrentState { get; set; }
        public DateTime CurrentDate { get; set; }
    }
}