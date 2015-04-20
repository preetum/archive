using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnitsAndBuilduings
{
    public class AnimationTypeInfo
    {
        public AnimationTypeInfo() { }
        public string Animation;
        public float FPS;//set to zero for default

        public bool Reverse;

        public RectangleInfo DrawArea;//the X and Y are added to the normal rectangle. The width and height are MULTIPLIED by the normal rectangle.
    }

    public class EffectTypeInfo
    {
        public EffectTypeInfo() { }
        public string Type;
        public string Image;
    }
}
