using System.Windows;

namespace Formiik.DependenciesAnalyzer
{
    #region Delegates
    public delegate void RemoteRepoInfoDelegate(string remoteRepo, string user, string password, string pathGit);
    #endregion

    public partial class App : Application
    {
    }
}
