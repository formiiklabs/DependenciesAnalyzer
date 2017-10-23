using System.Collections.Generic;

namespace Formiik.DependenciesAnalyzer.Core.Entities
{
    public class TreeNode
    {
        #region Constructors
        public TreeNode(string value)
        {
            this.Value = value;
            this.Nodes = new List<TreeNode>();
        }

        public TreeNode(string value, StatesFileGitEnum stateFileGit, string path)
        {
            this.Value = value;
            this.Nodes = new List<TreeNode>();
            this.StateFileGit = stateFileGit;
            this.Path = path;
        }
        #endregion

        #region Properties
        public string Value { get; set; }

        public List<TreeNode> Nodes { get; set; }

        public StatesFileGitEnum StateFileGit { get; set; }

        public string Path { get; set; }
        #endregion

        #region Public Methods
        public void AddItem(TreeNode node)
        {
            this.Nodes.Add(node);
        }
        #endregion
    }
}
