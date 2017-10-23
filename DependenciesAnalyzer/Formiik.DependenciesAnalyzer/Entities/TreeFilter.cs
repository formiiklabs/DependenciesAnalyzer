using System.Collections.Generic;

namespace Formiik.DependenciesAnalyzer.Entities
{
    public enum TreeFilterActionEnum
    {
        Added,
        Modified
    }

    public class TreeFilter
    {
        #region Constructor
        public TreeFilter()
        {
            this.Methods = new List<string>();
        }
        #endregion

        #region Properties
        public TreeFilterActionEnum TreeFilterAction { get; set; }

        public List<string> Methods { get; set; }

        public string RepoPath { get; set; }
        #endregion
    }
}
