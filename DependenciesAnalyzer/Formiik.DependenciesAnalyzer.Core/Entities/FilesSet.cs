using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Formiik.DependenciesAnalyzer.Core.Entities
{
    public class FilesSet
    {
        #region Constructor
        public FilesSet()
        {
            this.Renamed = new List<string>();
            this.Modified = new List<string>();
            this.Added = new List<string>();
            this.Deleted = new List<string>();
            this.Copied = new List<string>();
            this.UpdateButUnmerged = new List<string>();
        }
        #endregion

        #region Properties
        public List<string> Renamed { get; set; }

        public List<string> Modified { get; set; }

        public List<string> Added { get; set; }

        public List<string> Deleted { get; set; }

        public List<string> Copied { get; set; }

        public List<string> UpdateButUnmerged { get; set; }
        #endregion
    }
}