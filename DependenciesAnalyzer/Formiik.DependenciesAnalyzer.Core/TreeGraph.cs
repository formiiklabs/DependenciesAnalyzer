using Formiik.DependenciesAnalyzer.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Formiik.DependenciesAnalyzer.Core
{
    public class TreeGraph : IDisposable
    {
        #region Constructor
        public TreeGraph()
        {
            this.GitActionsManager = new GitActionsManager();
            this.TreeNodes = new List<TreeNode>();
        }
        #endregion

        #region Properties 
        public GitActionsManager GitActionsManager { get; set; }

        public List<TreeNode> TreeNodes { get; set; }
        #endregion

        #region Public Methods
        public FilesSet Build(string repoPath, string branch)
        {
            var fileSet = new FilesSet();

            var filesGit = new List<FileGit>();

            var filesChanged = this.GitActionsManager.GetFilesChanged(repoPath, branch);

            if (filesChanged.Any())
            {
                filesChanged.ForEach(fc =>
                {
                    FileGit fileGit = null;

                    var splitTab = fc.Split('\t');

                    switch (splitTab[0])
                    {
                        case "R":
                            fileGit = new FileGit
                            {
                                StateFileGit = StatesFileGitEnum.Renamed
                            };
                            fileSet.Renamed.Add(splitTab[1]);
                            break;
                        case "M":
                            fileGit = new FileGit
                            {
                                StateFileGit = StatesFileGitEnum.Modified
                            };
                            fileSet.Modified.Add(splitTab[1]);
                            break;
                        case "A":
                            fileGit = new FileGit
                            {
                                StateFileGit = StatesFileGitEnum.Added
                            };
                            fileSet.Added.Add(splitTab[1]);
                            break;
                        case "D":
                            fileGit = new FileGit
                            {
                                StateFileGit = StatesFileGitEnum.Deleted
                            };
                            fileSet.Deleted.Add(splitTab[1]);
                            break;
                        case "C":
                            fileGit = new FileGit
                            {
                                StateFileGit = StatesFileGitEnum.Copied
                            };
                            fileSet.Copied.Add(splitTab[1]);
                            break;
                        case "U":
                            fileGit = new FileGit
                            {
                                StateFileGit = StatesFileGitEnum.UpdateButUnmerged
                            };
                            fileSet.UpdateButUnmerged.Add(splitTab[1]);
                            break;
                        default:
                            fileGit = new FileGit
                            {
                                StateFileGit = StatesFileGitEnum.Modified
                            };
                            fileSet.Modified.Add(splitTab[1]);
                            break;
                    };

                    if (fileGit != null)
                    {
                        fileGit.PathFile = splitTab[1];

                        filesGit.Add(fileGit);
                    }
                });
            }

            if (filesGit.Any())
            {
                filesGit.ForEach(f =>
                {
                    this.CreatePath(this.TreeNodes, f.PathFile, f.StateFileGit);
                });
            }

            return fileSet;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Private Methods
        private void CreatePath(List<TreeNode> treeNodes, string path, StatesFileGitEnum stateFileGit)
        {
            TreeNode node = null;

            string folder = string.Empty;

            int p = path.IndexOf('/');

            if (p == -1)
            {
                folder = path;
                path = string.Empty;
            }
            else
            {
                folder = path.Substring(0, p);

                path = path.Substring(p + 1, path.Length - (p + 1));
            }

            node = null;

            foreach (TreeNode item in treeNodes)
            {
                if (item.Value == folder)
                {
                    node = item;
                }
            }

            if (node == null)
            {
                node = new TreeNode(folder, stateFileGit, path);

                treeNodes.Add(node);
            }

            if (path != "")
            {
                CreatePath(node.Nodes, path, stateFileGit);
            }
        }
        #endregion
    }
}
