using PestControlAnimator.wpf.controls.icons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PestControlAnimator.wpf.structs
{
    public class ProjectListViewElement
    {
        public string FileName { get; set; }

        public Visibility FolderVisibility { get; set; }

        public Visibility FileVisibility { get; set; }
    }
}
