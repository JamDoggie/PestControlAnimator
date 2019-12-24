using PestControlAnimator.monogame.objects;
using PestControlAnimator.shared;
using PestControlAnimator.shared.animation;
using PestControlAnimator.shared.animation.json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PestControlAnimator.wpf.controls
{
    /// <summary>
    /// Interaction logic for TimeLine.xaml
    /// </summary>
    public partial class TimeLine : UserControl
    {
        private List<Keyframe> _KeyFrames = new List<Keyframe>();

        public int SelectedKeyframe { get; set; } = -1;

        public int PaddingX { get; set; } = 20;
        private int PaddingY { get; set; } = 20;

        public double ScreenScale { get; set; } = 60.0d;

        public double TimeLineOffset { get; set; } = 0.0d;

        public bool CanMoveScrubber { get; set; } = true;

        public int TimeLineEnd { get; set; } = 10;

        public static TimeLine timeLine = null;

        public TimeLine()
        {
            InitializeComponent();

            timeLine = this;

            Scrubber.Width = 2;
            Canvas.SetLeft(Scrubber, -1);
            Canvas.SetTop(Scrubber, 0);
            //var boolean = Mouse.Capture(this);
        }

        public void AddKeyframe(Keyframe keyframe)
        {
            if (keyframe != null)
            {
                _KeyFrames.Add(keyframe);
                TimeLineCanvas.Children.Add(keyframe);
            }
        }

        public void RemoveKeyframe(Keyframe keyframe)
        {
            _KeyFrames.Remove(keyframe);
            TimeLineCanvas.Children.Remove(keyframe);
        }

        public ref List<Keyframe> GetKeyframes()
        {
            return ref _KeyFrames;
        }
        // Start button
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TimeLineInfo.isPlaying = true;
        }

        // Stop button
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            TimeLineInfo.isPlaying = false;
        }

        private void TimeGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && CanMoveScrubber)
            {
                Canvas.SetLeft(Scrubber, e.GetPosition(MainWindow.mainWindow.MainTimeline).X);
                TimeLineInfo.timelineMs = ((e.GetPosition(MainWindow.mainWindow.MainTimeline).X) * 16 / ScreenScale) + (GetRealScreenOffset() * 16 / ScreenScale);
            }

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Clear sprite boxes from preview and replace them with the last keyframe's spriteboxes or none if there are no keyframes.
                MainWindowViewModel.MonogameWindow.GetPreviewObject().SetSpriteBoxes(new Dictionary<string, Spritebox>());
                MainWindowViewModel.MonogameWindow.setSprBoxSelected("enginereserved_null");

                DisplayAtScrubber();

                foreach (Keyframe keyFrame in _KeyFrames)
                {
                    if (keyFrame.Selected)
                    {
                        int scrubPos = (int)Math.Floor((e.GetPosition(TimeLineCanvas).X + GetRealScreenOffset()) / ScreenScale);

                        bool isFrameAtScrubber = false;

                        foreach(Keyframe frame in _KeyFrames)
                        {
                            if (frame.PositionX == scrubPos)
                            {
                                isFrameAtScrubber = true;
                            }
                        }

                        if (!isFrameAtScrubber)
                            keyFrame.PositionX = scrubPos;

                        CanMoveScrubber = false;
                        keyFrame.rect3761.Fill = new SolidColorBrush(Color.FromRgb(138, 120, 55));
                    }
                    else
                    {
                        keyFrame.rect3761.Fill = new SolidColorBrush(Color.FromRgb(160, 139, 64));
                    }
                }
            }
        }

        public void DisplayTimelineEnd()
        {
            List<UIElement> toRemove = new List<UIElement>();

            // Find what elements are rectangles and need to be cleared.
            foreach (UIElement element in TimeLineCanvas.Children)
            {
                if (element is Rectangle)
                {
                    toRemove.Add(element);
                }
            }

            // Clear all the rectangles we found so we can start fresh without removing other controls like the scrubber or keyframes.
            foreach (UIElement element in toRemove)
            {
                TimeLineCanvas.Children.Remove(element);
            }

            Rectangle rect = new Rectangle
            {
                Stroke = new SolidColorBrush(Color.FromRgb(46, 46, 54)),
                Visibility = Visibility.Visible
            };

            double rectX = TimeLineEnd * ScreenScale - GetRealScreenOffset();

            if (rectX > TimeLineCanvas.ActualWidth)
                return;

            rect.Width = TimeLineCanvas.ActualWidth - rectX;
            //rect.Width = 100;
            rect.Height = TimeLineCanvas.ActualHeight;
            rect.StrokeThickness = rect.Width;

            Panel.SetZIndex(rect, -9);

            TimeLineCanvas.Children.Add(rect);

            Canvas.SetLeft(rect, rectX);
        }

        /// <summary>
        /// Displays current animation in preview window.
        /// </summary>
        public void DisplayAtScrubber()
        {
            int closestKeyframeTo = -1;
            int closestKeyframeDistance = -1;

            if (_KeyFrames.Count > 0)
            {
                for (int i = 0; i < _KeyFrames.Count; i++)
                {
                    int scrubberPos = (int)Math.Floor(TimeLineInfo.timelineMs / 16);
                    Keyframe currentFrame = _KeyFrames.ElementAt(i);

                    bool isValid = true;

                    if (currentFrame.PositionX > scrubberPos)
                    {
                        
                        isValid = false;
                    }
                        
                    if (isValid)
                    {
                        if (closestKeyframeDistance == -1)
                        {
                            closestKeyframeDistance = Math.Abs(scrubberPos - _KeyFrames.ElementAt(i).PositionX);
                            closestKeyframeTo = i;
                        }
                        else
                        {
                            if (Math.Abs(scrubberPos - _KeyFrames.ElementAt(i).PositionX) < closestKeyframeDistance)
                            {
                                closestKeyframeDistance = Math.Abs(scrubberPos - _KeyFrames.ElementAt(i).PositionX);
                                closestKeyframeTo = i;
                            }
                        }
                    }
                }
            }

            if (closestKeyframeTo >= 0 && _KeyFrames.Count > 0)
            {
                Keyframe originalKeyframe = _KeyFrames.ElementAt(closestKeyframeTo);

                Dictionary<string, Spritebox> sprBoxes = new Dictionary<string, Spritebox>();

                foreach (KeyValuePair<string, Spritebox> pair in originalKeyframe.GetSpriteBoxes())
                {
                    SpriteboxJson jsonSpr = Spritebox.ToJsonElement(pair.Value);
                    Spritebox spriteBox = Spritebox.FromJsonElement(jsonSpr);
                    string newKey = pair.Key.Substring(0, pair.Key.Length);
                    sprBoxes.Add(newKey, spriteBox);
                }

                MainWindowViewModel.MonogameWindow.GetPreviewObject().SetSpriteBoxes(sprBoxes);
            }
                
            foreach(Keyframe frame in _KeyFrames)
            {
                Console.WriteLine(_KeyFrames.IndexOf(frame) + "_" + frame.GetSpriteBoxes().Count + "_" + closestKeyframeTo);
            }
        }

        public double GetRealScreenOffset()
        {
            //return TimeLineOffset;
            return 0;
        }

        public void SetScreenOffset(double offset)
        {
            TimeLineOffset = offset;
        }

        /// <summary>
        /// Draws lines if timeline is zoomed in enough to show where keyframes will be snapped to.
        /// </summary>
        public void DrawTimelineSections()
        {
            List<UIElement> toRemove = new List<UIElement>();

            // Find what elements are lines and need to be cleared.
            foreach(UIElement element in TimeLineCanvas.Children)
            {
                if (element is Line)
                {
                    toRemove.Add(element);
                }
            }

            // Clear all the lines we found so we can start fresh without removing other controls like the scrubber or keyframes.
            foreach(UIElement element in toRemove)
            {
                TimeLineCanvas.Children.Remove(element);
            }

            // This makes it so if the screenscale is too small, it won't display a million lines for the keyframe spacing when it's at that point redundant.
            if (ScreenScale < 3.4)
                return;

            // Get how many lines we should render
            int linesInTimeline = (int)(ActualWidth / ScreenScale) + 1;

            for(int i = 0; i <= linesInTimeline; i++)
            {
                Line line = new Line();
                line.Visibility = Visibility.Visible;
                line.StrokeThickness = 2;
                line.Stroke = new SolidColorBrush(Color.FromRgb(46, 46, 54));
                line.X1 = (i * ScreenScale) - (TimeLineOffset % ScreenScale);
                line.X2 = (i * ScreenScale) - (TimeLineOffset % ScreenScale);
                line.Y1 = 0;

                if (double.IsNaN(ActualHeight))
                {
                    line.Y2 = 200;
                }
                else
                {
                    line.Y2 = TimeLineCanvas.ActualHeight;
                }

                Panel.SetZIndex(line, -10);

                TimeLineCanvas.Children.Add(line);
            }
        }

        private void TimeGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScreenScale += e.Delta * 0.005;
            DrawTimelineSections();
            DisplayTimelineEnd();
            if (ScreenScale <= 0)
            {
                ScreenScale = 0.05;
                
            }
        }

        private void TimeLineCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine(SelectedKeyframe + " now");

            bool isKeyframeHovered = false;

            foreach(Keyframe keyFrame in _KeyFrames)
            {
                if (keyFrame.IsMouseOver)
                {
                    isKeyframeHovered = true;
                }
            }

            if (!isKeyframeHovered)
            {
                SelectedKeyframe = -1;
            }
        }

        private void TimeLineCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            foreach (Keyframe keyFrame in _KeyFrames)
            {
                keyFrame.Selected = false;
                keyFrame.rect3761.Fill = new SolidColorBrush(Color.FromRgb(160, 139, 64));
            }
            CanMoveScrubber = true;
        }

        private void TimeGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (!IsMouseOver)
            {
                return;
            }

            if (e.Key == Key.Delete)
            {
                if (SelectedKeyframe >= 0 && _KeyFrames.Count > 0)
                {
                    RemoveKeyframe(_KeyFrames.ElementAt(SelectedKeyframe));
                }
            }

            
        }

        // Create new keyframe button
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            int keyFramePos = (int)Math.Floor(TimeLineInfo.timelineMs / 16);

            Dictionary<string, Spritebox> sprBoxes = new Dictionary<string, Spritebox>();

            foreach(KeyValuePair<string, Spritebox> pair in MainWindowViewModel.MonogameWindow.GetPreviewObject().GetSpriteBoxes())
            {
                SpriteboxJson jsonSpr = Spritebox.ToJsonElement(pair.Value);
                Spritebox spriteBox = Spritebox.FromJsonElement(jsonSpr);
                string newKey = pair.Key.Substring(0, pair.Key.Length);
                sprBoxes.Add(newKey, spriteBox);
            }
            List<Keyframe> toRemove = new List<Keyframe>();

            foreach(Keyframe frame in _KeyFrames)
            {
                if (frame.PositionX == keyFramePos)
                {
                    toRemove.Add(frame);
                }
            }

            foreach(Keyframe frame in toRemove)
            {
                RemoveKeyframe(frame);
            }

            Keyframe newFrame = new Keyframe(keyFramePos, 0, sprBoxes, this);
            AddKeyframe(newFrame);
        }

        private void TimeGrid_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void TimeLineCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void TimeLineCanvas_Initialized(object sender, EventArgs e)
        {
            DrawTimelineSections();
            DisplayTimelineEnd();
        }

        private void TimeLineCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawTimelineSections();
            DisplayTimelineEnd();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            TimeLineOffset -= 10;
            if (TimeLineOffset < 0)
            {
                TimeLineOffset = 0;
            }
            DrawTimelineSections();
            DisplayTimelineEnd();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            TimeLineOffset += 10;
            DrawTimelineSections();
            DisplayTimelineEnd();
        }
    }
}
