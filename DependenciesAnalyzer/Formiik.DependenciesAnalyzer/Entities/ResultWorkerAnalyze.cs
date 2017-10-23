using Formiik.DependenciesAnalyzer.Core.Entities;
using System.Collections.Generic;

namespace Formiik.DependenciesAnalyzer.Entities
{
    public class ResultWorkerAnalyze
    {
        #region Constructor
        public ResultWorkerAnalyze()
        {
            this.FileSet = new FilesSet();
            this.TreeNodes = new List<TreeNode>();
            this.ModulesAffected = new List<string>();
        }
        #endregion

        #region Properties
        public FilesSet FileSet { get; set; }

        public List<TreeNode> TreeNodes { get; set; }

        public List<string> ModulesAffected { get; set; }
        #endregion
    }
}
