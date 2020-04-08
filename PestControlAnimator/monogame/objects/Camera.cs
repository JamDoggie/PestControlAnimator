using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PestControlAnimator.monogame.objects
{
    public class Camera
    {
        public float Zoom;
        public Vector2 Position { get; set; }
        public Vector2 OriginalPosition { get; set; }
        public Rectangle Bounds { get; protected set; }
        public Rectangle VisibleArea { get; protected set; }
        public Matrix Transform { get; set; }

        public float CurrentZoomLerp { get; set; } = 1f;

        private float zoom, previousZoom;

        public Camera(Viewport viewport)
        {
            Bounds = viewport.Bounds;
            Zoom = 1f;
        }


        private void UpdateVisibleArea()
        {
            var inverseViewMatrix = Matrix.Invert(Transform);

            var tl = Vector2.Transform(Vector2.Zero, inverseViewMatrix);
            var tr = Vector2.Transform(new Vector2(Bounds.X, 0), inverseViewMatrix);
            var bl = Vector2.Transform(new Vector2(0, Bounds.Y), inverseViewMatrix);
            var br = Vector2.Transform(new Vector2(Bounds.Width, Bounds.Height), inverseViewMatrix);

            var min = new Vector2(
                MathHelper.Min(tl.X, MathHelper.Min(tr.X, MathHelper.Min(bl.X, br.X))),
                MathHelper.Min(tl.Y, MathHelper.Min(tr.Y, MathHelper.Min(bl.Y, br.Y))));
            var max = new Vector2(
                MathHelper.Max(tl.X, MathHelper.Max(tr.X, MathHelper.Max(bl.X, br.X))),
                MathHelper.Max(tl.Y, MathHelper.Max(tr.Y, MathHelper.Max(bl.Y, br.Y))));
            VisibleArea = new Rectangle((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y));
        }

        private void UpdateMatrix()
        {
            Transform = Matrix.CreateTranslation(new Vector3(Position.X, Position.Y, 0)) *
                    Matrix.CreateScale(CurrentZoomLerp);

            UpdateVisibleArea();
        }

        public void MoveCamera(Vector2 movePosition)
        {
            Vector2 newPosition = Position + movePosition;
            Position = newPosition;
        }

        public void AdjustZoom(float zoomAmount)
        {
            Zoom += zoomAmount;
            if (Zoom < .35f)
            {
                Zoom = .35f;
            }
            if (Zoom > 10f)
            {
                Zoom = 10f;
            }
        }

        public void UpdateCamera(Viewport bounds, GameTime gameTime)
        {
            Bounds = bounds.Bounds;

            UpdateMatrix();

            CurrentZoomLerp = MathHelper.Lerp(CurrentZoomLerp, Zoom, 0.1f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 100));

            previousZoom = zoom;
            zoom = Zoom;
        }
    }
}
