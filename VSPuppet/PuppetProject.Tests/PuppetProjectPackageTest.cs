

namespace PuppetProject.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using MicrosoftOpenTech.PuppetProject;
    using FilesToPack = System.Collections.Generic.List<System.Tuple<System.IO.FileInfo, string>>;
    using ForgeData = System.Collections.Generic.Dictionary<string, string>;
    using SELF = PuppetProjectPackageTest;


    [TestClass]
    public class PuppetProjectPackageTest
    {
        private const string ForgeUserName = "testUser";
        private const string ForgeModuleName = "testModule";
        private const string ForgeModuleVersion = "1.2.3";
        private const string ForgeModuleDependency = "'namespace/depmodule', '>=3.2.1'";
        private const string ForgeModuleSummary = "test module summary";
        private const string ForgeModuleDescription = "test module description";

        private static ForgeData CreateForgeData()
        {
            return new ForgeData
            {
                { Conatants.PuppetForgeUserName, ForgeUserName},
                { Conatants.PuppetForgeModuleName, ForgeModuleName},
                { Conatants.PuppetForgeModuleVersion, ForgeModuleVersion},
                { Conatants.PuppetForgeModuleDependency, ForgeModuleDependency},
                { Conatants.PuppetForgeModuleSummary, ForgeModuleSummary},
                { Conatants.PuppetForgeModuleDescription, ForgeModuleDescription},
            };
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException), "NullReferenceException is expected.")]
        public void GetActiveProjectStruture_NullReferenceExceptionTest()
        {

            var puppetProjectPackage = new PuppetProjectPackage();
            PuppetProjectNode puppetProjectNode;
            FilesToPack ftp = puppetProjectPackage.GetActiveProjectStruture(out puppetProjectNode);
        }

        [TestMethod]
        public void GetFileStructure_Test()
        {
            var filesToPack = PackageUtility.CreateFilesToPack();
            Assert.IsNotNull(filesToPack);
            Assert.AreEqual(filesToPack.Count, 12);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "ArgumentNullException is expected.")]
        public void CreateJsonMetadata_ArgumentNullException1Test()
        {
            var filesToPack = PackageUtility.CreateFilesToPack();
            PuppetProjectPackage.CreateJsonMetadata(null, filesToPack);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "ArgumentNullException is expected.")]
        public void CreateJsonMetadata_ArgumentNullException2Test()
        {
            PuppetProjectPackage.CreateJsonMetadata(SELF.CreateForgeData(), null);
        }

        [TestMethod]
        [ExpectedException(typeof(System.Collections.Generic.KeyNotFoundException), "KeyNotFoundException is expected.")]
        public void CreateJsonMetadata_KeyNotFoundExceptionTest()
        {
            var filesToPack = PackageUtility.CreateFilesToPack();
            var forgeData = SELF.CreateForgeData();
            forgeData.Remove(Conatants.PuppetForgeModuleDependency);
            PuppetProjectPackage.CreateJsonMetadata(forgeData, filesToPack);
        }

        [TestMethod]
        public void CreateJsonMetadata_Test()
        {
            var filesToPack = PackageUtility.CreateFilesToPack();
            var forgeData = SELF.CreateForgeData();

            const string module = @"ns/module";
            const string version = @"2.3.x";
            forgeData[Conatants.PuppetForgeModuleDependency] = string.Format(@"'{0}', '{1}'", module, version);
            var metaData = PuppetProjectPackage.CreateJsonMetadata(forgeData, filesToPack);

            Assert.AreEqual(SELF.ForgeUserName, metaData.author);
            Assert.AreEqual(filesToPack.Count, metaData.checksums.Count);
            Assert.AreEqual(module, metaData.dependencies[0].name);
            Assert.AreEqual(version, metaData.dependencies[0].version_requirement);
        }

        [TestMethod]
        public void ParseDependency_Test()
        {
            const string module = @"ns/module";
            const string version = @">=2.3.2";
            var forgeModuleDependency = string.Format(@"'{0}', '{1}'", module, version);
            var dep = PuppetProjectPackage.ParseDependency(forgeModuleDependency);

            Assert.AreEqual(module, dep.name);
            Assert.AreEqual(version, dep.version_requirement);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException), "Dependency format doesn't match the pattern")]
        public void ParsedDependency_FormatException1Test()
        {
            const string module = @"ns/module";
            const string version = @">=2.3.x";  // this is not allowed
            var forgeModuleDependency = string.Format(@"'{0}', '{1}'", module, version);
            var dep = PuppetProjectPackage.ParseDependency(forgeModuleDependency);

            Assert.AreEqual(module, dep.name);
            Assert.AreEqual(version, dep.version_requirement);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException), "Dependency format doesn't match the pattern")]
        public void ParsedDependency_FormatException4Test()
        {
            const string module = @"ns/module";
            const string version = @"2.3.s";  // 'x' expected
            var forgeModuleDependency = string.Format(@"'{0}', '{1}'", module, version);
            var dep = PuppetProjectPackage.ParseDependency(forgeModuleDependency);

            Assert.AreEqual(module, dep.name);
            Assert.AreEqual(version, dep.version_requirement);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException), "Dependency format doesn't match the pattern")]
        public void ParsedDependency_FormatException2Test()
        {
            const string module = @"ns-module"; // forward slash expected
            const string version = @">=2.3.1";  
            var forgeModuleDependency = string.Format(@"'{0}', '{1}'", module, version);
            var dep = PuppetProjectPackage.ParseDependency(forgeModuleDependency);

            Assert.AreEqual(module, dep.name);
            Assert.AreEqual(version, dep.version_requirement);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException), "Dependency format doesn't match the pattern")]
        public void ParsedDependency_FormatException3Test()
        {
            const string module = @"ns/module"; 
            const string version = @">=2.3.1";
            var forgeModuleDependency = string.Format(@"'{0}' '{1}'", module, version); // no comma
            var dep = PuppetProjectPackage.ParseDependency(forgeModuleDependency);

            Assert.AreEqual(module, dep.name);
            Assert.AreEqual(version, dep.version_requirement);
        }



        /*       
        [TestMethod]
        public void CreateFilesToPack_Test()
        {
            // set working dir
            var moduleStructDir = new DirectoryInfo(@"../../Data/TestPuppetModule");
            //            Directory.SetCurrentDirectory(moduleStructDir.ToString());



            var projectNode = new PuppetProjectNode();
            var project = new OAProject(projectNode);

            var subfolder = string.Empty;

            foreach (var dir in moduleStructDir.GetDirectories())
            {
                var sfi = new OAFolderItem(project, projectNode.CreateFolderNode(dir.FullName));
                //project.ProjectItems.Add(sfi);
                SELF.CreateProjectSructure(sfi, dir, subfolder, project, projectNode);
            }

            foreach (var file in moduleStructDir.GetFiles())
            {
                var fi = new OAFileItem(project, projectNode.CreateFileNode(file.FullName));
                //project.ProjectItems.Add(fi);
            }
        }

        private static void CreateProjectSructure(OAFolderItem folderItem, FileSystemInfo fsi, string subfolder, OAProject project, ProjectNode projectNode)
        {
            if ((fsi.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                var directoryInfo = fsi as DirectoryInfo;
                if (directoryInfo == null) return;

                subfolder = Path.Combine(subfolder, fsi.Name);

                foreach (var item in directoryInfo.GetFileSystemInfos())
                {
                    var sfi = new OAFolderItem(project, projectNode.CreateFolderNode(item.FullName));
                    //folderItem.ProjectItems.Add(sfi);
                    SELF.CreateProjectSructure(sfi, item, subfolder,project,projectNode);
                }
            }
            else
            {
                var fi = new OAFileItem(project, projectNode.CreateFileNode(fsi.FullName));
                //folderItem.ProjectItems.Add(fi);
            }
        }

*/





    }
}
