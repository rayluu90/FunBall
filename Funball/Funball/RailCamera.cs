using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace Funball
{
    class RailCamera : Camera
    {
        protected Vector3 railLocation;
        protected const int LEFT_LIMIT = 150;
        protected const int RIGHT_LIMIT = -350;
        Vector3 velocity;
        bool isJump = false;

        public RailCamera(Game game, Vector3 pos, Vector3 target, Vector3 up)
            : base(game, pos, target, up)
        {
            railLocation = pos;
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // Limit the Yaw Direction
            float yawAngle = (-MathHelper.PiOver4 / 150) * (Mouse.GetState().X - prevMouseState.X);

            if (Math.Abs(currentYaw + yawAngle) < totalYaw)
            {
                cameraDirection = Vector3.Transform(
                    cameraDirection,
                    Matrix.CreateFromAxisAngle(cameraUp, yawAngle));

                currentYaw += yawAngle;
            }

            // Limited pitch camera
            float pitchAngle = (MathHelper.PiOver4 / 150) *
                (Mouse.GetState().Y - prevMouseState.Y);

            if (Math.Abs(currentPitch + pitchAngle) < totalPitch)
            {
                cameraDirection = Vector3.Transform(
                    cameraDirection,
                    Matrix.CreateFromAxisAngle(
                        Vector3.Cross(cameraUp, cameraDirection),
                        pitchAngle));

                currentPitch += pitchAngle;
            }

            cameraJump(10, 0.15f);

            // Move side to side
            railLocation = cameraPosition;
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                if (railLocation.Z < LEFT_LIMIT)
                {
                    railLocation.Z = railLocation.Z + speed;
                    cameraPosition = railLocation;
                }
            }

            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                if (railLocation.Z > RIGHT_LIMIT )
                {
                    railLocation.Z = railLocation.Z - speed;
                    cameraPosition = railLocation;
                }
            }

            // Reset prevMouseState
            prevMouseState = Mouse.GetState();

            // Recreate the camera view matrix
            CreateLookAt();
        }
        public void cameraJump(int intVelocity, float fallSpeed)
        {
            velocity = new Vector3(0, intVelocity, 0);
            if (isJump == true)
            {
                velocity.Y += fallSpeed;
                //If hit top
                if (cameraPosition.Y > 300) { isJump = false; }
                cameraPosition += velocity;
            }
            else if (cameraPosition.Y > 50)
            {
                cameraPosition -= velocity;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                    isJump = true;
            }
          
        }
    }
}
