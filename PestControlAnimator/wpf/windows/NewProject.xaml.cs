using Microsoft.Win32;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using PestControlAnimator.monogame.content;
using PestControlAnimator.monogame.objects;
using PestControlAnimator.shared.animation;
using PestControlAnimator.shared.animation.json;
using PestControlAnimator.shared.json;
using PestControlAnimator.wpf.controls;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace PestControlAnimator.wpf.windows
{
    /// <summary>
    /// Interaction logic for NewProject.xaml
    /// </summary>
    public partial class NewProject : Window
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

        public NewProject()
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
            // Create new project
            SaveFileDialog newProjectDialog = new SaveFileDialog();
            newProjectDialog.Filter = "PestControl Engine Animation Project (*.animproj)|*.animproj|All files (*.*)|*.*";
            if (newProjectDialog.ShowDialog() == true)
            {
                FileStream stream = File.Create(newProjectDialog.FileName);
                stream.Close();

                Stream projectStream;

                if ((projectStream = newProjectDialog.OpenFile()) != null)
                {
                    string jsonOutput = "";
                    ProjectInfoJson projectInfo = new ProjectInfoJson()
                    {
                        ContentPath = $"{new FileInfo(newProjectDialog.FileName).Directory.FullName}\\content",
                        ProjectSaveIncrement = 0,
                        ProjectName = newProjectDialog.SafeFileName.Substring(0, newProjectDialog.SafeFileName.Length - 9)
                    };

                    MainWindow.project.ProjectPath = $"{newProjectDialog.FileName}"; 

                    jsonOutput = JsonConvert.SerializeObject(projectInfo);
                    projectStream.Dispose();
                    File.WriteAllText(newProjectDialog.FileName, jsonOutput);
                    
                }

                Close();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // Open project
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PestControl Engine Animation Project (*.animproj)|*.animproj|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                Stream projectStream;

                if ((projectStream = openFileDialog.OpenFile()) != null)
                {
                    MainWindow.project.ProjectPath = $"{openFileDialog.FileName}";
                    projectStream.Dispose();
                }

                Close();

                TimeLine.timeLine.GetKeyframes().Clear();

                foreach(Animation animation in MainWindow.project.GetProjectInfo().animations)
                {
                    Dictionary<string, Spritebox> spriteBoxes = new Dictionary<string, Spritebox>();

                    foreach(KeyValuePair<string, SpriteboxJson> pair in animation.spriteBoxes)
                    {
                        SpriteboxJson spritebox = pair.Value;

                        spriteBoxes.Add(pair.Key, new Spritebox(new Vector2((float)spritebox.posX, (float)spritebox.posY), spritebox.width, spritebox.height, spritebox.rotation, 
                            pair.Value.textureKey, new Rectangle(spritebox.sourceX, spritebox.sourceY, spritebox.sourceWidth, spritebox.sourceHeight), null));
                    }

                    TimeLine.timeLine.AddKeyframe(new Keyframe(animation.timelineX, 0, spriteBoxes, TimeLine.timeLine));
                }

                TimeLine.timeLine.TimeLineEnd = MainWindow.project.GetProjectInfo().TimelineEnd;
            }
        }
    }
    
}
