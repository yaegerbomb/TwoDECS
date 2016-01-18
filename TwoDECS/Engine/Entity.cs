using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TwoDECS.Engine
{
    public class Entity
    {
        public Guid ID { get; set; }
        public Component ComponentFlags { get; set; }

        public Entity()
        {
            ID = Guid.NewGuid();
            ComponentFlags = Component.NONE;
        }
    }
}
