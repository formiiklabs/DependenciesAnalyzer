using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Formiik.DependenciesAnalyzer.Core.Entities;
using LibGit2Sharp;
using Microsoft.CodeAnalysis;

namespace Formiik.DependenciesAnalyzer.Core
{
    public class GitActionsManager : IDisposable
    {
        #region Public Methods
        public List<Branch> GetBranches(string pathRepo)
        {
            var branches = new List<Branch>();

            using (var repo = new Repository(pathRepo))
            {
                branches.AddRange(repo.Branches);
            }

            return branches;
        }

        public void CloneRepository(string user, string password, string repoPath, string remoteRepo)
        {
            var cloneOptions = new CloneOptions
            {
                CredentialsProvider = delegate
                {
                    return new UsernamePasswordCredentials
                    {
                        Username = user,
                        Password = password
                    };
                }
            };


            var localRepoPath = UtilsGit.CanonizeGitPath(repoPath);

            Repository.Clone(remoteRepo, localRepoPath, cloneOptions);
        }

        public void FetchAndCheckout(string pathRepo, string branchName)
        {
            var onlyNameBranch = branchName.Replace("origin/", "");

            using (var repo = new Repository(pathRepo))
            {
                var remoteBranch = repo.Branches[branchName];

                if (remoteBranch == null)
                {
                    return;
                }

                try
                {
                    var newLocalBranch = repo.CreateBranch(onlyNameBranch, remoteBranch.Tip);

                    repo.Branches.Update(newLocalBranch,
                        b => b.TrackedBranch = remoteBranch.CanonicalName);

                    var trackingBranch = repo.Branches[onlyNameBranch];

                    Commands.Checkout(repo, trackingBranch);
                }
                catch (NameConflictException)
                {
                    Commands.Checkout(repo, remoteBranch);
                }
            }
        }

        public void PullChanges(string pathRepo, string username, string password, string email)
        {
            using (var repo = new Repository(pathRepo))
            {
                var options = new PullOptions
                {
                    FetchOptions = new FetchOptions
                    {
                        CredentialsProvider = (url, usernameFromUrl, types) =>
                            new UsernamePasswordCredentials()
                            {
                                Username = username,
                                Password = password
                            }
                    }
                };

                var signature = new Signature(username, email, new DateTimeOffset(DateTime.Now));

                Commands.Pull(repo, signature, options);
            }
        }

        public void DiffHead(string pathRepo)
        {
            using (var repo = new Repository(pathRepo))
            {
                foreach (var c in repo.Diff.Compare<TreeChanges>(repo.Head.Tip.Tree, DiffTargets.Index | DiffTargets.WorkingDirectory))
                {
                    Debug.WriteLine(c);
                }
            }
        }

        public List<string> GetFilesChanged(string pathRepo, string branchName)
        {
            var startInfo = new ProcessStartInfo("git.exe")
            {
                WindowStyle = ProcessWindowStyle.Normal,
                UseShellExecute = false,
                WorkingDirectory = pathRepo,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                Arguments = $"checkout {branchName}"
            };

            var process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();

            // ReSharper disable once UseStringInterpolation
            startInfo.Arguments = string.Format("diff --name-status origin/master...{0}", branchName);

            process.Start();

            var output = new List<string>();

            var lineValue = process.StandardOutput.ReadLine();

            while (lineValue != null)
            {
                output.Add(lineValue);

                lineValue = process.StandardOutput.ReadLine();
            }

            process.WaitForExit();

            return output;
        }

        public List<string> GetListMethodsOfFile(string pathRepo, Solution solution, string pathFileRelative)
        {
            var result = new List<string>();

            // ReSharper disable once UseStringInterpolation

            using (var methodAnalyzer = new MethodAnalyzer())
            {
                var methods = methodAnalyzer.GetMethods(solution, pathFileRelative);

                if (methods.Any())
                {
                    methods.ForEach(m =>
                    {
                        if (!result.Any(x => x.Equals(m)))
                        {
                            result.Add(m);
                        }
                    });
                }
            }

            return result;
        }

        public List<Component> GetComponents(Solution solution)
        {
            List<Component> components;

            using (var methodAnalyzer = new MethodAnalyzer())
            {
                components = methodAnalyzer.GetComponents(solution);
            }

            return components;
        }

        public List<string> GetListMethodsOfFileModified(string pathRepo, string branchName, Solution solution, string pathFileRelative)
        {
            var result = new List<string>();

            var startInfo = new ProcessStartInfo("git.exe")
            {
                WindowStyle = ProcessWindowStyle.Normal,
                UseShellExecute = false,
                WorkingDirectory = pathRepo,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                // ReSharper disable once UseStringInterpolation
                Arguments = string.Format("checkout {0}", branchName)
            };

            var process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();

            pathFileRelative = pathFileRelative.Replace("\\", "/");

            // ReSharper disable once UseStringInterpolation
            startInfo.Arguments = string.Format("diff {0}:.{1} origin/master:.{2}", branchName, pathFileRelative, pathFileRelative);

            process.Start();

            // ReSharper disable once CollectionNeverQueried.Local
            var output = new List<string>();

            var lineValue = process.StandardOutput.ReadLine();

            while (lineValue != null)
            {
                if (lineValue.Contains("@@"))
                {
                    var tokens = lineValue.Split(',');

                    if (tokens.Any())
                    {
                        var numberString = tokens.First().Remove(0, 4).Trim();

                        int numberLine;

                        if (int.TryParse(numberString, out numberLine))
                        {
                            using (var methodAnalyzer = new MethodAnalyzer())
                            {
                                var method = methodAnalyzer.GetMethodFromLineCode(solution, pathFileRelative, numberLine);

                                if (!result.Any(x => x.Equals(method)) && !string.IsNullOrEmpty(method.Trim()))
                                {
                                    result.Add(method);
                                }
                            }
                        }
                    }
                }

                output.Add(lineValue);

                lineValue = process.StandardOutput.ReadLine();
            }

            process.WaitForExit();

            return result;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
