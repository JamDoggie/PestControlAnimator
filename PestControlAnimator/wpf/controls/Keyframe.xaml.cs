using PestControlAnimator.monogame.objects;
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
    /// Interaction logic for Keyframe.xaml
    /// </summary>
    public partial class Keyframe : UserControl
    {
        public int PositionX { get; set; }
        public int PositionY { get; set; }

        private Dictionary<string, Spritebox> _SpriteBoxes = new Dictionary<string, Spritebox>();

        private const int margin = 5;

        public bool Selected { get; set; } = false;
        private TimeLine _timeLine;

        public Keyframe(int posX, int posY, Dictionary<string, Spritebox> sprBoxes, TimeLine timeLine)
        {
            InitializeComponent();

            PositionX = posX;
            PositionY = posY;
            _SpriteBoxes = sprBoxes;
            _timeLine = timeLine;

            if (timeLine != null)
            {
                Canvas.SetLeft(this, (PositionX * _timeLine.ScreenScale) - (ActualWidth / 2) - (_timeLine.GetRealScreenOffset()));
                Canvas.SetTop(this, margin);
            }
        }

        private void mouseEnter(object sender, MouseEventArgs e)
        {
            rect3761.Fill = new SolidColorBrush(Color.FromRgb(138, 120, 55));
        }
        private void mouseLeave(object sender, MouseEventArgs e)
        {
            rect3761.Fill = new SolidColorBrush(Color.FromRgb(160, 139, 64));
        }

        public ref Dictionary<string, Spritebox> GetSpriteBoxes()
        {
            return ref _SpriteBoxes;
        }

        private void rect3761_MouseMove(object sender, MouseEventArgs e)
        {
        }

        private void rect3761_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Selected = true;
            _timeLine.SelectedKeyframe = _timeLine.GetKeyframes().IndexOf(this);
            _timeLine.Focus();
        }

        private void rect3761_MouseUp(object sender, MouseButtonEventArgs e)
        {
            
        }

        public void RemoveSpriteBox(string key)
        {
            _SpriteBoxes.Remove(key);
        }

        public void AddSpriteBox(string key, Spritebox spriteBox)
        {
            if (!_SpriteBoxes.ContainsKey(key))
                _SpriteBoxes.Add(key, spriteBox);
        }
    }
}
