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
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // Model stuff
        ModelManager modelManager;

        // Camera
        public Camera camera { get; protected set; }

        // Random
        public Random rnd { get; protected set; }
        int millisecondDelay = 3000;

        public int score { get; set; }
        public int totalHits { get; set; }
        public int totalMisses { get; set; }

        // Audio
        AudioEngine audioEngine;
        WaveBank waveBank;
        SoundBank soundBank;
        Cue trackCue;

        // Game state items
        public enum GameState { START, PLAY, GAMEOVER, END }
        public GameState currentGameState = GameState.START;

        // Splash screen
        SplashScreen splashScreen;
        SpriteFont splashScreenFontLarge;
        SpriteFont scoreFont;
        RollingCredits credits;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            rnd = new Random();

            // Set preferred resolution
            //graphics.PreferredBackBufferWidth = 800;
            //graphics.PreferredBackBufferHeight = 600;

            // If not running debug, run in full screen
            #if !DEBUG
            graphics.IsFullScreen = true;
            #endif
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Initialize Camera
            camera = new RailCamera(
                this,
                new Vector3(150, 50, -100), // Position
                new Vector3(0, 50, -100), // Target
                Vector3.Up);
            Components.Add(camera);

            // Initialize model manager
            modelManager = new ModelManager(this);
            Components.Add(modelManager);
            modelManager.Enabled = true;
            modelManager.Visible = false;

            // Splash screen component
            splashScreen = new SplashScreen(this);
            Components.Add(splashScreen);
            splashScreen.SetData("Welcome to Funball!",
                currentGameState);

            score = 0;
            totalHits = 0;
            totalMisses = 0;

            credits = new RollingCredits(this);
            credits.addText("Developed by:");
            credits.addText("Alex Salerno");
            credits.addText("Dan Carrigan");
            credits.addText("Kim Phan");
            credits.addText("Ray Luu");
            credits.addText("");
            credits.addText("Models by David Durston");
            credits.addText("Music by Tyler Salerno");
            credits.addText("Sounds from grsites.com");
            credits.addText("");
            credits.addText("Press escape to exit");

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Set cullmode to none
            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rs;

            //Load audio content
            audioEngine = new AudioEngine(@"Content\Audio\RacketBallSound.xgs");
            waveBank = new WaveBank(audioEngine, @"Content\Audio\Wave Bank.xwb");
            soundBank = new SoundBank(audioEngine, @"Content\Audio\Sound Bank.xsb");

            //Background music created by Tyler Salerno
            trackCue = soundBank.GetCue("Background Music");
            trackCue.Play();

            // Load fonts
            scoreFont = Content.Load<SpriteFont>(@"Fonts\ScoreFont");
            splashScreenFontLarge = Content.Load<SpriteFont>(@"Fonts\SplashScreenFontLarge");
        }

        protected override void UnloadContent()
        {
        }

        public void PlayCue(string cue) { soundBank.PlayCue(cue); }

        public void ChangeGameState(GameState state, int level)
        {
            currentGameState = state;

            switch (currentGameState)
            {
                case GameState.PLAY:
                    modelManager.Enabled = true;
                    modelManager.Visible = true;
                    splashScreen.Enabled = false;
                    splashScreen.Visible = false;
                    break;

                case GameState.END:
                    splashScreen.SetData("Game Over.\nLevel: " + (level + 1) +
                        "\nScore: " + score, GameState.END);
                    modelManager.Enabled = false;
                    modelManager.Visible = false;
                    splashScreen.Enabled = true;
                    splashScreen.Visible = true;
                    break;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            modelManager.getGameState(currentGameState.ToString());

            if (currentGameState == GameState.END)
            {
                trackCue.Stop(AudioStopOptions.Immediate);
                soundBank.PlayCue("End Music");

                try
                {
                    Components.Remove(splashScreen);
                }
                catch (Exception e) { }

                if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                    this.Exit();
                credits.Update(gameTime);
                return;
            }

            // Allows the game to exit
            if (currentGameState != GameState.GAMEOVER &&
                (score < 0 ||
                GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape)))
            {
                currentGameState = GameState.GAMEOVER;
            }

            // Don't let the mouse move if in between levels
            if (currentGameState == GameState.START)
                Mouse.SetPosition(Window.ClientBounds.Width >> 1,
                    Window.ClientBounds.Height >> 1);

            if (currentGameState == GameState.GAMEOVER)
            {
                millisecondDelay -= gameTime.ElapsedGameTime.Milliseconds;
                if (millisecondDelay < 0)
                    currentGameState = GameState.END;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.Clear(Color.LightSlateGray);

            base.Draw(gameTime);

            // Display game rules at start screen
            if(currentGameState == GameState.START)
            {
                spriteBatch.Begin();

                spriteBatch.DrawString(
                    scoreFont,
                    "Hit the ball with a racquet\nHit:\n  +2, faster, smaller ball\nMiss:\n  -1\nNew Ball:\n  -10\nGame Over:\n  Score < 0\n  After the 1st hit",
                    new Vector2(10, 10),
                    Color.Goldenrod);

                spriteBatch.End();
            }

            // Display help if game is not over
            if (currentGameState != GameState.END)
            {
                spriteBatch.Begin();

                if (Keyboard.GetState().IsKeyDown(Keys.F1))
                {
                    spriteBatch.DrawString(
                        scoreFont,
                        "Q - rotate racquet left\nW - jump\nE - rotate racquet right\nA - strafe left\nD - strafe right\nMouse - point camera and racquet\nSpace - release ball",
                        new Vector2(10, Window.ClientBounds.Height - 168),
                        Color.Goldenrod);
                }
                else
                    spriteBatch.DrawString(
                        scoreFont,
                        "Hold F1 for Help",
                        new Vector2(10, Window.ClientBounds.Height - 30),
                        Color.DarkKhaki);

                spriteBatch.End();
            }
            else
                credits.Draw(gameTime, spriteBatch);

            // Display game states during play and after ending
            if (currentGameState != GameState.START)
            {
                spriteBatch.Begin();

                // Draw the current score
                string scoreText = "Score: " + score;
                spriteBatch.DrawString(scoreFont,
                    scoreText,
                    new Vector2(10, 10),
                    Color.Goldenrod);

                // Draw the total number of hits
                scoreText = "Hits: " + totalHits;
                spriteBatch.DrawString(scoreFont,
                    scoreText,
                    new Vector2(10, scoreFont.MeasureString(scoreText).Y + 10),
                    Color.Goldenrod);

                // Draw the total number of misses
                scoreText = "Misses: " + totalMisses;
                spriteBatch.DrawString(scoreFont,
                    scoreText,
                    new Vector2(10, 2 * scoreFont.MeasureString(scoreText).Y + 10),
                    Color.Goldenrod);

                spriteBatch.End();
            }

            if(currentGameState == GameState.GAMEOVER)
            {
                spriteBatch.Begin();

                // Draw the current score
                string scoreText = "Score: " + score;
                spriteBatch.DrawString(scoreFont,
                    scoreText,
                    new Vector2(10, 10),
                    Color.Red);
                scoreText = "Game Over";
                spriteBatch.DrawString(splashScreenFontLarge,
                    scoreText,
                    new Vector2(
                        (int)(Window.ClientBounds.Width - splashScreenFontLarge.MeasureString(scoreText).X) >> 1,
                        40),
                    Color.Red);

                spriteBatch.End();
            }
        }
    }
}
