﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PestControlAnimator.monogame.graphicspipeline;
using PestControlAnimator.monogame.objects;
using PestControlAnimator.MonoGameControls;
using PestControlAnimator.shared;
using PestControlAnimator.shared.animations;
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

        private bool _shouldUpdateProperties = false;

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
                    float msDifference = ((float)TimeLineInfo.timelineMs) - ((float)TimeLine.timeLine.TimeLineEnd * 16);
                    TimeLineInfo.timelineMs = TimeLine.timeLine.TimeLineEnd * 16;

                    if (MainWindow.project.GetProjectInfo().Loop)
                    {
                        TimeLineInfo.timelineMs = MainWindow.project.GetProjectInfo().LoopToFrame * 16 + msDifference;
                    }
                    else
                    {
                        TimeLineInfo.isPlaying = false;
                    }
                }

                Canvas.SetLeft(MainWindow.mainWindow.MainTimeline.Scrubber, (TimeLineInfo.timelineMs/ 16) * MainWindow.mainWindow.MainTimeline.ScreenScale);
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
                        if (_shouldUpdateProperties)
                        {
                            MainWindow.mainWindow.Properties.UpdateFields();
                            _shouldUpdateProperties = false;
                        }
                    }
                }
            }
            else
            {
                selectionBox.SetBoundObject(null);
                if (_shouldUpdateProperties)
                {
                    MainWindow.mainWindow.Properties.UpdateFields();
                    _shouldUpdateProperties = false;
                }
            }
            MainCamera.UpdateCamera(GraphicsDevice.Viewport, gameTime);
        }

        public void KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                if (sprBoxSelected != "enginereserved_null")
                {
                    int keyFrameIndex = 0;

                    for (int i = 0; i < TimeLine.timeLine.GetKeyframes().Count; i++)
                    {
                        if (i == GetNearestKeyframe())
                        {
                            keyFrameIndex = i;
                        }
                    }
                    
                    TimeLine.timeLine.GetKeyframes()[keyFrameIndex].GetSpriteBoxes().Remove(sprBoxSelected);

                    TimeLine.timeLine.DisplayAtScrubber();
                }
            }
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

                foreach(ResizeBox box in selectionBox.GetChildren())
                {
                    Vector2 transformed = Vector2.Transform(new Vector2(box.GetHoverPoint().X, box.GetHoverPoint().Y), MainCamera.Transform);
                    if (selectionBox.GetBoundObject() != null)
                        SpriteRenderer.DrawRectangle(_guiBatch, GraphicsDevice, new Rectangle((int)transformed.X - 16, (int)transformed.Y - 16, 32, 32), Color.White, 3);
                }
                

                _guiBatch.End();
            }
        }

        public static int GetNearestKeyframe()
        {
            int closestKeyframeTo = -1;
            int closestKeyframeDistance = -1;

            if (TimeLine.timeLine.GetKeyframes().Count > 0)
            {
                for (int i = 0; i < TimeLine.timeLine.GetKeyframes().Count; i++)
                {
                    int scrubberPos = (int)Math.Floor(TimeLineInfo.timelineMs / 16);
                    Keyframe currentFrame = TimeLine.timeLine.GetKeyframes().ElementAt(i);

                    bool isValid = true;

                    if (currentFrame.PositionX > scrubberPos)
                    {

                        isValid = false;
                    }

                    if (isValid)
                    {
                        if (closestKeyframeDistance == -1)
                        {
                            closestKeyframeDistance = Math.Abs(scrubberPos - TimeLine.timeLine.GetKeyframes().ElementAt(i).PositionX);
                            closestKeyframeTo = i;
                        }
                        else
                        {
                            if (Math.Abs(scrubberPos - TimeLine.timeLine.GetKeyframes().ElementAt(i).PositionX) < closestKeyframeDistance)
                            {
                                closestKeyframeDistance = Math.Abs(scrubberPos - TimeLine.timeLine.GetKeyframes().ElementAt(i).PositionX);
                                closestKeyframeTo = i;
                            }
                        }
                    }
                }
            }
            return closestKeyframeTo;
        }

        public string GetSelectedSpriteboxKey()
        {
            return sprBoxSelected;
        }



        Vector2 previousMousePos = Vector2.Zero;
        bool previouseMouseInit = false;

        Vector2 grabPosition = new Vector2();

        // WPF events for input.
        public void MouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseDown)
            {
                _mouseX = (float)e.GetPosition(MainWindow.mainWindow.MainView).X;
                _mouseY = (float)e.GetPosition(MainWindow.mainWindow.MainView).Y;

                worldMousePosition = Vector2.Transform(new Vector2(_mouseX, _mouseY), Matrix.Invert(MainCamera.Transform));
                worldMousePosition = new Vector2((int)worldMousePosition.X, (int)worldMousePosition.Y);
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

            if (previouseMouseInit != false)
            {
                if (e.MiddleButton == MouseButtonState.Pressed)
                {

                    Vector2 worldNew = Vector2.Transform(new Vector2((float)e.GetPosition(MainWindow.mainWindow.MainView).X, (float)e.GetPosition(MainWindow.mainWindow.MainView).Y), Matrix.Invert(MainCamera.Transform));
                    Vector2 worldOld = Vector2.Transform(new Vector2(previousMousePos.X, previousMousePos.Y), Matrix.Invert(MainCamera.Transform));
                    MainCamera.Position = MainCamera.Position + (worldNew - worldOld);
                }
            }
            previouseMouseInit = true;
            previousMousePos = new Vector2((float)e.GetPosition(MainWindow.mainWindow.MainView).X, (float)e.GetPosition(MainWindow.mainWindow.MainView).Y);

            foreach (Drawable drawable in SpriteManager.GetSprites())
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

                    if (selectionRect.Intersects(new Rectangle((int)worldMousePosition.X, (int)worldMousePosition.Y, 1, 1)) && pair.Key != sprBoxSelected && pair.Value.Visible())
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

                _shouldUpdateProperties = true;
            }
                
        }

        public void MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                MainCamera.AdjustZoom(0.2f);
            }
            if (e.Delta < 0)
            {
                MainCamera.AdjustZoom(-0.2f);
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
            previewObject.GetSpriteBoxes().Add($"{previewObject.GetSpriteBoxes().Count}", spriteBox);
            
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

        public SelectionBox GetSelectionBox()
        {
            return selectionBox;
        }
    }
}