using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Funball
{
    class BasicModel 
    {
        public Model model { get; protected set; }
        protected Matrix world = Matrix.Identity;
       
        public BasicModel(Model m) { model = m; }
        
        public BasicModel(Model m, Vector3 Position,
            float yaw, float pitch, float roll, float Scale)
        {
            model = m;
            this.scale = Scale;

            world = Matrix.CreateScale(Scale) *
                Matrix.CreateFromYawPitchRoll(yaw, pitch, roll) *
                Matrix.CreateTranslation(Position);
        }

        public float scale { get; set; }
        public virtual Matrix GetWorld() { return world; }
        public void setWorld(Matrix world) { this.world = world; }

        public virtual void Update(Camera camera)
        {
        }

        public void Draw(Camera camera)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect be in mesh.Effects)
                {
                    be.EnableDefaultLighting();
                    be.Projection = camera.projection;
                    be.View = camera.view;
                    be.World = GetWorld() * mesh.ParentBone.Transform;
                }

                mesh.Draw();
            }
        }
    }
}
