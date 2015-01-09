using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Funball
{
    class RollingCredits
    {
        private
            SpriteFont font;
            List<string> nameList;
            Game game;

            int fontHeight;

        public RollingCredits(Game game) 
        {
            font = game.Content.Load<SpriteFont>(@"Fonts\ScoreFont");
            nameList = new List<string>();
            this.game = game;
            fontHeight = game.Window.ClientBounds.Height + 50;
        }

        public void addText(string s) { nameList.Add(s); }

        public void Update(GameTime gameTime)
        {
            fontHeight--;
            if ((fontHeight - (-35 * nameList.Count)) < 0)
            {
                fontHeight = game.Window.ClientBounds.Height;
            }
        }

        public void Draw(GameTime gameTime,SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            int fontWidthDifference = 0;

            foreach(string s in nameList)
            {
                spriteBatch.DrawString(
                    font,
                    s, 
                    new Vector2(
                        game.Window.ClientBounds.Width/2,
                        fontHeight - fontWidthDifference),
                    Color.Gold,
                    0.0f,
                    font.MeasureString(s) / 2,
                    1.5f,
                    SpriteEffects.None,
                    0f);

                fontWidthDifference -= 35;
            }

            spriteBatch.End();
        }
    }
}
