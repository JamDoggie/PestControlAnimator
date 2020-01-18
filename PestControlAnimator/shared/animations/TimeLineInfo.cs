using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PestControlAnimator.shared.animations
{
    public static class TimeLineInfo
    {
        public static bool isPlaying { get; set; } = false;
        public static double timelineMs { get; set; } = 0;

    }
}
