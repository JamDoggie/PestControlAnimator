using PestControlAnimator.shared.animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PestControlAnimator.shared.json
{
    public class ProjectInfoJson
    {
        public ProjectInfoJson()
        {
            animations = new List<Animation>();
        }
        public string ProjectName { get; set; }
        public int ProjectSaveIncrement { get; set; }
        public string ContentPath { get; set; }

        public bool Loop { get; set; } = true;

        public int LoopToFrame { get; set; }

        public int TimelineEnd { get; set; } = 10;

        public List<Animation> animations { get; set; }
    }
}
