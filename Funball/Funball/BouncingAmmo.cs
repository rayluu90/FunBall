using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;


namespace Funball
{
    class BouncingAmmo : BasicModel
    {
        Matrix rotation = Matrix.Identity;

        // Rotation and movement variables
        float yawAngle = 0;
        float pitchAngle = 0;
        float rollAngle = 0;
        Vector3 speed;
        public Vector3 startingSpeed { get; protected set; }

        public bool isCollided = false;
        double floor = -133.5;

        // Audio
        AudioEngine audioEngine;
        WaveBank waveBank;
        SoundBank soundBank;

        public bool holdBall { get; set; }

        public BouncingAmmo(Model m, Vector3 Speed, float Scale)
            : base(m, Vector3.Zero, 0, 0, 0, Scale)
        {
            startingSpeed = Speed;
            setSpeed(startingSpeed);
            this.scale = Scale;
            holdBall = true;
            world = Matrix.CreateScale(Scale);

            //Load audio content
            //Bounce sounds found at 
            //http://www.grsites.com/archive/sounds/category/4/?offset=120
            audioEngine = new AudioEngine(@"Content\Audio\RacketBallSound.xgs");
            waveBank = new WaveBank(audioEngine, @"Content\Audio\Wave Bank.xwb");
            soundBank = new SoundBank(audioEngine, @"Content\Audio\Sound Bank.xsb");
        }

        public void setScale(float newScale)
        {
            scale *= newScale;
            Vector3 pos = world.Translation;
            world = Matrix.CreateScale(scale);
            world.Translation = pos;
        }
        public void setSpeed(Vector3 value) { speed = value; }
        public Vector3 GetSpeed() { return this.speed; }

        public override void Update(Camera camera)
        {
            // Don't move or spin the ball if it's being held
            if (!holdBall)
            {
                yawAngle = MathHelper.ToRadians(-speed.Z / 100f);
                pitchAngle = MathHelper.ToRadians(-speed.Y / 100f);
                rollAngle = MathHelper.ToRadians(-speed.X / 100f);
                // Rotate model
                rotation *= Matrix.CreateFromYawPitchRoll(
                    yawAngle,
                    pitchAngle,
                    rollAngle);

                // Bounce off the walls, lose some speed
                // Forward and back
                if (world.Translation.X < -820)
                {
                    speed.X = .8f * Math.Abs(speed.X);
                    soundBank.PlayCue("Bounce");
                }
                if (world.Translation.X > 160)
                {  // off the back wall
                    speed.X = -Math.Abs(speed.X);
                    soundBank.PlayCue("Bounce");
                }

                if (isCollided)
                {
                    floor -= floor * 0.005f;
                    isCollided = false;
                }

                // Up and down
                if (world.Translation.Y < floor)
                {
                    float beScaleDown = scale;
                    
                    speed.Y = .8f * Math.Abs(speed.Y);
                    soundBank.PlayCue("Bounce");
                }
                if (world.Translation.Y > 450)
                {
                    speed.Y = -.8f * Math.Abs(speed.Y);
                    soundBank.PlayCue("Bounce");
                }
                // Left and right
                if (world.Translation.Z < -380)
                {
                    speed.Z = .8f * Math.Abs(speed.Z);
                    soundBank.PlayCue("Bounce");
                }
                if (world.Translation.Z > 190)
                {
                    speed.Z = -.8f * Math.Abs(speed.Z);
                    soundBank.PlayCue("Bounce");
                }

                // Slow down while rolling along the floor
                if (Math.Abs(speed.Y) < .05f &&
                    world.Translation.Y < floor - 1)
                {
                    speed.Y = 0;
                    if (Math.Abs(speed.Z) > 0)
                        speed.Z *= .95f;
                    if (Math.Abs(speed.Z) < .1)
                        speed.Z = 0;
                    if (Math.Abs(speed.X) > 0)
                        speed.X *= .95f;
                    if (Math.Abs(speed.X) < .1)
                        speed.X = 0;
                }
                else
                    // React to gravity
                    speed.Y -= .075f;

                // Move model
                world = rotation * GetWorld() * Matrix.CreateTranslation(speed);
            }
           else
            // Move the ball with the camera
            {
                world.Translation = new Vector3(-(camera.getCameraPosition().X + camera.getCameraDirection().X) + 150, // toward
                    (camera.getCameraPosition().Y + 150 * camera.getCameraDirection().Y), // up
                    camera.getCameraPosition().Z + 150 * camera.getCameraDirection().Z); // left

                if (world.Translation.Z > 155)
                    world.Translation = new Vector3(world.Translation.X, world.Translation.Y, 155);
                if (world.Translation.Z < -355)
                    world.Translation = new Vector3(world.Translation.X, world.Translation.Y, -355);
            }
        }
    }
}