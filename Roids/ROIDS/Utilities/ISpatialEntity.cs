using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
namespace Utilities
{
    public delegate void SpatialStateChangeHandler(ISpatialEntity sender);

    public interface ISpatialEntity
    {
        float Rotation { get; set;  }
        Vector2 Position { get; set; }     
    }
}
