﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PestControlAnimator.monogame.content;
using PestControlAnimator.monogame.objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PestControlAnimator.monogame.graphicspipeline
{
    public class SpriteRenderer
    {
        private List<Drawable> sprites = new List<Drawable>();

        public void Draw(GraphicsDevice device, SpriteBatch spriteBatch)
        {
            if (spriteBatch != null)
            {

                foreach (Drawable drawable in sprites)
                {

                    drawable.Draw(device, spriteBatch);

                }

            }
            else
            {
                throw new ArgumentNullException(nameof(spriteBatch));
            }
        }

        public List<Drawable> GetSprites()
        {
            return sprites;
        }

        public void Update(GameTime gameTime)
        {
            foreach (Drawable drawable in sprites)
            {
                drawable.Update(gameTime);
            }
        }

        // Returns 1x1 texture that is RGB 255,255,255
        public static Texture2D GetWhitePixel(GraphicsDevice graphicsDevice)
        {
            if (ContentManager.GetTexture("engine_onepx") != null)
            {
                return ContentManager.GetTexture("engine_onepx");
            }

            Texture2D texture = new Texture2D(graphicsDevice, 1, 1);
            Color[] colors = new Color[1];
            colors[0] = new Color(255, 255, 255);
            texture.SetData(colors);

            // Load texture into memory so we don't have to do the expensive operation of generating it every time.
            ContentManager.LoadTexture("engine_onepx", texture);

            return texture;
        }

        public static void DrawRectangle(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Rectangle rectangle, Color color)
        {
            Texture2D texture = GetWhitePixel(graphicsDevice);

            if (spriteBatch == null)
            {
                return;
            }

            // Top part of rectangle
            spriteBatch.Draw(texture, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, 1), new Rectangle(0, 0, 1, 1), color);
            // Left side of rectangle
            spriteBatch.Draw(texture, new Rectangle(rectangle.X, rectangle.Y + 1, 1, rectangle.Height - 1), new Rectangle(0, 0, 1, 1), color);
            // Right side of rectangle
            spriteBatch.Draw(texture, new Rectangle(rectangle.X + rectangle.Width - 1, rectangle.Y + 1, 1, rectangle.Height - 1), new Rectangle(0, 0, 1, 1), color);
            // Bottom part of rectangle
            spriteBatch.Draw(texture, new Rectangle(rectangle.X, rectangle.Y + rectangle.Height - 1, rectangle.Width, 1), new Rectangle(0, 0, 1, 1), color);
        }
    }
}
