using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TwoDECS.Engine.Components
{
    public enum DamageTags
    {
        Player,
        Enemy
    }
    public struct DamageComponent
    {
        public int Damage;
        public int AttackRange;
        public float Cooldown;
        public DamageTags DamageTag;
        public int Quantity;
    }
}
