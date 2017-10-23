using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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
                var solutionPath = @"C:\Test\Mobiik.Popcorn\Mobiik.Popcorn.sln";

                //var fullNameMethod = "Mobiik.Formiik.DataBasePersistence.Fake.PersistanceFake.GetDescriptionsFake()";

                var fullNameMethod = @"Mobiik.Formiik.DataBasePersistence.GeofencePersistance.GetGeofencesByGroup()";

                var node = methodAnalyzer.Analyze(solutionPath, fullNameMethod);

                Assert.IsTrue(true);
            }
            catch
            {
                Assert.Fail();
            }
        }
    }
}
