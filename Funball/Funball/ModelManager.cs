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
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class ModelManager : DrawableGameComponent
    {
        RacquetModel racquet;
        BasicModel room;
        List<BouncingAmmo> balls = new List<BouncingAmmo>();
        AudioEngine audioEngine;
        SoundBank soundBank;
        WaveBank waveBank;

        string currentGameState;

        public Vector3 ballSpeed { get; set; }
        public ModelManager(Game game)
            : base(game)
        {
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            //Load audio content
            audioEngine = new AudioEngine(@"Content\Audio\RacketBallSound.xgs");
            waveBank = new WaveBank(audioEngine, @"Content\Audio\Wave Bank.xwb");
            soundBank = new SoundBank(audioEngine, @"Content\Audio\Sound Bank.xsb");
            
            newBall();
            racquet = new RacquetModel(
                Game.Content.Load<Model>(@"Models/racquet_textured"),
                0.4f);
            room = new BasicModel(
                Game.Content.Load<Model>(@"Models/room_textured"),
               new Vector3(-200, -150, -100),
                0,
                0,
                0,
                1.0f);
            base.LoadContent();
        }

        public void newBall()
        {
            balls.Add(new BouncingAmmo(
                Game.Content.Load<Model>(@"Models/ammo"),
                // positives (toward, up, left)
                new Vector3(-1, 0, 0),
                10f));
        }

        public void resetBallSpeed()
        {
            ballSpeed = new Vector3(
                // away: total hits plus 15
                -(((Game1)Game).totalHits + 15),
                // vertical: random number from 4 to 5
                ((Game1)Game).rnd.Next(11) / 10f + 4,
                // horizontal: random number from 1 to 2 times the camera's direction
                (((Game1)Game).rnd.Next(11) / 10f + 1) * ((Game1)Game).camera.getCameraDirection().X);
        }

        public void getGameState(String currentState) { currentGameState = currentState; }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {

            if (currentGameState == "END") { return; }

            // If the current ball is being held and [Space] is pressed, release it
            if(balls[balls.Count - 1].holdBall &&
                Keyboard.GetState().IsKeyDown(Keys.Space))
              {
                balls[balls.Count - 1].holdBall = false;
                resetBallSpeed();
                balls[balls.Count - 1].setSpeed(ballSpeed);
              }
 
            // If the current ball isn't moving, make a new one
            if (balls[balls.Count - 1].GetSpeed() == Vector3.Zero)
            {
                newBall();
                if (((Game1)Game).totalHits > 0)
                    ((Game1)Game).score -= 10;
                racquet.resetRacquet();
            }

            // Check for collision with the racquet
            if (balls[balls.Count - 1].GetSpeed().X > 0 &&
                racquet.CollidesWith(balls[balls.Count - 1].model, balls[balls.Count - 1].GetWorld()))
            {
                soundBank.PlayCue("Bounce");
                resetBallSpeed();
                balls[balls.Count - 1].setSpeed(ballSpeed);
                balls[balls.Count - 1].setScale(0.95f);
                // Add points and increment hits
                ((Game1)Game).score += 2;
                ((Game1)Game).totalHits++;
                racquet.forwardSwing = true;
            }

            // If the ball bounces off the back wall, deduct points and increment total misses
            if (balls[balls.Count - 1].GetWorld().Translation.X > 160)
            {
                if (((Game1)Game).totalHits > 0)
                    ((Game1)Game).score -= 1;
                ((Game1)Game).totalMisses++;
            }
            
            // Update only the current ball
            balls[balls.Count - 1].Update(((Game1)Game).camera);
            
            // Update the racquet's position
            racquet.Update(((Game1)Game).camera);
            if (!balls[balls.Count - 1].holdBall)
                racquet.RotateTheRacquet(((Game1)Game).camera);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (currentGameState == "END")
            {
                return;
            }
            room.Draw(((Game1)Game).camera);
            racquet.Draw(((Game1)Game).camera);

            foreach (BasicModel bm in balls)
            {
                bm.Draw(((Game1)Game).camera);
            }

            base.Draw(gameTime);
        }
    }
}
