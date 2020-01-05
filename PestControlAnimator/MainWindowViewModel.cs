using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PestControlAnimator.monogame.graphicspipeline;
using PestControlAnimator.monogame.objects;
using PestControlAnimator.MonoGameControls;
using PestControlAnimator.shared;
using PestControlAnimator.shared.animation;
using PestControlAnimator.wpf.controls;

namespace PestControlAnimator
{
    public class MainWindowViewModel : MonoGameViewModel
    {
        private SpriteBatch _spriteBatch;
        private SpriteBatch _guiBatch;

        private SpriteRenderer SpriteManager;

        public Camera MainCamera;

        public static MainWindowViewModel MonogameWindow = null;

        // Currently previewed spriteboxes
        SpritePreviewObject previewObject = new SpritePreviewObject();

        SelectionBox selectionBox = new SelectionBox(null);

        int spriteboxesCreated = 0;
        private string sprBoxSelected = "enginereserved_null";

        private float _mouseX = 0;
        private float _mouseY = 0;

        Vector2 worldMousePosition = new Vector2();
        Vector2 oldWorldMousePosition = new Vector2();

        private bool _mouseDown = false;
        private bool _mouseDownMiddle = false;

        private bool sidebarVisible = false;
        private float sidebarCoords = -32;

        Texture2D gridTexture;
        public SpriteFont DefaultFont { get; set; }

        public override void Initialize()
        {
            SpriteManager = new SpriteRenderer();
            MainCamera = new Camera(GraphicsDevice.Viewport);
            MonogameWindow = this;

            // Adding objects to sprite manager
            SpriteManager.GetSprites().Add(previewObject);
            SpriteManager.GetSprites().Add(selectionBox);

            base.Initialize();

            ProjectManager.LoadProjectContent();

            // Grid object
            monogame.objects.Grid Grid = new monogame.objects.Grid();
            SpriteManager.GetSprites().Add(Grid);

            gridTexture = Content.Load<Texture2D>("grid");


            TimeLine.timeLine.DisplayAtScrubber();
        }

        public override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _guiBatch = new SpriteBatch(GraphicsDevice);

            DefaultFont = Content.Load<SpriteFont>("DefaultFont");


        }

        public override void UnloadContent()
        {
            base.UnloadContent();
        }

        public GraphicsDevice GetGraphicsDevice()
        {
            return GraphicsDevice;
        }

