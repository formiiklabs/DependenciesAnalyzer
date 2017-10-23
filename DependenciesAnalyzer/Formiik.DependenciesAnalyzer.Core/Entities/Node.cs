using System.Collections.Generic;

namespace Formiik.DependenciesAnalyzer.Core.Entities
{
    /// <summary>
    /// Node entity for the construction of the invoke methods tree
    /// </summary>
    public class Node
    {
        #region Constructor
        public Node()
        {
            this.Nodes = new List<Node>();
            this.ModulesAffected = new List<string>();
        }
        #endregion

        #region Properties
        public string MethodName { get; set; }

        public List<Node> Nodes { get; set; }

        public List<string> ModulesAffected { get; set; }
        #endregion
    }
}
