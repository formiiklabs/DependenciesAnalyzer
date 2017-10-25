using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Formiik.DependenciesAnalyzer.Core;
using LibGit2Sharp;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis;
using System.Diagnostics;

namespace Formiik.DependenciesAnalyzer.Test
{
    [TestClass]
    public class ConfigurationManagementTest
    {
        [TestMethod]
        public void GetBranchesTest()
        {
            var gitActionsManager = new GitActionsManager();

            var repoPath = @"C:\Formiik2\formiik-backend";

            var branches = gitActionsManager.GetBranches(repoPath);

            Assert.IsNotNull(branches);
        }

        [TestMethod]
        public void FetchAndCheckoutTest()
        {
            var gitActionsManager = new GitActionsManager();

            var repoPath = @"C:\Test";

            gitActionsManager.FetchAndCheckout(repoPath, "origin/agregar-ordenes-a-ruta-en-la-asignacion");

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void PullChangesTest()
        {
            var gitActionsManager = new GitActionsManager();

            var repoPath = @"C:\Test";
            var username = "martin.rangel@formiik.com";
            var password = "e5p1n0z4";
            var email = "martin.rangel@formiik.com";

            gitActionsManager.PullChanges(repoPath, username, password, email);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void DiffHeadTest()
        {
            var gitActionsManager = new GitActionsManager();

            var repoPath = @"C:\Test";

            gitActionsManager.DiffHead(repoPath);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void GetFilesChangedTest()
        {
            var gitActionsManager = new GitActionsManager();

            var repoPath = @"C:\Formiik2\formiik-backend";

            var archivosChanged = gitActionsManager.GetFilesChanged(repoPath, "DoublePersistentAndCalendar2");

            Assert.IsNotNull(archivosChanged);
        }

        [TestMethod]
        public void GetTreeOfFilesChanges()
        {
            var treeGraph = new TreeGraph();

            var repoPath = @"C:\Formiik2\formiik-backend";

            var branchName = "DoublePersistentAndCalendar2";

            treeGraph.Build(repoPath, branchName);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void GetChangeFile(string pathFile)
        {
            var gitActionsManager = new GitActionsManager();
        }

        [TestMethod]
        public void GetListMethodsOfFileModifiedTest()
        {
            try
            {
                var gitActionsManager = new GitActionsManager();

                var repoPath = @"C:\Test";

                var branchName = "TestBranchDepend";

                var pathFileRelative = "/Mobiik.Popcorn/Infrastructure/Mobiik.Formiik.DataBasePersistence/GeofencePersistance.cs";

                var solutionPath = @"C:\Test\Mobiik.Popcorn\Mobiik.Popcorn.sln";

                var workspace = MSBuildWorkspace.Create();

                workspace.WorkspaceFailed += Workspace_WorkspaceFailed;

                var solution = workspace.OpenSolutionAsync(solutionPath).Result;

                var methods = gitActionsManager.GetListMethodsOfFileModified(repoPath, branchName, solution, pathFileRelative);

                Assert.IsTrue(true);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        private void Workspace_WorkspaceFailed(object sender, WorkspaceDiagnosticEventArgs e)
        {
            Debug.WriteLine(e.Diagnostic.Message);
        }
    }
}
