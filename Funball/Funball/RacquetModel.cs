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

namespace Funball
{
    class RacquetModel : BasicModel
    {
        // Racquet's rotation
        public float currentAngle = 0;
        float angle = MathHelper.ToRadians(6);
        float upperBoundAngle = MathHelper.ToRadians(75);
        float lowerBoundAngle = MathHelper.ToRadians(-75);

        // Racquet's swing
        public bool forwardSwing = false;
        bool backwardSwing = false;
        public float currentSwing = 0;
        float lowerBoundSwing = MathHelper.ToRadians(75);
        Vector3 swingAxis = new Vector3(0, 0, 1);

        public RacquetModel(Model m, float Scale)
            : base(m, Vector3.Zero, 0, 0, 0, Scale)
        {
            this.scale = Scale;
            world = Matrix.CreateScale(Scale);
        }

        public override void Update(Camera camera)
        {
            // Move the racquet within the walls
            world.Translation = camera.getCameraPosition() + 150 * camera.getCameraDirection() + new Vector3(0, -75, 0);
            if (world.Translation.Z > 155)
                world.Translation = new Vector3(world.Translation.X, world.Translation.Y, 155);
            if (world.Translation.Z < -355)
                world.Translation = new Vector3(world.Translation.X, world.Translation.Y, -355);

            swingTheRacquet();
        }

        public void swingTheRacquet()
        {
            Vector3 pos = world.Translation;
            // Swing forward until reaching the limit, then swing backward
            if (forwardSwing)
            {
                if (currentSwing < lowerBoundSwing)
                {
                    world = world * Matrix.CreateFromAxisAngle(swingAxis, angle);
                    currentSwing += angle;
                }
                else
                {
                    forwardSwing = false;
                    backwardSwing = true;
                }
            }
            else if (backwardSwing)
            {
                if (currentSwing > 0)
                {
                    world = world * Matrix.CreateFromAxisAngle(swingAxis, -angle);
                    currentSwing -= angle;
                }
                else
                {
                    backwardSwing = false;
                }
            }
            world.Translation = pos;
        }

        public void resetRacquet()
        {
            RacquetRotation(-currentAngle);
            currentAngle = 0;
            currentSwing = 0;
            swingAxis = new Vector3(0, 0, 1);
        }

        public void RotateTheRacquet(Camera camera)
        {
            // Rotate the racquet with Q and E
            if (Keyboard.GetState().IsKeyDown(Keys.Q))
            {
                if (currentAngle - angle > lowerBoundAngle)
                {
                    RacquetRotation(-angle);
                    currentAngle -= angle;
                    // Rotate the swing axis
                    swingAxis = Vector3.Transform(swingAxis, Matrix.CreateRotationX(angle));
                }
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.E))
            {
                if (currentAngle + angle < upperBoundAngle)
                {
                    RacquetRotation(angle);
                    currentAngle += angle;
                    // Rotate the swing axis
                    swingAxis = Vector3.Transform(swingAxis, Matrix.CreateRotationX(-angle));
                }
            }
            if (currentAngle < 0 &&
                camera.cameraPosition.Z - 0.75f * MathHelper.ToDegrees(currentAngle) > 150)
            {
                RacquetRotation(angle);
                currentAngle += angle;
                // Rotate the swing axis
                swingAxis = Vector3.Transform(swingAxis, Matrix.CreateRotationX(-angle));
            }
            if (currentAngle > 0 &&
                camera.cameraPosition.Z - 0.75f * MathHelper.ToDegrees(currentAngle) < -350)
            {
                RacquetRotation(-angle);
                currentAngle -= angle;
                // Rotate the swing axis
                swingAxis = Vector3.Transform(swingAxis, Matrix.CreateRotationX(angle));
            }

        }

        public void RacquetRotation(float rotationAngle)
        {
            Vector3 pos = world.Translation;
            world = world * Matrix.CreateFromAxisAngle(new Vector3(-1, 0, 0), rotationAngle);
            world.Translation = pos;
        }

        public bool CollidesWith(Model otherModel, Matrix otherWorld)
        {
            // Loop through each ModelMesh in both objects and compare
            // all bounding spheres for collisions
            foreach (ModelMesh myModelMeshes in model.Meshes)
            {
                foreach (ModelMesh hisModelMeshes in otherModel.Meshes)
                {
                    if (myModelMeshes.BoundingSphere.Transform(
                        GetWorld()).Intersects(
                        hisModelMeshes.BoundingSphere.Transform(otherWorld)))
                        return true;
                }
            }
            return false;
        }
    }
}
