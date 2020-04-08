using Microsoft.Win32;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using PestControlAnimator.monogame.content;
using PestControlAnimator.monogame.objects;
using PestControlAnimator.shared;
using PestControlAnimator.shared.animations;
using PestControlAnimator.shared.animations.json;
using PestControlAnimator.wpf.controls;
using PestControlAnimator.wpf.controls.icons;
using PestControlAnimator.wpf.structs;
using PestControlAnimator.wpf.windows;
using PestControlAnimator.wpf.windows.Popups;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace PestControlAnimator
{
    public partial class MainWindow : Window
    {
        public static ProjectManager project { get; set; } = new ProjectManager();

        public static MainWindow mainWindow { get; set; }

        public Preferences Preferences { get; set; } = new Preferences();

        public string PreferencesPath { get; set; } = "preferences.json";

        public ObservableCollection<ProjectListViewElement> ListViewElements { get; }

        private string _CurrentDirectory = null;

        public bool? ShouldReloadTexures = null;

        public MainWindow()
        {
            ListViewElements = new ObservableCollection<ProjectListViewElement>();
            mainWindow = this;
            InitializeComponent();

            if (App.startupPath.Length == 0)
            {
                NewProject projectWindow = new NewProject();
                projectWindow.ShowDialog();
            }
            else
            {
                OpenProject(App.startupPath);
            }

            _CurrentDirectory = project.GetContentPath();

            UpdateExplorer();

            if (!File.Exists(PreferencesPath))
            {
                FileStream stream = File.Create(PreferencesPath);

                stream.Dispose();

                File.WriteAllText(PreferencesPath, JsonConvert.SerializeObject(Preferences, Formatting.Indented));

                string jsonString = File.ReadAllText(PreferencesPath);
                Preferences = JsonConvert.DeserializeObject<Preferences>(jsonString);
            }
            else
            {
                string jsonString = File.ReadAllText(PreferencesPath);

                Preferences = JsonConvert.DeserializeObject<Preferences>(jsonString);
            }

            MainView.AllowDrop = true;
        }

        public void WriteToPreferences()
        {
            File.WriteAllText(PreferencesPath, JsonConvert.SerializeObject(Preferences, Formatting.Indented));
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

        private static T FindAncestor<T>(DependencyObject current)
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
                ListViewItem listViewItem = FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource);

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
                FileInfo info = new FileInfo($"{_CurrentDirectory}/{ViewElement.FileName}");
                string safeFileName = Util.GetRelativePath(project.GetContentPath(), _CurrentDirectory) + info.Name.Substring(0, info.Name.Length - info.Extension.Length);
                
                Console.WriteLine(safeFileName);

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
                OpenProject(openFileDialog.FileName);

                TimeLine.timeLine.DisplayAtScrubber();
                TimeLine.timeLine.DisplayTimelineEnd();
                Properties.UpdateFields();

                ProjectManager.LoadProjectContent();

                if (Directory.Exists(project.GetProjectInfo().ContentPath))
                {
                    _CurrentDirectory = project.GetProjectInfo().ContentPath;
                }

                UpdateExplorer();
            }
        }

        private void ProjectView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ProjectView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ProjectListViewElement element = ProjectView.SelectedItem as ProjectListViewElement;

            if (element != null && element.FolderVisibility == Visibility.Visible)
            {
                if (Directory.Exists($"{_CurrentDirectory}/{element.FileName}"))
                {
                    string dir = $"{_CurrentDirectory}/{element.FileName}";
                    _CurrentDirectory = dir;
                    UpdateExplorer();
                }
            }
        }

        public void OpenProject(string path)
        {
            Stream projectStream;

            if ((projectStream = File.OpenRead(path)) != null)
            {
                project.ProjectPath = $"{path}";
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
        }

        public void UpdateExplorer()
        {
            if (ProjectView.ItemsSource as ObservableCollection<ProjectListViewElement> == null)
            {
                ProjectView.ItemsSource = ListViewElements;
            }

            ObservableCollection<ProjectListViewElement> elements = ProjectView.ItemsSource as ObservableCollection<ProjectListViewElement>;

            elements.Clear();

            if (_CurrentDirectory.Length == 0)
                return;

            foreach (string folder in Directory.GetDirectories(_CurrentDirectory))
            {
                FileInfo info = new FileInfo(folder);
                string fileName = info.Name;

                ProjectListViewElement element = new ProjectListViewElement()
                {
                    FileName = fileName,
                    FolderVisibility = Visibility.Visible,
                    FileVisibility = Visibility.Hidden
                };

                elements.Add(element);
            }

            foreach (string file in Directory.GetFiles(_CurrentDirectory))
            {
                FileInfo info = new FileInfo(file);
                string fileName = info.Name;

                ProjectListViewElement element = new ProjectListViewElement()
                {
                    FileName = fileName,
                    FolderVisibility = Visibility.Hidden,
                    FileVisibility = Visibility.Visible
                };

                elements.Add(element);
            }
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UpdateExplorer();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (Preferences.ShowWarningOnReload)
            {
                ShouldReloadTextures warningWindow = new ShouldReloadTextures();
                warningWindow.ShowDialog();
            }

            if (ShouldReloadTexures != false)
            {
                ContentManager.UnloadAllTextures();

                ProjectManager.LoadProjectContent();

                Console.WriteLine("Textures Reloaded.");
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (_CurrentDirectory != project.GetContentPath())
            {
                _CurrentDirectory = Directory.GetParent(_CurrentDirectory).FullName;
                UpdateExplorer();
            }
        }

        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            MassMoveSprites massMoveSprites = new MassMoveSprites();
            massMoveSprites.ShowDialog();
        }
    }
}
