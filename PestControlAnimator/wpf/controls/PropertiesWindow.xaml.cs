using PestControlAnimator.monogame.content;
using PestControlAnimator.monogame.objects;
using PestControlAnimator.MonoGameControls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
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
    /// Interaction logic for PropertiesWindow.xaml
    /// </summary>
    public partial class PropertiesWindow : UserControl
    {
        public PropertiesWindow()
        {
            InitializeComponent();
        }

        private void Canvas_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateFields();
            UpdateSpriteTree();
        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateFields();
            UpdateSpriteTree();
        }

        public void UpdateFields()
        {
            if (MainWindowViewModel.MonogameWindow == null)
                return;

            int rectangleHeight = 20;

            SpritePropertiesCanvas.Children.Clear();

            if (MainWindowViewModel.MonogameWindow.GetPreviewObject() != null)
            {
                int keyFrameIndex = 0;

                for (int i = 0; i < TimeLine.timeLine.GetKeyframes().Count; i++)
                {
                    if (i == MainWindowViewModel.GetNearestKeyframe())
                    {
                        keyFrameIndex = i;
                    }
                }

                Spritebox spriteBox = MainWindowViewModel.MonogameWindow.GetSelectionBox().GetBoundObject();

                Spritebox validateBox = null;
                if (TimeLine.timeLine.GetKeyframes().Count > 0)
                    TimeLine.timeLine.GetKeyframes()[keyFrameIndex].GetSpriteBoxes().TryGetValue(MainWindowViewModel.MonogameWindow.GetSelectedSpriteboxKey(), out validateBox);

                if (spriteBox != null && validateBox != null)
                {
                    FieldInfo[] fields = typeof(Spritebox).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

                    int currentIndex = 0;

                    PropertyStrip propertyStripName = new PropertyStrip();

                    propertyStripName.Width = SpritePropertiesCanvas.ActualWidth;

                    propertyStripName.PropertyHeader.Text = "name";

                    // (SpritePropertiesCanvas is the canvas that we're adding these property strips to and also the thing we're changing the color of)
                    SpritePropertiesCanvas.Background = new SolidColorBrush(Color.FromRgb(53, 53, 63));
                    if (currentIndex % 2 == 0)
                    {
                        propertyStripName.StripBackground.Fill = new SolidColorBrush(Color.FromRgb(53, 53, 63));
                        SpritePropertiesCanvas.Background = new SolidColorBrush(Color.FromRgb(47, 47, 56));
                    }

                    foreach(KeyValuePair<string, Spritebox> pair in MainWindowViewModel.MonogameWindow.GetPreviewObject().GetSpriteBoxes())
                    {
                        if (pair.Value == spriteBox)
                        {
                            propertyStripName.PropertyTextBox.Text = pair.Key;
                        }
                    }

                    propertyStripName.PropertyTextBox.TextChanged += (sender, e) =>
                    {
                        string forbiddenPrefix = "enginereserved_";
                        int forbiddenPrefixLength = forbiddenPrefix.Length;

                        bool nameExists = false;

                        Spritebox spriteBoxRename = MainWindowViewModel.MonogameWindow.GetSelectionBox().GetBoundObject();

                        // Check if there is a sprite box with the name in the text box.
                        foreach (KeyValuePair<string, Spritebox> pair in TimeLine.timeLine.GetKeyframes()[keyFrameIndex].GetSpriteBoxes())
                        {
                            if (pair.Key == propertyStripName.PropertyTextBox.Text && pair.Value != spriteBoxRename)
                            {
                                nameExists = true;
                            }
                        }

                        // Check if the name is valid
                        if ((propertyStripName.PropertyTextBox.Text.Length >= forbiddenPrefixLength && propertyStripName.PropertyTextBox.Text.Substring(0, forbiddenPrefixLength) == forbiddenPrefix) || (propertyStripName.PropertyTextBox.Text.Contains('"')) || nameExists)
                        {
                            // Name is invalid.
                            propertyStripName.PropertyTextBox.Background = new SolidColorBrush(Color.FromRgb(166, 49, 49));
                        }
                        else
                        {
                            // Name is valid, set the color of the text box back and change the name of the spritebox.
                            propertyStripName.PropertyTextBox.Background = Brushes.White;

                            KeyValuePair<string, Spritebox> oldPair = new KeyValuePair<string, Spritebox>();

                            foreach(KeyValuePair<string, Spritebox> pair in TimeLine.timeLine.GetKeyframes()[keyFrameIndex].GetSpriteBoxes())
                            {
                                if (pair.Key == MainWindowViewModel.MonogameWindow.getSprBoxSelected())
                                {
                                    oldPair = pair;
                                }
                            }

                            // Remove keyvalue pair so we can add it again with the new updated key.
                            if (MainWindowViewModel.MonogameWindow.GetSelectedSpriteboxKey() != "enginereserved_null")
                            {
                                TimeLine.timeLine.GetKeyframes().ElementAt(keyFrameIndex).RemoveSpriteBox(MainWindowViewModel.MonogameWindow.GetSelectedSpriteboxKey());
                                
                            }

                            KeyValuePair<string, Spritebox> newPair = new KeyValuePair<string, Spritebox>(propertyStripName.PropertyTextBox.Text, oldPair.Value);
                            TimeLine.timeLine.GetKeyframes()[keyFrameIndex].AddSpriteBox(newPair.Key, oldPair.Value);
                            TimeLine.timeLine.DisplayAtScrubber();
                            MainWindowViewModel.MonogameWindow.setSprBoxSelected(propertyStripName.PropertyTextBox.Text);
                        }
                    };
                    

                    SpritePropertiesCanvas.Children.Add(propertyStripName);

                    Canvas.SetLeft(propertyStripName, 0);
                    Canvas.SetTop(propertyStripName, currentIndex * rectangleHeight);


                    currentIndex++;

                    foreach (FieldInfo field in fields)
                    {
                        // Add property field for modifying this property.
                        if (field.FieldType == typeof(int) || field.FieldType == typeof(float) || field.FieldType == typeof(string))
                        {
                            PropertyStrip propertyStrip = new PropertyStrip();
                            propertyStrip.PropertyHeader.Text = field.Name;
                            propertyStrip.Width = SpritePropertiesCanvas.ActualWidth;

                            SpritePropertiesCanvas.Background = new SolidColorBrush(Color.FromRgb(53, 53, 63));
                            if (currentIndex % 2 == 0)
                            {
                                propertyStrip.StripBackground.Fill = new SolidColorBrush(Color.FromRgb(53, 53, 63));
                                SpritePropertiesCanvas.Background = new SolidColorBrush(Color.FromRgb(47, 47, 56));
                            }

                            SpritePropertiesCanvas.Children.Add(propertyStrip);

                            Canvas.SetLeft(propertyStrip, 0);
                            Canvas.SetTop(propertyStrip, currentIndex * rectangleHeight);

                            // Set textbox text to current value
                            if (field.FieldType == typeof(int))
                                propertyStrip.PropertyTextBox.Text = ((int)field.GetValue(validateBox)).ToString(CultureInfo.CurrentCulture);

                            if (field.FieldType == typeof(float))
                                propertyStrip.PropertyTextBox.Text = ((float)field.GetValue(validateBox)).ToString(CultureInfo.CurrentCulture);

                            if (field.FieldType == typeof(string))
                                propertyStrip.PropertyTextBox.Text = (string)field.GetValue(validateBox);

                            propertyStrip.PropertyTextBox.TextChanged += (sender, e) =>
                            {
                                // INTEGER BOX
                                if (field.FieldType == typeof(int))
                                {
                                    int parseOut = 0;
                                    if (int.TryParse(propertyStrip.PropertyTextBox.Text, out parseOut))
                                    {
                                        if (field != null)
                                        {
                                            field.SetValue(validateBox, parseOut);
                                        }
                                        propertyStrip.PropertyTextBox.Background = Brushes.White;

                                        TimeLine.timeLine.DisplayAtScrubber();

                                        foreach(KeyValuePair<string, Spritebox> sprPair in TimeLine.timeLine.GetKeyframes()[keyFrameIndex].GetSpriteBoxes())
                                        {
                                            if (sprPair.Value == validateBox)
                                            {
                                                MainWindowViewModel.MonogameWindow.setSprBoxSelected(sprPair.Key);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        propertyStrip.PropertyTextBox.Background = new SolidColorBrush(Color.FromRgb(166, 49, 49));
                                    }
                                }

                                // FLOAT BOX
                                if (field.FieldType == typeof(float))
                                {
                                    float parseOut = 0;
                                    if (float.TryParse(propertyStrip.PropertyTextBox.Text, out parseOut))
                                    {
                                        if (field != null)
                                        {
                                            field.SetValue(validateBox, parseOut);
                                        }
                                        propertyStrip.PropertyTextBox.Background = Brushes.White;

                                        TimeLine.timeLine.DisplayAtScrubber();

                                        foreach (KeyValuePair<string, Spritebox> sprPair in TimeLine.timeLine.GetKeyframes()[keyFrameIndex].GetSpriteBoxes())
                                        {
                                            if (sprPair.Value == validateBox)
                                            {
                                                MainWindowViewModel.MonogameWindow.setSprBoxSelected(sprPair.Key);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        propertyStrip.PropertyTextBox.Background = new SolidColorBrush(Color.FromRgb(166, 49, 49));
                                    }
                                }

                                // STRING BOX
                                if (field.FieldType == typeof(string))
                                {
                                    if (!propertyStrip.PropertyTextBox.Text.Contains('"'))
                                    {
                                        if (field != null)
                                        {
                                            field.SetValue(validateBox, propertyStrip.PropertyTextBox.Text);
                                        }

                                        if (field.Name == "_textureKey")
                                        {
                                            // Turn yellow if the sprite key isn't loaded but it is a valid string.
                                            if (ContentManager.GetTexture(propertyStrip.PropertyTextBox.Text) == null)
                                                propertyStrip.PropertyTextBox.Background = Brushes.Yellow;
                                            else
                                                propertyStrip.PropertyTextBox.Background = Brushes.White;
                                        }
                                        

                                        TimeLine.timeLine.DisplayAtScrubber();

                                        foreach (KeyValuePair<string, Spritebox> sprPair in TimeLine.timeLine.GetKeyframes()[keyFrameIndex].GetSpriteBoxes())
                                        {
                                            if (sprPair.Value == validateBox)
                                            {
                                                MainWindowViewModel.MonogameWindow.setSprBoxSelected(sprPair.Key);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        propertyStrip.PropertyTextBox.Background = new SolidColorBrush(Color.FromRgb(166, 49, 49));
                                    }
                                }
                            };

                            currentIndex++;
                        }

                        if (field.FieldType == typeof(Microsoft.Xna.Framework.Vector2))
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                // POSITION X AND Y
                                PropertyStrip propertyStripVector = new PropertyStrip();
                                Microsoft.Xna.Framework.Vector2 vector2 = (Microsoft.Xna.Framework.Vector2)field.GetValue(validateBox);

                                propertyStripVector.Width = SpritePropertiesCanvas.ActualWidth;

                                if (i == 0)
                                {
                                    propertyStripVector.PropertyHeader.Text = $"{field.Name}.X";
                                    propertyStripVector.PropertyTextBox.Text = vector2.X.ToString(CultureInfo.CurrentCulture);
                                }
                                else
                                {
                                    propertyStripVector.PropertyHeader.Text = $"{field.Name}.Y";
                                    propertyStripVector.PropertyTextBox.Text = vector2.Y.ToString(CultureInfo.CurrentCulture);
                                }

                                // (SpritePropertiesCanvas is the canvas that we're adding these property strips to and also the thing we're changing the color of)
                                SpritePropertiesCanvas.Background = new SolidColorBrush(Color.FromRgb(53, 53, 63));
                                if (currentIndex % 2 == 0)
                                {
                                    propertyStripVector.StripBackground.Fill = new SolidColorBrush(Color.FromRgb(53, 53, 63));
                                    SpritePropertiesCanvas.Background = new SolidColorBrush(Color.FromRgb(47, 47, 56));
                                }

                                SpritePropertiesCanvas.Children.Add(propertyStripVector);

                                Canvas.SetLeft(propertyStripVector, 0);
                                Canvas.SetTop(propertyStripVector, currentIndex * rectangleHeight);

                                propertyStripVector.PropertyTextBox.TextChanged += (sender, e) =>
                                {
                                    

                                    if (float.TryParse(propertyStripVector.PropertyTextBox.Text, out float outParse))
                                    {
                                        if (propertyStripVector.PropertyHeader.Text == $"{field.Name}.X")
                                        {
                                            vector2.X = outParse;
                                            
                                        }
                                        Console.WriteLine(vector2.X);
                                        if (propertyStripVector.PropertyHeader.Text == $"{field.Name}.Y")
                                        {
                                            vector2.Y = outParse;
                                        }

                                        propertyStripVector.PropertyTextBox.Background = Brushes.White;
                                    }
                                    else
                                    {
                                        propertyStripVector.PropertyTextBox.Background = new SolidColorBrush(Color.FromRgb(166, 49, 49));
                                    }

                                    field.SetValue(validateBox, vector2);

                                    TimeLine.timeLine.DisplayAtScrubber();

                                    foreach (KeyValuePair<string, Spritebox> sprPair in TimeLine.timeLine.GetKeyframes()[keyFrameIndex].GetSpriteBoxes())
                                    {
                                        if (sprPair.Value == validateBox)
                                        {
                                            MainWindowViewModel.MonogameWindow.setSprBoxSelected(sprPair.Key);
                                        }
                                    }
                                };

                                currentIndex++;
                            }
                        }
                    }
                }
            }
        }

        public void UpdateSpriteTree()
        {
            if (TimeLine.timeLine.GetKeyframes().Count <= 0)
                return;

            SpriteTreeCanvas.Children.Clear();

            int keyFrameIndex = 0;

            for (int i = 0; i < TimeLine.timeLine.GetKeyframes().Count; i++)
            {
                if (i == MainWindowViewModel.GetNearestKeyframe())
                {
                    keyFrameIndex = i;
                }
            }

            Keyframe keyFrame = TimeLine.timeLine.GetKeyframes()[keyFrameIndex];

            int count = 0;

            foreach(KeyValuePair<string, Spritebox> pair in keyFrame.GetSpriteBoxes())
            {
                WordCheckStrip checkStrip = new WordCheckStrip();
                checkStrip.MainTextName.Text = pair.Key;

                SpriteTreeCanvas.Children.Add(checkStrip);
                Canvas.SetTop(checkStrip, count * 20);
                checkStrip.Width = SpritePropertiesCanvas.Width;
                checkStrip.Height = 20;
                checkStrip.MainCheckbox.IsChecked = pair.Value.Visible();

                checkStrip.MainCheckbox.Click += (sender, e) =>
                {
                    Spritebox sprBox = null;
                    keyFrame.GetSpriteBoxes().TryGetValue(pair.Key, out sprBox);

                    if (sprBox != null)
                    {
                        sprBox.SetVisible((bool)checkStrip.MainCheckbox.IsChecked);

                        TimeLine.timeLine.DisplayAtScrubber();
                    }
                };

                count++;
            }
        }
    }
}
