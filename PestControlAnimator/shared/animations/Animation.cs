using PestControlAnimator.monogame.objects;
using PestControlAnimator.shared.animations.json;
using PestControlAnimator.shared.json;
using PestControlAnimator.wpf.controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PestControlAnimator.shared.animations
{
    public class Animation
    {
        public const string headerName = "PestControlAnimationBinaryFile";
        public const int currentVersion = 1;

        public Animation()
        {
            spriteBoxes = new Dictionary<string, SpriteboxJson>();
        }

        public int timelineX { get; set; }

        // This kinda has to be public because it's for json serializing so ignore the CA2227 standard warning in visual studio here.
        public Dictionary<string, SpriteboxJson> spriteBoxes { get; set; }

        public static void WriteAnimationFile(string filePath, ProjectManager projectManager)
        {
            FileInfo file = new FileInfo(filePath);

            // Null checking
            if (file == null)
                return;

            if (projectManager == null)
                return;

            BinaryWriter binaryWriter = new BinaryWriter(file.Open(FileMode.OpenOrCreate));

            // HEADER 
            // PestControlAnimationBinaryFile{version number}
            binaryWriter.Write(headerName);
            binaryWriter.Write(currentVersion);

            // Project info
            binaryWriter.Write(projectManager.GetProjectInfo().Loop);
            binaryWriter.Write(projectManager.GetProjectInfo().LoopToFrame);
            binaryWriter.Write(projectManager.GetProjectInfo().ProjectName);
            binaryWriter.Write(projectManager.GetProjectInfo().TimelineEnd);

            // Keyframes
            // First write amount of keyframes
            ProjectInfoJson projectInfo = projectManager.GetProjectInfo();
            List<Animation> animations = projectInfo.animations;

            binaryWriter.Write(animations.Count);

            foreach(Animation frame in animations)
            {
                binaryWriter.Write(frame.timelineX);

                // Write spritebox info
                // First write amount of SpriteBoxes
                binaryWriter.Write(frame.spriteBoxes.Count);

                foreach(KeyValuePair<string, SpriteboxJson> pair in frame.spriteBoxes)
                {
                    SpriteboxJson spriteBox = pair.Value;

                    // Name of SpriteBox
                    binaryWriter.Write(pair.Key);

                    // Other info of SpriteBox
                    binaryWriter.Write(spriteBox.posX);
                    binaryWriter.Write(spriteBox.posY);
                    binaryWriter.Write(spriteBox.width);
                    binaryWriter.Write(spriteBox.height);
                    binaryWriter.Write(spriteBox.textureKey);
                    binaryWriter.Write(spriteBox.rotation);
                    binaryWriter.Write(spriteBox.sourceX);
                    binaryWriter.Write(spriteBox.sourceY);
                    binaryWriter.Write(spriteBox.sourceWidth);
                    binaryWriter.Write(spriteBox.sourceHeight);
                    binaryWriter.Write(spriteBox.layer);
                }
            }

            binaryWriter.Dispose();
        }

        public static void ReadAnimationFile(string filePath, ProjectManager projectManager, TimeLine timeLine)
        {
            FileInfo file = new FileInfo(filePath);

            // Null checking
            if (file == null)
                return;

            if (projectManager == null)
                return;

            if (timeLine == null)
                return;

            BinaryReader binaryReader = new BinaryReader(File.Open(filePath, FileMode.Open));

            // Check if header is correct(basically check if this is even a valid PCAF file.)
            string headerString = binaryReader.ReadString();

            if (headerString == headerName)
            {
                // Clear keyframes in timeline
                timeLine.ClearKeyframes();

                // Ok cool so this file is a PCAF file.
                // Load version number incase this is ever useful(and because we kinda have to push the reader forward for it to read correctly)
                int versionNumber = binaryReader.ReadInt32();

                // Load Project Info
                binaryReader.ReadBoolean();
                binaryReader.ReadInt32();
                binaryReader.ReadString();
                binaryReader.ReadInt32();

                int keyframeCount = binaryReader.ReadInt32();

                for(int i = 0; i < keyframeCount; i++)
                {
                    

                    // Read timeline x coordinate
                    int timelineX = binaryReader.ReadInt32();

                    int spriteboxCount = binaryReader.ReadInt32();

                    Keyframe keyframe = new Keyframe(timelineX, 0, new Dictionary<string, monogame.objects.Spritebox>(), timeLine);

                    for (int j = 0; j < spriteboxCount; j++)
                    {
                        // Get name
                        string name = binaryReader.ReadString();

                        // General properties
                        double posX = binaryReader.ReadDouble();
                        double posY = binaryReader.ReadDouble();
                        int width = binaryReader.ReadInt32();
                        int height = binaryReader.ReadInt32();
                        string textureKey = binaryReader.ReadString();
                        float rotation = binaryReader.ReadSingle();
                        int sourceX = binaryReader.ReadInt32();
                        int sourceY = binaryReader.ReadInt32();
                        int sourceWidth = binaryReader.ReadInt32();
                        int sourceHeight = binaryReader.ReadInt32();
                        float layer = binaryReader.ReadSingle();

                        SpriteboxJson spriteBox = new SpriteboxJson();
                        spriteBox.posX = posX;
                        spriteBox.posY = posY;
                        spriteBox.width = width;
                        spriteBox.height = height;
                        spriteBox.textureKey = textureKey;
                        spriteBox.rotation = rotation;
                        spriteBox.sourceX = sourceX;
                        spriteBox.sourceY = sourceY;
                        spriteBox.sourceWidth = sourceWidth;
                        spriteBox.sourceHeight = sourceHeight;
                        spriteBox.layer = layer;

                        keyframe.AddSpriteBox(name, Spritebox.FromJsonElement(spriteBox));
                    }

                    timeLine.AddKeyframe(keyframe);
                }

                MainWindowViewModel.MonogameWindow.setSprBoxSelected("enginereserved_null");
                timeLine.DisplayAtScrubber();
            }

            binaryReader.Dispose();
        }
    }
}
