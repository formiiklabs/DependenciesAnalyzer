using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace Formiik.DependenciesAnalyzer.Core.Test
{
    [TestClass]
    public class MethodAnalyzerTest
    {
        private static MethodAnalyzer methodAnalyzer = null;
        private static TestContext testContextInstance;

        public TestContext TestContextInstance
        {
            get
            {
                return testContextInstance;
            }

            set
            {
                testContextInstance = value;
            }
        }

        [ClassInitialize]
        public static void InitializeTest(TestContext context)
        {
            methodAnalyzer = new MethodAnalyzer();
        }

        [ClassCleanup]
        public static void CleanupTest()
        {
            methodAnalyzer.Dispose();
        }

        [TestMethod]
        public void AnalyzeTest()
        {
            try
            {
                var solutionFile = @"C:\Test\Mobiik.Popcorn\Mobiik.Popcorn.sln";

                var fullNameMethod = "Mobiik.Formiik.DataBasePersistence.Fake.PersistanceFake.GetDescriptionsFake()";

                var workspace = MSBuildWorkspace.Create();

                workspace.WorkspaceFailed += Workspace_WorkspaceFailed;

                var solution = workspace.OpenSolutionAsync(solutionFile).Result;

                var node = methodAnalyzer.Analyze(solution, fullNameMethod);

                Assert.IsTrue(true);
            }
            catch
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void SpanAnalyzeTest()
        {
            try
            {
                var solutionPath = @"C:\Test\Mobiik.Popcorn\Mobiik.Popcorn.sln";

                var filePath = @"/Mobiik.Popcorn/Infrastructure/Mobiik.Formiik.DataBasePersistence/GeofencePersistance.cs";

                var lineCode = 43;

                var workspace = MSBuildWorkspace.Create();

                workspace.WorkspaceFailed += Workspace_WorkspaceFailed;

                var solution = workspace.OpenSolutionAsync(solutionPath).Result;

                var method = methodAnalyzer.GetMethodFromLineCode(solution, filePath, lineCode);

                Assert.IsTrue(true);
            }
            catch
            {
                Assert.Fail();
            }
        }

        private void Workspace_WorkspaceFailed(object sender, WorkspaceDiagnosticEventArgs e)
        {
            Debug.WriteLine(e.Diagnostic.Message);
        }
    }
}
