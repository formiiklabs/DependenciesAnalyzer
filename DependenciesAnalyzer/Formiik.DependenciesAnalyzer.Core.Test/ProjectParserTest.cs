using Microsoft.VisualStudio.TestTools.UnitTesting;
using Formiik.DependenciesAnalyzer.Core.Parser;

namespace Formiik.DependenciesAnalyzer.Test
{
    [TestClass]
    public class ProjectParserTest
    {
        [TestMethod]
        public void PopulateTest()
        {
            var fileName = @"C:\CopyAssignBusiness.cs";

            try
            {
                string method = string.Empty;

                using (var parserManager = new ParserManager())
                {
                    method = parserManager.GetCodeMethodByLineNumber(fileName, 65);
                }

                if (string.IsNullOrEmpty(method))
                {
                    Assert.Fail();
                }
                else
                {
                    Assert.IsTrue(true);
                }
            }
            catch 
            {
                Assert.Fail();
            }
        }
    }
}
