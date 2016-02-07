using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TwoDECS.Engine.Components
{
    public enum MapObjectType
    {
        Wall,
        Door,
        Window,
        Terminal,
        Gun,
        Crate,
        Medkit,
        Sanity,
        Mana
    }

    public struct MapObjectComponent
    {
        public MapObjectType Type;
    }
}
