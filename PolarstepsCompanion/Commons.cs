using System;
using System.Collections.Generic;
using System.Text;

namespace PolarstepsCompanion
{
    public static class Commons
    {
        static public string TimeSpanPretty(TimeSpan timeSpan)
        {
            TimeSpan absTimeSpan = timeSpan.Ticks > 0 ? timeSpan : -timeSpan;


            return String.Format("{0} days, {1} hours, {2} minutes {3} and seconds {4}", 
                absTimeSpan.Days, 
                absTimeSpan.Hours, 
                absTimeSpan.Minutes, 
                absTimeSpan.Seconds, 
                timeSpan.Ticks > 0 ? "forward" : "backward");
        }
    }
}
