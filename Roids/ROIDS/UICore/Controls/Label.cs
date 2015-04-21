using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Utilities;

namespace UICore.Controls
{
    public class Label : TextControl
    {
        public Label(Vector2 location, string text, Element parent)
            : base(location, text, parent)
        { }
    }
}