        public override void Update(GameTime gameTime)
        {
            if (gameTime == null)
            {
                return;
            }

            SpriteManager.Update(gameTime);

            // Timeline is updated here because monogame has a nice update loop and delta time system just sitting here waiting for us.
            if (TimeLineInfo.isPlaying)
            {
                TimeLineInfo.timelineMs += gameTime.ElapsedGameTime.TotalMilliseconds;

                if (TimeLineInfo.timelineMs > TimeLine.timeLine.TimeLineEnd * 16)
                {
                    TimeLineInfo.timelineMs = TimeLine.timeLine.TimeLineEnd * 16;

                    if (MainWindow.project.GetProjectInfo().Loop)
                    {
                        TimeLineInfo.timelineMs = MainWindow.project.GetProjectInfo().LoopToFrame;
                    }
                }

                Canvas.SetLeft(MainWindow.mainWindow.MainTimeline.Scrubber, (TimeLineInfo.timelineMs/16) * MainWindow.mainWindow.MainTimeline.ScreenScale);
                if (TimeLine.timeLine != null)
                {
                    TimeLine.timeLine.DisplayAtScrubber();
                }
            }

            // Make scrubber's height correct
            MainWindow.mainWindow.MainTimeline.Scrubber.Height = MainWindow.mainWindow.MainTimeline.TimeLineCanvas.ActualHeight;

            foreach(UIElement c in MainWindow.mainWindow.MainTimeline.TimeLineCanvas.Children)
            {
                Keyframe keyframe = c as Keyframe;

                // If keyframe is null, that means the object is not a type of keyframe.
                if (keyframe != null)
                {
                    Canvas.SetLeft(c, (keyframe.PositionX * MainWindow.mainWindow.MainTimeline.ScreenScale) - (keyframe.ActualWidth/2) - (MainWindow.mainWindow.MainTimeline.GetRealScreenOffset()));
                }
            }

            // Resize timeline so scrolling works
            int greatestX = 0;
            foreach(FrameworkElement element in TimeLine.timeLine.TimeLineCanvas.Children)
            {
                if (Canvas.GetLeft(element) + TimeLine.timeLine.TimeLineEndPadding > greatestX)
                {
                    
                    greatestX = (int)Canvas.GetLeft(element) + TimeLine.timeLine.TimeLineEndPadding;
                }
            }
            TimeLine.timeLine.TimeLineCanvas.Width = greatestX;
            TimeLine.timeLine.TimeLineCanvas.Margin = new Thickness(0, TimeLine.timeLine.TimeLineCanvas.Margin.Top, TimeLine.timeLine.TimeLineCanvas.Margin.Right, TimeLine.timeLine.TimeLineCanvas.Margin.Bottom);
            TimeLine.timeLine.TimeLineCanvas.HorizontalAlignment = HorizontalAlignment.Left;

            // World mouse position, used for transforming wpf mouse coords into coordinates relative to the world and camera.
            worldMousePosition = Vector2.Transform(new Vector2(_mouseX, _mouseY), Matrix.Invert(MainCamera.Transform));

            if (sidebarVisible && sidebarCoords < 0)
            {
                sidebarCoords = Lerp(sidebarCoords, 1f, 0.1f);
                if (sidebarCoords > 0)
                {
                    sidebarCoords = 0;
                }
            }
            if (!sidebarVisible)
            {
                sidebarCoords = Lerp(sidebarCoords, -33f, 0.1f);
                if (sidebarCoords < -32)
                {
                    sidebarCoords = -32;
                }
            }

            // Selection
            if (sprBoxSelected != "enginereserved_null")
            {
                foreach (KeyValuePair<string, Spritebox> pair in previewObject.GetSpriteBoxes())
                {
                    if (pair.Key == sprBoxSelected)
                    {
                        selectionBox.SetBoundObject(pair.Value);
                    }
                }
            }
            else
            {
                selectionBox.SetBoundObject(null);
            }
            MainCamera.UpdateCamera(GraphicsDevice.Viewport);
        }

       

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(16, 17, 25));

            if (SpriteManager != null)
            {
                _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, RasterizerState.CullCounterClockwise, null, MainCamera.Transform);

                SpriteManager.Draw(GraphicsDevice, _spriteBatch);
                //SpriteRenderer.DrawRectangle(_spriteBatch, GraphicsDevice, new Rectangle((int)worldMousePosition.X, (int)worldMousePosition.Y, 20, 20), Color.White);

                _spriteBatch.End();


                _guiBatch.Begin();

                _guiBatch.Draw(gridTexture, new Rectangle((int)sidebarCoords, 50, 32, 32), Color.White);

                _guiBatch.End();
            }
        }

        float oldMidX = 0;
        float oldMidY = 0;


        Vector2 oldMidPositionWorld = new Vector2();

        Vector2 grabPosition = new Vector2();

        // WPF events for input.
        public void MouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseDown)
            {
                _mouseX = (float)e.GetPosition(MainWindow.mainWindow.MainView).X;
                _mouseY = (float)e.GetPosition(MainWindow.mainWindow.MainView).Y;

                worldMousePosition = Vector2.Transform(new Vector2(_mouseX, _mouseY), Matrix.Invert(MainCamera.Transform));

                // Moving selected object
                if (sprBoxSelected != "enginereserved_null")
                {
                    Spritebox sprBox;
                    previewObject.GetSpriteBoxes().TryGetValue(sprBoxSelected, out sprBox);

                    if (sprBox != null && new Rectangle(sprBox.GetRectangle().X + (int)previewObject.GetPosition().X, sprBox.GetRectangle().Y + (int)previewObject.GetPosition().Y, sprBox.GetWidth(), sprBox.GetHeight()).Intersects(new Rectangle((int)worldMousePosition.X, (int)worldMousePosition.Y, 1, 1)))
                    {
                        bool resizing = false;
                        foreach(Drawable drawable in selectionBox.GetChildren())
                        {
                            ResizeBox resizeBox = (ResizeBox)drawable;

                            if (resizeBox.GetSelected())
                            {
                                resizing = true;
                            }
                        }
                        if (!resizing)
                            sprBox.SetPosition(worldMousePosition + grabPosition);
                    }
                }

                oldWorldMousePosition = worldMousePosition;
            }
            
            if (_mouseDownMiddle)
            {
                Vector2 newMidPosition = Vector2.Transform(new Vector2((float)e.GetPosition(MainWindow.mainWindow.MainView).X, (float)e.GetPosition(MainWindow.mainWindow.MainView).Y), Matrix.Invert(MainCamera.Transform));

                MainCamera.MoveCamera(new Vector2(-((newMidPosition.X - oldMidPositionWorld.X)/1.5f), -((newMidPosition.Y - oldMidPositionWorld.Y)/1.5f)));

                oldMidX = (float)e.GetPosition(MainWindow.mainWindow.MainView).X;
                oldMidY = (float)e.GetPosition(MainWindow.mainWindow.MainView).Y;
                oldMidPositionWorld = newMidPosition;
            }

            foreach(Drawable drawable in SpriteManager.GetSprites())
            {
                drawable.MouseMove(sender, e, worldMousePosition);
            }
        }

        public void MouseDown(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _mouseDown = true;

                _mouseX = (float)e.GetPosition(MainWindow.mainWindow.MainView).X;
                _mouseY = (float)e.GetPosition(MainWindow.mainWindow.MainView).Y;

                worldMousePosition = Vector2.Transform(new Vector2(_mouseX, _mouseY), Matrix.Invert(MainCamera.Transform));

                // Object selection
                sprBoxSelected = "enginereserved_null";

                foreach(KeyValuePair<string, Spritebox> pair in previewObject.GetSpriteBoxes())
                {
                    Rectangle selectionRect = new Rectangle(pair.Value.GetRectangle().X + (int)previewObject.GetPosition().X, pair.Value.GetRectangle().Y + (int)previewObject.GetPosition().Y, pair.Value.GetWidth(), pair.Value.GetHeight());

                    if (selectionRect.Intersects(new Rectangle((int)worldMousePosition.X, (int)worldMousePosition.Y, 1, 1)) && pair.Key != sprBoxSelected)
                    {
                        sprBoxSelected = pair.Key;
                    }
                }

                // Object moving initialization
                Spritebox sprBox;
                previewObject.GetSpriteBoxes().TryGetValue(sprBoxSelected, out sprBox);

                if (sprBox != null)
                {
                    Rectangle selectedRect = new Rectangle(sprBox.GetRectangle().X + (int)previewObject.GetPosition().X, sprBox.GetRectangle().Y + (int)previewObject.GetPosition().Y, sprBox.GetWidth(), sprBox.GetHeight());
                    Rectangle mouseRect = new Rectangle((int)worldMousePosition.X, (int)worldMousePosition.Y, 1, 1);

                    // Get the position the object was at the time of the grab relative to the mouse
                    if (selectedRect.Intersects(mouseRect))
                    {
                        grabPosition = new Vector2(selectedRect.X - mouseRect.X, selectedRect.Y - mouseRect.Y);
                    }
                }
                

                oldWorldMousePosition = worldMousePosition;

                foreach (Drawable drawable in SpriteManager.GetSprites())
                {
                    drawable.MouseDown(sender, e, worldMousePosition);
                }
            }
                

            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                _mouseDownMiddle = true;
                oldMidX = (float)e.GetPosition(MainWindow.mainWindow.MainView).X;
                oldMidY = (float)e.GetPosition(MainWindow.mainWindow.MainView).Y;
                oldMidPositionWorld = Vector2.Transform(new Vector2(oldMidX, oldMidY), Matrix.Invert(MainCamera.Transform));
            }
                
        }

        public void MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                MainCamera.Zoom += 0.2f;
            }
            if (e.Delta < 0)
            {
                MainCamera.Zoom -= 0.2f;
            }
        }
        public void MouseUp(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
                _mouseDown = false;

            if (e.MiddleButton == MouseButtonState.Released)
                _mouseDownMiddle = false;

            foreach (Drawable drawable in SpriteManager.GetSprites())
            {
                drawable.MouseUp(sender, e, worldMousePosition);
            }
        }

        public void MouseEnter(object sender, MouseEventArgs e)
        {
            sidebarVisible = true;
        }

        public void MouseLeave(object sender, MouseEventArgs e)
        {
            _mouseDown = false;
            _mouseDownMiddle = false;
            sidebarVisible = false;
        }

        public void AddPreviewSpriteBox(Spritebox spriteBox)
        {
            previewObject.GetSpriteBoxes().Add($"{spriteboxesCreated}", spriteBox);
            spriteboxesCreated++;
        }

        public void RemovePreviewSpriteBox(Spritebox spriteBox)
        {
            previewObject.RemoveSpriteBox(spriteBox);
        }

        float Lerp(float firstFloat, float secondFloat, float by)
        {
            return firstFloat * (1 - by) + secondFloat * by;
        }

        public SpritePreviewObject GetPreviewObject()
        {
            return previewObject;
        }

        public string getSprBoxSelected()
        {
            return sprBoxSelected;
        }

        public void setSprBoxSelected(string str)
        {
            sprBoxSelected = str;
        }
    }
}