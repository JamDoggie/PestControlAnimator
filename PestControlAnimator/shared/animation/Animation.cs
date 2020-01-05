using PestControlAnimator.shared.animation.json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PestControlAnimator.shared.animation
{
    public class Animation
    {
        public Animation()
        {
            spriteBoxes = new Dictionary<string, SpriteboxJson>();
        }

        public int timelineX { get; set; }

        // This kinda has to be public because it's for json serializing so ignore the CA2227 standard warning in visual studio here.
        public Dictionary<string, SpriteboxJson> spriteBoxes { get; set; }

        
    }
}
