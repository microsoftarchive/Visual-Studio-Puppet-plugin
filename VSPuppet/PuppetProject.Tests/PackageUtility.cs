
namespace PuppetProject.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using MicrosoftOpenTech.PuppetProject;
    using FilesToPack = System.Collections.Generic.List<System.Tuple<System.IO.FileInfo, string>>;

    static class PackageUtility
    {

        internal static FilesToPack CreateFilesToPack()
        {
            var moduleStructDir = new DirectoryInfo(@"../../Data/TestPuppetModule");
            var project = new OAProject();

            foreach (var dir in moduleStructDir.GetDirectories())
            {
                var sfi = new OAFolderItem(dir.Name);
                project.ProjectItems.Add(sfi);

                var subfolder = string.Empty;
                foreach (var item in dir.GetFileSystemInfos())
                {
                    PackageUtility.CreateProjectSructureRec(sfi, item, subfolder);
                }
            }

            foreach (var file in moduleStructDir.GetFiles())
            {
                var fi = new OAFileItem(new FileNode(file.FullName));
                project.ProjectItems.Add(fi);
            }

            return PackageUtility.GetFileStructure(project);
        }

        private static void CreateProjectSructureRec(OAFolderItem folderItem, FileSystemInfo fsi, string subfolder)
        {
            if ((fsi.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                var directoryInfo = fsi as DirectoryInfo;
                if (directoryInfo == null) return;

                var sfi = new OAFolderItem(directoryInfo.Name);
                folderItem.ProjectItems.Add(sfi);
                subfolder = Path.Combine(subfolder, directoryInfo.Name);

                foreach (var item in directoryInfo.GetFileSystemInfos())
                {
                    PackageUtility.CreateProjectSructureRec(sfi, item, subfolder);
                }
            }
            else
            {
                var fi = new OAFileItem(new FileNode(fsi.FullName));
                folderItem.ProjectItems.Add(fi);
            }
        }

        private static FilesToPack GetFileStructure(OAProject project)
        {
            var filesToPack = new FilesToPack();

            foreach (var projectItem in project.ProjectItems)
            {
                var fileItem = projectItem as OAFileItem;
                if (fileItem != null)
                {
                    var fileNode = fileItem.Object as FileNode;
                    if (fileNode != null)
                    {
                        filesToPack.Add(new Tuple<FileInfo, string>(new FileInfo(fileNode.Url), string.Empty));
                    }
                }
                else if (projectItem is OAFolderItem)
                {
                    var q = new Queue<object>();
                    q.Enqueue(projectItem);

                    var subfolder = string.Empty;
                    while (q.Count > 0)
                    {
                        var folderItem = q.Dequeue() as OAFolderItem;
                        if (folderItem == null) continue;
                        subfolder = Path.Combine(subfolder, folderItem.Name);
                        foreach (var item in folderItem.ProjectItems)
                        {
                            if (item is OAFolderItem)
                            {
                                q.Enqueue(item);
                            }
                            else if (item is OAFileItem)
                            {
                                var fileNode = (item as OAFileItem).Object as FileNode;
                                if (fileNode != null)
                                {
                                    filesToPack.Add(new Tuple<FileInfo, string>(new FileInfo(fileNode.Url), subfolder));
                                }
                            }
                        }
                    }
                }

                // ignore other types
            }

            if (filesToPack.Count == 0)
                throw new Exception(Resources.EmptyModule);

            return filesToPack;
        }
    }
}
