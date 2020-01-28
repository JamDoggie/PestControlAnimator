/*
 * This class is used to simplify and automate the drawing process. Spriteboxes will mainly be handled internally and controlled by animation files.
 */

using Microsoft.Xna.Framework;
using PestControlAnimator.shared.animations.json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PestControlAnimator.monogame.objects
{
    public class Spritebox
    {
        // Note: object based, not world based. Essentially more of an offset.
        
        private Vector2 _position = new Vector2(0, 0);

        private Rectangle _sourceRectangle = new Rectangle();
        private int _width = 16;
        private int _height = 16;
        private float _rotation = 0.0f;
        private string _textureKey = "";
        private Drawable _parent = null;
        private float _layer = 0;
        private bool _isVisible = true;

        public Spritebox(Vector2 position, int width, int height, float rotation, string textureKey, float layer, Rectangle sourceRectangle, Drawable parent)
        {
            _position = position;
            _width = width;
            _height = height;
            _rotation = rotation;
            _textureKey = textureKey;
            _sourceRectangle = sourceRectangle;
            _parent = parent;
            _layer = layer;
        }

        public Vector2 GetPosition()
        {
            return _position;
        }

        public int GetWidth()
        {
            return _width;
        }

        public int GetHeight()
        {
            return _height;
        }

        public void SetWidth(int width)
        {
            _width = width;
        }

        public void SetHeight(int height)
        {
            _height = height;
        }

        public float GetRotation()
        {
            return _rotation;
        }

        public string GetTextureKey()
        {
            return _textureKey;
        }

        public Rectangle GetRectangle()
        {
            return new Rectangle((int)_position.X, (int)_position.Y, _width, _height);
        }

        public float GetLayer()
        {
            return _layer;
        }

        public void SetRectangle(Rectangle rectangle)
        {
            _position.X = rectangle.X;
            _position.Y = rectangle.Y;
            _width = rectangle.Width;
            _height = rectangle.Height;
        }

        public Rectangle GetSourceRectangle()
        {
            return _sourceRectangle;
        }

        public void SetSourceRectangle(Rectangle source)
        {
            _sourceRectangle = source;
        }

        public Drawable GetParent()
        {
            return _parent;
        }

        public void SetPosition(Vector2 position)
        {
            _position = position;
        }

        public void SetVisible(bool visible)
        {
            _isVisible = visible;
        }

        public bool Visible()
        {
            return _isVisible;
        }

        public static SpriteboxJson ToJsonElement(Spritebox spriteBox)
        {
            if (spriteBox == null)
                return null;

            SpriteboxJson sprBoxJson = new SpriteboxJson()
            {
                posX = spriteBox.GetPosition().X,
                posY = spriteBox.GetPosition().Y,
                width = spriteBox.GetWidth(),
                height = spriteBox.GetHeight(),
                rotation = spriteBox._rotation,
                sourceHeight = spriteBox.GetSourceRectangle().Height,
                sourceWidth = spriteBox.GetSourceRectangle().Width,
                sourceX = spriteBox.GetSourceRectangle().X,
                sourceY = spriteBox.GetSourceRectangle().Y,
                textureKey = spriteBox.GetTextureKey(),
                layer = spriteBox.GetLayer(),
                visible = spriteBox.Visible()
            };

            return sprBoxJson;
        }

        public static Spritebox FromJsonElement(SpriteboxJson spriteBoxJson)
        {
            if (spriteBoxJson == null)
                return null;

            Spritebox sprBox = new Spritebox(new Vector2((float)spriteBoxJson.posX, (float)spriteBoxJson.posY), spriteBoxJson.width, spriteBoxJson.height, spriteBoxJson.rotation, 
                spriteBoxJson.textureKey, spriteBoxJson.layer, new Rectangle(spriteBoxJson.sourceX, spriteBoxJson.sourceY, spriteBoxJson.sourceWidth, spriteBoxJson.sourceHeight), 
                null);
            sprBox.SetVisible(spriteBoxJson.visible);

            return sprBox;
        }
    }
}
