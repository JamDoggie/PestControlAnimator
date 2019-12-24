using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PestControlAnimator.monogame.graphicspipeline;

namespace PestControlAnimator.monogame.objects
{
    public class SelectionBox : Drawable
    {
        private Spritebox _boundObject;

        public SelectionBox(Spritebox boundOject)
        {
            _boundObject = boundOject;

            // Resize boxes for resizing sprite
            ResizeBox box1 = new ResizeBox(ResizeBoxEnumVertical.TOP, ResizeBoxEnumHorizontal.LEFT, this);
            ResizeBox box2 = new ResizeBox(ResizeBoxEnumVertical.BOTTOM, ResizeBoxEnumHorizontal.LEFT, this);
            ResizeBox box3 = new ResizeBox(ResizeBoxEnumVertical.TOP, ResizeBoxEnumHorizontal.RIGHT, this);
            ResizeBox box4 = new ResizeBox(ResizeBoxEnumVertical.BOTTOM, ResizeBoxEnumHorizontal.RIGHT, this);

            // Add the resize boxes as children
            AddChild(box1);
            AddChild(box2);
            AddChild(box3);
            AddChild(box4);
        }

        public override void Draw(GraphicsDevice device, SpriteBatch spriteBatch)
        {
            if (_boundObject == null || spriteBatch == null)
                return;

            SpriteRenderer.DrawRectangle(spriteBatch, device, new Rectangle((int)GetPosition().X, (int)GetPosition().Y, _boundObject.GetWidth(), _boundObject.GetHeight()), Color.White);
            spriteBatch.DrawString(MainWindowViewModel.MonogameWindow.DefaultFont, $"{_boundObject.GetRectangle().Width}, {_boundObject.GetRectangle().Height}", new Vector2((int)_boundObject.GetPosition().X, (int)_boundObject.GetPosition().Y - 15), Color.White);

            base.Draw(device, spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            if (_boundObject == null)
                return;

            SetPosition(_boundObject.GetPosition());

            base.Update(gameTime);
        }

        public Spritebox GetBoundObject()
        {
            return _boundObject;
        }

        public void SetBoundObject(Spritebox spritebox)
        {
            _boundObject = spritebox;
        }
    }

    public class ResizeBox : Drawable
    {
        private ResizeBoxEnumVertical VerticalPos = ResizeBoxEnumVertical.TOP;
        private ResizeBoxEnumHorizontal HorizontalPos = ResizeBoxEnumHorizontal.LEFT;
        private SelectionBox _selectionBox = null;

        private bool _mouseDown = false;
        private bool _selected = false;

        private Vector2 _distanceOnSelect = new Vector2();

        public ResizeBox(ResizeBoxEnumVertical vertical, ResizeBoxEnumHorizontal horizontal, SelectionBox selectionBox)
        {
            VerticalPos = vertical;
            HorizontalPos = horizontal;
            _selectionBox = selectionBox;
        }

        public override void Draw(GraphicsDevice device, SpriteBatch spriteBatch)
        {
            SpriteRenderer.DrawRectangle(spriteBatch, device, GetHoverBox(), Color.White);
            
        }

        public Rectangle GetHoverBox()
        {
            return new Rectangle((int)GetPosition().X - 4, (int)GetPosition().Y - 4, 8, 8);
        }

        public override void Update(GameTime gameTime)
        {
            Drawable parent = GetParent();

            if (parent == null)
                return;

            if (_selectionBox == null)
                return;


            if (VerticalPos == ResizeBoxEnumVertical.BOTTOM)
            {
                SetPosition(new Vector2(GetRawPosition().X, _selectionBox.GetBoundObject().GetHeight()));
            }
            if (HorizontalPos == ResizeBoxEnumHorizontal.RIGHT)
            {
                SetPosition(new Vector2(_selectionBox.GetBoundObject().GetRectangle().Width, GetRawPosition().Y));
            }
            base.Update(gameTime);
        }

        Vector2 oldMousePos = new Vector2();

