using Microsoft.Win32;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PestControlAnimator.monogame.content;
using PestControlAnimator.monogame.objects;
using PestControlAnimator.shared;
using PestControlAnimator.shared.animations;
using PestControlAnimator.shared.animations.json;
using PestControlAnimator.wpf.controls;
using PestControlAnimator.wpf.structs;
using PestControlAnimator.wpf.windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PestControlAnimator
{
    public partial class MainWindow : Window
    {
        public static ProjectManager project { get; set; } = new ProjectManager();

        public static MainWindow mainWindow { get; set; }

        public MainWindow()
        {
            mainWindow = this;
            InitializeComponent();
            NewProject projectWindow = new NewProject();
            projectWindow.ShowDialog();

            foreach (string file in Directory.GetFiles(project.GetContentPath()))
            {
                FileInfo info = new FileInfo(file);
                string fileName = info.Name;

                ProjectListViewElement element = new ProjectListViewElement()
                {
                    FileName = fileName
                };

                ProjectView.Items.Add(element);
            }

            MainView.AllowDrop = true;

            
        }

        private void MonoGameContentControl_MouseEnter(object sender, MouseEventArgs e)
        {
            MainWindowViewModel.MonogameWindow.MouseEnter(sender, e);
        }

        private void MonoGameContentControl_MouseLeave(object sender, MouseEventArgs e)
        {
            MainWindowViewModel.MonogameWindow.MouseLeave(sender, e);
        }

        private void MonoGameContentControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainWindowViewModel.MonogameWindow.MouseDown(sender, e);
        }

        private void MonoGameContentControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MainWindowViewModel.MonogameWindow.MouseUp(sender, e);
        }

        private void MonoGameContentControl_MouseMove(object sender, MouseEventArgs e)
        {
            MainWindowViewModel.MonogameWindow.MouseMove(sender, e);
        }

        private static T FindAnchestor<T>(DependencyObject current)
    where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }

        // This handles our dragging and dropping to drag png files into the MainView as a spritebox.
        private void ProjectView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ListView listView = sender as ListView;
                ListViewItem listViewItem = FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);

                // If there was no listviewitem selected, break.
                if (listViewItem == null)
                {
                    return;
                }

                ProjectListViewElement element = (ProjectListViewElement)ProjectView.ItemContainerGenerator.ItemFromContainer(listViewItem);

                if (element == null)
                {
                    return;
                }

                DataObject dragData = new DataObject("ProjectListViewElement", element);

                DragDrop.DoDragDrop(listViewItem, dragData, DragDropEffects.Copy | DragDropEffects.Move);
            }
        }

        // For dragging sprites into main sprite view
        private void MainView_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("ProjectListViewElement"))
            {
                e.Effects = DragDropEffects.None;
            }
            else
            {
                ProjectListViewElement ViewElement = (ProjectListViewElement)e.Data.GetData("ProjectListViewElement");
                FileInfo info = new FileInfo($"{project.GetProjectInfo().ContentPath}/{ViewElement.FileName}");

                if (info.Extension != ".png")
                    e.Effects = DragDropEffects.None;
            }
        }

        // For dropping sprites into main sprite view
        private void MainView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("ProjectListViewElement"))
            {
                ProjectListViewElement ViewElement = (ProjectListViewElement)e.Data.GetData("ProjectListViewElement");
                FileInfo info = new FileInfo($"{project.GetProjectInfo().ContentPath}/{ViewElement.FileName}");
                string safeFileName = info.Name.Substring(0, info.Name.Length - info.Extension.Length);

                Texture2D tex = ContentManager.GetTexture(safeFileName);

                if (info.Extension == ".png" && tex != null)
                    MainWindowViewModel.MonogameWindow.AddPreviewSpriteBox(new Spritebox(new Vector2(0, 0), tex.Width, tex.Height, 0, safeFileName, 0, new Rectangle(0, 0, tex.Width, tex.Height), MainWindowViewModel.MonogameWindow.GetPreviewObject()));
            }
        }

        private void MainView_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            MainWindowViewModel.MonogameWindow.MouseWheel(sender, e);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            project.SaveProject(project.ProjectPath, this);
        }

        private void MainView_KeyDown(object sender, KeyEventArgs e)
        {
            MainWindowViewModel.MonogameWindow.KeyDown(sender, e);
        }

        // Export As...
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.Filter = "Pest Control Animation File (*.pcaf)|*.pcaf|All files (*.*)|*.*";

            if (fileDialog.ShowDialog() == true)
            {
                Animation.WriteAnimationFile(fileDialog.FileName, project);
            }
        }

        // Import (Mainly for PCAF files.)
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Pest Control Animation File (*.pcaf)|*.pcaf|All files (*.*)|*.*";

            if (fileDialog.ShowDialog() == true)
            {
                Animation.ReadAnimationFile(fileDialog.FileName, project, MainTimeline);
            }
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            // Open project
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PestControl Engine Animation Project (*.animproj)|*.animproj|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                Stream projectStream;

                if ((projectStream = openFileDialog.OpenFile()) != null)
                {
                    project.ProjectPath = $"{openFileDialog.FileName}";
                    projectStream.Dispose();
                }

                TimeLine.timeLine.ClearKeyframes();

                foreach (Animation animation in project.GetProjectInfo().animations)
                {
                    Dictionary<string, Spritebox> spriteBoxes = new Dictionary<string, Spritebox>();

                    foreach (KeyValuePair<string, SpriteboxJson> pair in animation.spriteBoxes)
                    {
                        SpriteboxJson spritebox = pair.Value;

                        spriteBoxes.Add(pair.Key, Spritebox.FromJsonElement(spritebox));
                    }

                    TimeLine.timeLine.AddKeyframe(new Keyframe(animation.timelineX, 0, spriteBoxes, TimeLine.timeLine));
                }

                TimeLine.timeLine.TimeLineEnd = project.GetProjectInfo().TimelineEnd;

                TimeLine.timeLine.DisplayAtScrubber();
                TimeLine.timeLine.DisplayTimelineEnd();
                Properties.UpdateFields();
            }
        }
    }
}
