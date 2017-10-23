using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Formiik.DependenciesAnalyzer.Core
{
    public class UtilsGit
    {
        public static string CanonizeGitPath(string systemPath)
        {
            if ((systemPath.IndexOf(":") == 1) || (systemPath.IndexOf("\\\\") == 0))
            {
                systemPath = systemPath.Substring(2);
            }
            if (systemPath.IndexOf("\\") > 0)
            {
                systemPath = systemPath.Substring(systemPath.IndexOf("\\"));
            }

            return systemPath.Replace(":\\", "/").Replace("\\", "/").Replace("//", "/");
        }
    }
}
