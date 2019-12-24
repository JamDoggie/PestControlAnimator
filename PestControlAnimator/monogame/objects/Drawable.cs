using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PestControlAnimator.monogame.content;
using PestControlAnimator.monogame.graphicspipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PestControlAnimator.monogame.objects
{
    public class Drawable
    {
        private Dictionary<string, Spritebox> SpriteBoxes = new Dictionary<string, Spritebox>();

        private Vector2 _Position = new Vector2();

        private string _name = "";

        private List<Drawable> _children = new List<Drawable>();

        private Drawable _parent = null;

        /// <summary>
        /// Overridable, allows us to control how to draw this object directly.
        /// Not recommended if you can achieve your goal with more higher level methods.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="spriteBatch"></param>
        public virtual void Draw(GraphicsDevice device, SpriteBatch spriteBatch)
        {
            foreach (KeyValuePair<string, Spritebox> pair in SpriteBoxes)
            {
                Spritebox spriteBox = pair.Value;

                if (spriteBatch != null && ContentManager.GetTexture(spriteBox.GetTextureKey()) != null)
                {
                    spriteBatch.Draw(ContentManager.GetTexture(spriteBox.GetTextureKey()), new Rectangle((int)(spriteBox.GetPosition().X + GetPosition().X), (int)(spriteBox.GetPosition().Y + GetPosition().Y), spriteBox.GetWidth(), spriteBox.GetHeight()), spriteBox.GetSourceRectangle(), Color.White);
                }
                else
                {
                    throw new ArgumentNullException(nameof(spriteBatch));
                }
            }
            
            // Draw children
            foreach(Drawable d in _children)
            {
                d.Draw(device, spriteBatch);
            }
        }

        public virtual void Update(GameTime gameTime)
        {
            // Update children
            foreach (Drawable d in _children)
            {
                d.Update(gameTime);
            }
        }

        public virtual Vector2 GetPosition()
        {
            if (_parent != null)
            {
                return new Vector2(_Position.X + GetParent().GetPosition().X, _Position.Y + GetParent().GetPosition().Y);
            }
            else
            {
                return _Position;
            }
            
        }

        public virtual Vector2 GetRawPosition()
        {
            return _Position;
        }

        public virtual void SetPosition(Vector2 position)
        {
            _Position = position;
        }

        public virtual void SetSpriteBoxes(Dictionary<string, Spritebox> _spriteBoxes)
        {
            SpriteBoxes = _spriteBoxes;
        }

        public virtual Dictionary<string, Spritebox> GetSpriteBoxes()
        {
            return SpriteBoxes;
        }

        public virtual bool RemoveSpriteBox(Spritebox spriteBox)
        {
            foreach(KeyValuePair<string, Spritebox> pair in SpriteBoxes)
            {
                if (pair.Value == spriteBox)
                {
                    SpriteBoxes.Remove(pair.Key);

                    return true;
                }
            }

            return false;
        }

        public virtual void AddChild(Drawable drawable)
        {
            if (drawable == null)
                return;

            drawable._parent = this;
            _children.Add(drawable);
        }

        public virtual void RemoveChild(Drawable drawable)
        {
            if (drawable == null)
                return;

            if (drawable._parent == this)
            {
                _children.Remove(drawable);
                drawable._parent = null;
            }
            
        }

        public virtual List<Drawable> GetChildren()
        {
            return _children;
        }

        /// <summary>
        /// Sets the list of children in this drawable. NOTE: if you wish to add or remove children, it's recommended
        /// to use RemoveChild and AddChild.
        /// </summary>
        /// <param name="list"></param>
        public virtual void SetChildren(List<Drawable> list)
        {
            _children = list;
        }

        public virtual Drawable GetParent()
        {
            return _parent;
        }

        public virtual void SetParent(Drawable parent)
        {
            _parent = parent;
        }

        public virtual void MouseMove(object sender, MouseEventArgs e, Vector2 worldMousePosition)
        {
            foreach (Drawable drawable in _children)
            {
                drawable.MouseMove(sender, e, worldMousePosition);
            }
        }

        public virtual void MouseDown(object sender, MouseEventArgs e, Vector2 worldMousePosition)
        {
            foreach(Drawable drawable in _children)
            {
                drawable.MouseDown(sender, e, worldMousePosition);
            }
        }

        public virtual void MouseUp(object sender, MouseEventArgs e, Vector2 worldMousePosition)
        {
            foreach (Drawable drawable in _children)
            {
                drawable.MouseUp(sender, e, worldMousePosition);
            }
        }
    }
}
