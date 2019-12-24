using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using PestControlAnimator.monogame.content;
using PestControlAnimator.shared.json;
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
                Directory.CreateDirectory(projInfo.ContentPath);
            }

            return projInfo.ContentPath;
        }

        /// <summary>
        /// Loads all files that are a valid image in the project directory into memory.
        /// </summary>
        public static void LoadProjectContent()
        {
            foreach (string filePath in Directory.GetFiles(MainWindow.project.GetProjectInfo().ContentPath))
            {
                FileInfo fileInfo = new FileInfo(filePath);

                if (fileInfo.Extension == ".png")
                {
                    string safeFileName = fileInfo.Name.Substring(0, fileInfo.Name.Length - 4);
                    if (ContentManager.GetTexture(safeFileName) == null)
                    {
                        FileStream stream = new FileStream(filePath, FileMode.Open);

                        Texture2D texture = Texture2D.FromStream(MainWindowViewModel.MonogameWindow.GetGraphicsDevice(), stream);
                        ContentManager.LoadTexture(safeFileName, texture);

                        stream.Dispose();
                    }
                }
            }
        }
    }
}
