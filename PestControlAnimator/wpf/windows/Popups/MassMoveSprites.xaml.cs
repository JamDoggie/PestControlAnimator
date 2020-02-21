using Microsoft.Xna.Framework;
using PestControlAnimator.monogame.objects;
using PestControlAnimator.wpf.controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PestControlAnimator.wpf.windows.Popups
{
    /// <summary>
    /// Interaction logic for ShouldReloadTextures.xaml
    /// </summary>
    public partial class MassMoveSprites : Window
    {
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;

        internal class NativeMethods
        {
            [DllImport("user32.dll", SetLastError = true)]
            public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
            [DllImport("user32.dll")]
            public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        }

        public MassMoveSprites()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Hides x button, a bit jank but it works.
            var hwnd = new WindowInteropHelper(this).Handle;
            // this unused int is sitting here so that the returned value is used, it is immediately disposed of by the garbage collector.
            int i = NativeMethods.SetWindowLong(hwnd, GWL_STYLE, NativeMethods.GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();

            foreach(Keyframe frame in TimeLine.timeLine.GetKeyframes())
            {
                foreach(KeyValuePair<string, Spritebox> pair in frame.GetSpriteBoxes())
                {
                    // Check if all user inputs are valid
                    float posX = 0;
                    float posY = 0;
                    if (float.TryParse(amountx.Text, out posX) && float.TryParse(amounty.Text, out posY) && pair.Key == nametomove.Text)
                    {
                        pair.Value.SetPosition(pair.Value.GetPosition() + new Vector2(posX, posY));
                    }

                    TimeLine.timeLine.DisplayAtScrubber();

                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