        public override void MouseMove(object sender, MouseEventArgs e, Vector2 worldMousePosition)
        {
            if (e == null)
                return;

            if (_selectionBox.GetBoundObject() == null)
                return;

            // Resizing object
            if (_selected)
            {
                // Vertical-Top
                if (VerticalPos == ResizeBoxEnumVertical.TOP)
                {
                    Vector2 NewPos = new Vector2(_selectionBox.GetBoundObject().GetPosition().X, worldMousePosition.Y + _distanceOnSelect.Y + 4);

                    Vector2 SprOldPos = _selectionBox.GetBoundObject().GetPosition();

                    Spritebox sprBox = _selectionBox.GetBoundObject();

                    sprBox.SetPosition(NewPos);

                    int posDifference = ((int)sprBox.GetPosition().Y - (int)SprOldPos.Y);

                    sprBox.SetHeight(sprBox.GetHeight() - posDifference);
                    sprBox.SetSourceRectangle(new Rectangle(sprBox.GetSourceRectangle().X, sprBox.GetSourceRectangle().Y + posDifference, sprBox.GetSourceRectangle().Width, sprBox.GetHeight()));
                }

                // Vertical-Bottom
                if (VerticalPos == ResizeBoxEnumVertical.BOTTOM)
                {
                    Vector2 SprOldPos = _selectionBox.GetBoundObject().GetPosition();

                    Spritebox sprBox = _selectionBox.GetBoundObject();

                    int posDifference = ((int)(worldMousePosition.Y) - (int)oldMousePos.Y);

                    Rectangle newRect = sprBox.GetRectangle();
                    

                    sprBox.SetHeight((int)worldMousePosition.Y - (int)sprBox.GetPosition().Y + (int)_distanceOnSelect.Y + 4);

                    sprBox.SetSourceRectangle(new Rectangle(sprBox.GetSourceRectangle().X, sprBox.GetSourceRectangle().Y, sprBox.GetSourceRectangle().Width, sprBox.GetHeight()));
                }

                // Horizontal-Left
                if (HorizontalPos == ResizeBoxEnumHorizontal.LEFT)
                {
                    Vector2 NewPos = new Vector2(worldMousePosition.X + _distanceOnSelect.X + 4, _selectionBox.GetBoundObject().GetPosition().Y);

                    Vector2 SprOldPos = _selectionBox.GetBoundObject().GetPosition();

                    Spritebox sprBox = _selectionBox.GetBoundObject();

                    sprBox.SetPosition(NewPos);

                    int posDifference = ((int)sprBox.GetPosition().X - (int)SprOldPos.X);

                    sprBox.SetWidth(sprBox.GetWidth() - posDifference);
                    sprBox.SetSourceRectangle(new Rectangle(sprBox.GetSourceRectangle().X + posDifference, sprBox.GetSourceRectangle().Y, sprBox.GetWidth(), sprBox.GetSourceRectangle().Height ));
                }

                // Horizontal-Right
                if (HorizontalPos == ResizeBoxEnumHorizontal.RIGHT)
                {
                    Vector2 SprOldPos = _selectionBox.GetBoundObject().GetPosition();

                    Spritebox sprBox = _selectionBox.GetBoundObject();

                    int posDifference = ((int)(worldMousePosition.X) - (int)oldMousePos.X);

                    Rectangle newRect = sprBox.GetRectangle();

                    sprBox.SetWidth((int)worldMousePosition.X - (int)sprBox.GetPosition().X + (int)_distanceOnSelect.X + 4);

                    sprBox.SetSourceRectangle(new Rectangle(sprBox.GetSourceRectangle().X, sprBox.GetSourceRectangle().Y, sprBox.GetWidth(), sprBox.GetSourceRectangle().Height));
                }
            }

            oldMousePos = worldMousePosition;

            base.MouseMove(sender, e, worldMousePosition);
        }

        public override void MouseDown(object sender, MouseEventArgs e, Vector2 worldMousePosition)
        {
            if (GetHoverBox().Intersects(new Rectangle((int)worldMousePosition.X, (int)worldMousePosition.Y, 1, 1)))
            {
                foreach(KeyValuePair<string, Spritebox> pair in MainWindowViewModel.MonogameWindow.GetPreviewObject().GetSpriteBoxes())
                {
                    if (pair.Value.Equals(_selectionBox.GetBoundObject()))
                    {
                        MainWindowViewModel.MonogameWindow.setSprBoxSelected(pair.Key);
                    }
                }

                _selected = true;
                _distanceOnSelect = new Vector2(GetHoverBox().X - worldMousePosition.X, GetHoverBox().Y - worldMousePosition.Y);
            }

            _mouseDown = true;

            base.MouseDown(sender, e, worldMousePosition);
        }

        public override void MouseUp(object sender, MouseEventArgs e, Vector2 worldMousePosition)
        {
            _mouseDown = false;
            _selected = false;

            base.MouseUp(sender, e, worldMousePosition);
        }

        public bool GetSelected()
        {
            return _selected;
        }
    }


    public enum ResizeBoxEnumVertical
    {
        TOP,
        BOTTOM
    }

    public enum ResizeBoxEnumHorizontal
    {
        LEFT,
        RIGHT
    }
}
