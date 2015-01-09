using System;
using System.Collections.Generic;
using System.Linq;
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
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Camera : Microsoft.Xna.Framework.GameComponent
    {
        // Camera matrices
        public Matrix view { get; protected set; }
        public Matrix projection { get; protected set; }

        // Camera vectors
        public Vector3 cameraPosition { get; protected set; }

        //Vector3 cameraTarget;

        protected Vector3 cameraDirection;
        //public Vector3 cameraDirection { get; protected set; }

        protected Vector3 cameraUp;
        protected float speed = 6;


        // Max pitch variables
        protected float totalPitch = MathHelper.PiOver4;
        protected float currentPitch = 0;

        // Adjust addition Yaw angle
        protected float totalYaw = MathHelper.PiOver4;
        protected float currentYaw = 0;

        protected MouseState prevMouseState;

        public Camera(Game game, Vector3 pos, Vector3 target, Vector3 up)
            : base(game)
        {
            // Build camera view matrix
            cameraPosition = pos;
            cameraDirection = target - pos;
            cameraDirection.Normalize();
            cameraUp = up;
            CreateLookAt();

            // Initialize projection matrix
            projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4,
                (float)Game.Window.ClientBounds.Width /
                (float)Game.Window.ClientBounds.Height,
                1, 3000);
        }

        protected void CreateLookAt()
        {
            view = Matrix.CreateLookAt(
                cameraPosition,
                cameraPosition + cameraDirection,
                cameraUp);
        }

        public Vector3 getCameraDirection() { return this.cameraDirection; }
        public Vector3 getCameraPosition() { return this.cameraPosition; }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // Set mouse position and do initial get state
            Mouse.SetPosition(Game.Window.ClientBounds.Width >> 1,
                Game.Window.ClientBounds.Height >> 1);
            prevMouseState = Mouse.GetState();

            base.Initialize();
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

            // Move side to side
            if (Keyboard.GetState().IsKeyDown(Keys.A))
                cameraPosition += Vector3.Cross(cameraUp, cameraDirection) * speed;
            if (Keyboard.GetState().IsKeyDown(Keys.D))
                cameraPosition -= Vector3.Cross(cameraUp, cameraDirection) * speed;

            // Reset prevMouseState
            prevMouseState = Mouse.GetState();

            // Recreate the camera view matrix
            CreateLookAt();

            base.Update(gameTime);
        }

    }
}
