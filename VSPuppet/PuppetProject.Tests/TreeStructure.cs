using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetProject.Tests
{
    internal class OAProject
    {
        public List<object> ProjectItems { get; set; }

        public OAProject()
        {
            ProjectItems = new List<object>();
        }
    }

    internal class OAFileItem
    {
        public object Object { get; set; }

        public OAFileItem(FileNode fileNode)
        {
            Object = fileNode;
        }
    }

    internal class OAFolderItem
    {
        public string Name { get; set; }
        public List<object> ProjectItems { get; set; }

        public OAFolderItem(string name)
        {
            this.Name = name;
            this.ProjectItems = new List<object>();
        }
    }

    internal class FileNode
    {
        public string Url { get; set; }

        public FileNode(string url)
        {
            this.Url = url;
        }
    }

}
