﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TwoDECS.Engine.Components
{
    public struct PositionComponent
    {
        public Vector2 Position;
        public bool MoveRight;
        public bool MoveDown;
        public bool MoveLeft;
        public bool MoveUp;
    }
}
