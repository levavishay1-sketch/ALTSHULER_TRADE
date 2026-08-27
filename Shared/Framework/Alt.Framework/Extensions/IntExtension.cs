using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alt.Framework.Extensions
{
    public static class IntExtension
    {
        public static string ConvertSecondsToMMSSFormat(this int seconds)
        {
            int mins = seconds / 60;
            int secs = seconds % 60;

            return $"{mins}:{secs}";
        }
    }
}
