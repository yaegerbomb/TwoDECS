using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwoDECS.Engine.Cameras;
using TwoDECS.Engine.Components;
using TwoDECS.Engine.World;

namespace TwoDECS.Engine.Systems
{
    public static class PlayerInputSystem
    {
        public static float TendToZero(float val, float amount)
        {
            if (val > 0f && (val -= amount) < 0f) return 0f;
            if (val < 0f && (val += amount) > 0f) return 0f;
            return val;
        }

        public static void HandlePlayerMovement(PlayingState playingState, GraphicsDeviceManager graphics, GameTime gameTime, KeyboardState previousKeyboardState, 
            MouseState previousMouseState, GamePadState previousGamepadState, FollowCamera followCam, TileMap tileMap, LevelCollisionDetection levelCollisionDetection)
        {
            List<Guid> moveableEntities = playingState.Entities.Where(x => (x.ComponentFlags & ComponentMasks.PlayerInput) == ComponentMasks.PlayerInput).Select(x => x.ID).ToList();
            foreach (Guid id in moveableEntities)
            {
                var directionComponent = playingState.DirectionComponents[id];

                var positionComponent = playingState.PositionComponents[id];
                Vector2 position = positionComponent.Position;
                Rectangle destination = positionComponent.Destination;

                VelocityComponent velocityComponent = playingState.VelocityComponents[id];

                AccelerationComponent accelerationComponent = playingState.AccelerationComponents[id];

                #region gamepad controls
                GamePadState currentGamePadState = GamePad.GetState(PlayerIndex.One);

                //position.X += currentGamePadState.ThumbSticks.Left.X * playerMoveSpeed;
                //position.Y -= currentGamePadState.ThumbSticks.Left.Y * playerMoveSpeed;
                //previousGamepadState = currentGamePadState;
                #endregion


                #region keyboard controls
                KeyboardState CurrentKeyboardState = Keyboard.GetState();
                if (CurrentKeyboardState.IsKeyDown(Keys.Left) || CurrentKeyboardState.IsKeyDown(Keys.A) || currentGamePadState.DPad.Left == ButtonState.Pressed)
                {
                    velocityComponent.xVelocity -= accelerationComponent.xAcceleration;
                    velocityComponent.xVelocity = MathHelper.Clamp(velocityComponent.xVelocity, -1 * velocityComponent.xTerminalVelocity, 0);


                    position.X += velocityComponent.xVelocity;
                    position = levelCollisionDetection.CheckWallCollision(position, Direction.Left);
                }

                if (CurrentKeyboardState.IsKeyDown(Keys.Right) || CurrentKeyboardState.IsKeyDown(Keys.D) || currentGamePadState.DPad.Right == ButtonState.Pressed)
                {
                    velocityComponent.xVelocity += accelerationComponent.xAcceleration;
                    velocityComponent.xVelocity = MathHelper.Clamp(velocityComponent.xVelocity, 0, velocityComponent.xTerminalVelocity);


                    position.X += velocityComponent.xVelocity;
                    position = levelCollisionDetection.CheckWallCollision(position, Direction.Right);
                }

                //move player x axis



                if (CurrentKeyboardState.IsKeyDown(Keys.Up) || CurrentKeyboardState.IsKeyDown(Keys.W) || currentGamePadState.DPad.Up == ButtonState.Pressed)
                {

                    velocityComponent.yVelocity -= accelerationComponent.yAcceleration;
                    velocityComponent.yVelocity  = MathHelper.Clamp(velocityComponent.yVelocity, -1 * velocityComponent.yTerminalVelocity, 0 );

                    position.Y += velocityComponent.yVelocity;


                    position = levelCollisionDetection.CheckWallCollision(position, Direction.Up);
                }



                if (CurrentKeyboardState.IsKeyDown(Keys.Down) || CurrentKeyboardState.IsKeyDown(Keys.S) || currentGamePadState.DPad.Down == ButtonState.Pressed)
                {
                    velocityComponent.yVelocity += accelerationComponent.yAcceleration;
                    velocityComponent.yVelocity = MathHelper.Clamp(velocityComponent.yVelocity, 0, velocityComponent.yTerminalVelocity);
                    position.Y += velocityComponent.yVelocity;

                    position = levelCollisionDetection.CheckWallCollision(position, Direction.Down);
                }

                if (CurrentKeyboardState.IsKeyUp(Keys.Down) && CurrentKeyboardState.IsKeyUp(Keys.S) && CurrentKeyboardState.IsKeyUp(Keys.Up) && CurrentKeyboardState.IsKeyUp(Keys.W))
                {
                    velocityComponent.yVelocity = 0;
                }
                if (CurrentKeyboardState.IsKeyUp(Keys.Left) && CurrentKeyboardState.IsKeyUp(Keys.A) && CurrentKeyboardState.IsKeyUp(Keys.D) && CurrentKeyboardState.IsKeyUp(Keys.Right))
                {
                    velocityComponent.xVelocity = 0;
                }

                previousKeyboardState = CurrentKeyboardState;

                #endregion


                #region mouse controls
                //face player towards mouse

                MouseState currentMouseState = Mouse.GetState();

                destination = new Rectangle((int)position.X, (int)position.Y, destination.Width, destination.Height);

                Vector2 mouseLocation = new Vector2(currentMouseState.X + followCam.Center.X, currentMouseState.Y + followCam.Center.Y);

                Vector2 direction = mouseLocation - position;
                direction.Normalize();

                directionComponent.Direction = (float)Math.Atan2((double)direction.Y, (double)direction.X);


                //if (currentMouseState.LeftButton == ButtonState.Pressed)
                //{
                //    List<Guid> weaponEntities = playingState.Entities.Where(x => (x.ComponentFlags & ComponentMasks.Weapon) == ComponentMasks.Weapon).Select(x => x.ID).ToList();
                //    foreach (Guid wid in weaponEntities)
                //    {

                //        if (playingState.OwnerComponents.ContainsKey(wid))
                //        {
                //            OwnerComponent ownerComponent = playingState.OwnerComponents[wid];
                //            if (ownerComponent.OwnerID == id)
                //            {
                //                Guid projectileId = playingState.CreateEntity();
                //                playingState.Entities.Where(x => x.ID == projectileId).First().ComponentFlags = ComponentMasks.Projectile;


                //                playingState.DirectionComponents[projectileId] = new DirectionComponent() { Direction = directionComponent.Direction };
                //                playingState.DisplayComponents[projectileId] = new DisplayComponent() { Source = new Rectangle(21, 504, destination.Width, destination.Height) };
                //                playingState.SpeedComponents[projectileId] = new SpeedComponent() { Speed = 15f };
                //                playingState.PositionComponents[projectileId] = new PositionComponent() { Position = position, Destination = new Rectangle((int)position.X, (int)position.Y, destination.Width, destination.Height) };
                //                playingState.DamageComponents[projectileId] = new DamageComponent() { Damage = 10 };
                //                playingState.OwnerComponents[projectileId] = new OwnerComponent() { OwnerID = wid };
                //            }
                //        }
                //    }
                //}
                



                previousMouseState = currentMouseState;
                #endregion
                
                playingState.DirectionComponents[id] = directionComponent;

                positionComponent.Position = position;
                positionComponent.Destination = destination;
                playingState.PositionComponents[id] = positionComponent;

                playingState.VelocityComponents[id] = velocityComponent;
                playingState.AccelerationComponents[id] = accelerationComponent;
            }
        }
    }
}
