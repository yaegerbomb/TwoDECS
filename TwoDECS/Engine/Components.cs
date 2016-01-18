﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwoDECS.Engine.Components;

namespace TwoDECS.Engine
{
    [Flags]
    public enum Component : ulong
    {
        NONE = 0,
        COMPONENT_DIRECTION = 1 << 0,
        COMPONENT_DISPLAY = 1 << 1,
        COMPONENT_HEALTH = 1 << 2,
        COMPONENT_POSITION = 1 << 3,
        COMPONENT_SPEED = 1 << 4,
        COMPONENT_PLAYERINPUT = 1 << 5,
        COMPONENT_CONTAINER = 1 << 6,
        COMPONENT_TIMER = 1 << 7,
        COMPONENT_DAMAGE = 1 << 8,
        COMPONENT_AABB = 1 << 9,
        COMPONENT_MOUSEINPUT = 1 << 10,
        COMPONENT_ACTIVE = 1 << 11,
        COMPONENT_OWNER = 1 << 12,
        COMPONENT_AI = 1 << 13
    }

    public struct ComponentMasks
    {
        #region player components
        public const Component Player = Component.COMPONENT_DIRECTION | Component.COMPONENT_DISPLAY | Component.COMPONENT_HEALTH | Component.COMPONENT_POSITION | Component.COMPONENT_SPEED | Component.COMPONENT_PLAYERINPUT | Component.COMPONENT_CONTAINER | Component.COMPONENT_AABB | Component.COMPONENT_DAMAGE;
        public const Component PlayerInput = Component.COMPONENT_POSITION | Component.COMPONENT_PLAYERINPUT;
        #endregion

        #region enemy components
        public const Component Enemy = Component.COMPONENT_DIRECTION | Component.COMPONENT_DISPLAY | Component.COMPONENT_HEALTH | Component.COMPONENT_POSITION | Component.COMPONENT_SPEED | Component.COMPONENT_AABB | Component.COMPONENT_DAMAGE | Component.COMPONENT_AI;
        #endregion


        public const Component Projectile = Component.COMPONENT_DIRECTION | Component.COMPONENT_DISPLAY | Component.COMPONENT_POSITION | Component.COMPONENT_SPEED | Component.COMPONENT_AABB | Component.COMPONENT_ACTIVE | Component.COMPONENT_OWNER | Component.COMPONENT_DAMAGE;
        public const Component Weapon = Component.COMPONENT_DIRECTION | Component.COMPONENT_DISPLAY | Component.COMPONENT_POSITION | Component.COMPONENT_TIMER | Component.COMPONENT_CONTAINER | Component.COMPONENT_AABB | Component.COMPONENT_MOUSEINPUT | Component.COMPONENT_ACTIVE | Component.COMPONENT_OWNER | Component.COMPONENT_DAMAGE;

        public const Component Drawable = Component.COMPONENT_DIRECTION | Component.COMPONENT_DISPLAY | Component.COMPONENT_POSITION;
    }

    public class PlayingState
    {
        public List<Entity> Entities { get; set; }
        public List<Guid> EntitiesToDelete { get; set; }
        public Dictionary<Guid, DirectionComponent> DirectionComponents { get; set; }
        public Dictionary<Guid, DisplayComponent> DisplayComponents { get; set; }
        public Dictionary<Guid, HealthComponent> HealthComponents { get; set; }
        public Dictionary<Guid, PositionComponent> PositionComponents { get; set; }
        public Dictionary<Guid, SpeedComponent> SpeedComponents { get; set; }
        public Dictionary<Guid, ContainerComponent> ContainerComponents { get; set; }
        public Dictionary<Guid, TimerComponent> TimerComponents { get; set; }
        public Dictionary<Guid, DamageComponent> DamageComponents { get; set; }
        public Dictionary<Guid, OwnerComponent> OwnerComponents { get; set; }

        public PlayingState()
        {
            Entities = new List<Entity>();
            EntitiesToDelete = new List<Guid>();
            DirectionComponents = new Dictionary<Guid, DirectionComponent>();
            DisplayComponents = new Dictionary<Guid, DisplayComponent>();
            HealthComponents = new Dictionary<Guid, HealthComponent>();
            PositionComponents = new Dictionary<Guid, PositionComponent>();
            SpeedComponents = new Dictionary<Guid, SpeedComponent>();
            ContainerComponents = new Dictionary<Guid, ContainerComponent>();
            TimerComponents = new Dictionary<Guid, TimerComponent>();
            DamageComponents = new Dictionary<Guid, DamageComponent>();
            OwnerComponents = new Dictionary<Guid, OwnerComponent>();
        }        

        public Guid CreateEntity()
        {
            Entity entity = new Entity();
            this.Entities.Add(entity);
            return entity.ID;
        }

        public void DestroyEntity(Guid id)
        {
            Entity remove = this.Entities.Where(x => x.ID == id).FirstOrDefault();
            if (remove != null)
            {
                Entities.Remove(remove);
                DirectionComponents.Remove(id);
                DisplayComponents.Remove(id);
                HealthComponents.Remove(id);
                PositionComponents.Remove(id);
                SpeedComponents.Remove(id);
            }
        }
    }


}