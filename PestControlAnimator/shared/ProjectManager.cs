using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using PestControlAnimator.monogame.content;
using PestControlAnimator.monogame.objects;
using PestControlAnimator.shared.animations;
using PestControlAnimator.shared.animations.json;
using PestControlAnimator.shared.json;
using PestControlAnimator.wpf.controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PestControlAnimator.shared
{
    public class ProjectManager
    {
        public string ProjectPath { get; set; } = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        public ProjectInfoJson GetProjectInfo()
        {
            string jsonOutput = "";
            jsonOutput = File.ReadAllText(ProjectPath);

            ProjectInfoJson projectInfo = JsonConvert.DeserializeObject<ProjectInfoJson>(jsonOutput);

            return projectInfo;
        }

        /// <summary>
        /// This returns the content path for the current project and also makes sure the content path exists.
        /// </summary>
        /// <returns></returns>
        public string GetContentPath()
        {
            ProjectInfoJson projInfo = GetProjectInfo();
            if (!Directory.Exists(projInfo.ContentPath))
            {
                try
                {
                    Directory.CreateDirectory(projInfo.ContentPath);
                }
                catch (UnauthorizedAccessException)
                {
                    return "";
                }
            }

            return projInfo.ContentPath;
        }

        /// <summary>
        /// Loads all files that are a valid image in the project directory into memory.
        /// </summary>
        public static void LoadProjectContent()
        {
            if (MainWindow.project.GetProjectInfo().ContentPath.Length == 0 || !Directory.Exists(MainWindow.project.GetProjectInfo().ContentPath))
                return;

            WalkThroughAndLoadSprites(MainWindow.project.GetContentPath());
        }

        private static void WalkThroughAndLoadSprites(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            
            foreach (FileInfo fileInfo in dir.GetFiles())
            {
                if (fileInfo.Extension == ".png")
                {
                    string safeFileName = Util.GetRelativePath(MainWindow.project.GetContentPath(), dir.FullName) + fileInfo.Name.Substring(0, fileInfo.Name.Length - 4);
                    if (ContentManager.GetTexture(safeFileName) == null)
                    {
                        FileStream stream = new FileStream(fileInfo.FullName, FileMode.Open);

                        Texture2D texture = Texture2D.FromStream(MainWindowViewModel.MonogameWindow.GetGraphicsDevice(), stream);
                        ContentManager.LoadTexture(safeFileName.Replace('\\', '/'), texture);

                        stream.Dispose();
                    }
                }
            }

            foreach (DirectoryInfo directory in dir.GetDirectories())
            {
                WalkThroughAndLoadSprites(directory.FullName);
            }
        }

        private void SaveJSON(ProjectInfoJson projectInfo, string path)
        {
            string output = JsonConvert.SerializeObject(projectInfo);

            File.WriteAllText(path, output);
        }

        public void SaveProject(string path, MainWindow window)
        {
            if (window == null)
                return;

            TimeLine TimeLine = window.MainTimeline;

            ProjectInfoJson projectInfo = GetProjectInfo(); 

            projectInfo.animations.Clear();

            foreach (Keyframe keyFrame in TimeLine.GetKeyframes())
            {
                Dictionary<string, SpriteboxJson> spriteBoxes = new Dictionary<string, SpriteboxJson>();

                foreach(KeyValuePair<string, Spritebox> spr in keyFrame.GetSpriteBoxes())
                {
                    spriteBoxes.Add(spr.Key, Spritebox.ToJsonElement(spr.Value));
                }

                Animation animation = new Animation
                {
                    timelineX = keyFrame.PositionX, 
                    spriteBoxes = spriteBoxes
                };

                projectInfo.animations.Add(animation);
            }

            projectInfo.TimelineEnd = TimeLine.TimeLineEnd;
            projectInfo.ProjectSaveIncrement += 1;

            SaveJSON(projectInfo, path);
        }
    }
}
