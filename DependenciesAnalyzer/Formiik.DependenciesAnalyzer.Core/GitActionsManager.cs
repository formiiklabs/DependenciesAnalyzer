using Formiik.DependenciesAnalyzer.Core.Entities;
using Formiik.DependenciesAnalyzer.Core.Parser;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
                foreach (var branch in repo.Branches)
                {
                    branches.Add(branch);
                }
            }

            return branches;
        }

        public void CloneRepository(string user, string password, string repoPath, string remoteRepo)
        {
            var cloneOptions = new CloneOptions();

            cloneOptions.CredentialsProvider = (_url, _user, _cred) =>
            new UsernamePasswordCredentials
            {
                Username = user,
                Password = password
            };

            var localRepoPath = UtilsGit.CanonizeGitPath(repoPath);

            Repository.Clone(remoteRepo, localRepoPath, cloneOptions);
        }

        public void FetchAndCheckout(string pathRepo, string branchName)
        {
            var onlyNameBranch = branchName.Replace("origin/", "");

            using (var repo = new Repository(pathRepo))
            {
                Branch remoteBranch = repo.Branches[branchName];

                if (remoteBranch == null)
                {
                    return;
                }

                try
                {
                    Branch newLocalBranch = repo.CreateBranch(onlyNameBranch, remoteBranch.Tip);

                    repo.Branches.Update(newLocalBranch,
                        b => b.TrackedBranch = remoteBranch.CanonicalName);

                    Branch trackingBranch = repo.Branches[onlyNameBranch];

                    trackingBranch = Commands.Checkout(repo, trackingBranch);
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
                PullOptions options = new PullOptions();

                options.FetchOptions = new FetchOptions();

                options.FetchOptions.CredentialsProvider = new CredentialsHandler(
                    (url, usernameFromUrl, types) =>
                        new UsernamePasswordCredentials()
                        {
                            Username = username,
                            Password = password
                        });

                var signature = new Signature(username, email, new DateTimeOffset(DateTime.Now));

                Commands.Pull(repo, signature, options);
            }
        }

        public void DiffHead(string pathRepo)
        {
            using (var repo = new Repository(pathRepo))
            {
                foreach (TreeEntryChanges c in repo.Diff.Compare<TreeChanges>(repo.Head.Tip.Tree, DiffTargets.Index | DiffTargets.WorkingDirectory))
                {
                    Debug.WriteLine(c);
                }
            }
        }

        public List<string> GetFilesChanged(string pathRepo, string branchName)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("git.exe");

            startInfo.WindowStyle = ProcessWindowStyle.Normal;

            startInfo.UseShellExecute = false;

            startInfo.WorkingDirectory = pathRepo;

            startInfo.RedirectStandardInput = true;

            startInfo.RedirectStandardOutput = true;

            startInfo.Arguments = string.Format("checkout {0}", branchName);

            Process process = new Process();

            process.StartInfo = startInfo;

            process.Start();

            startInfo.Arguments = string.Format("diff --name-status origin/master...{0}", branchName);

            process.Start();

            var output = new List<string>();

            string lineValue = process.StandardOutput.ReadLine();

            while (lineValue != null)
            {
                output.Add(lineValue);

                lineValue = process.StandardOutput.ReadLine();
            }

            int val = output.Count();

            process.WaitForExit();

            return output;
        }

        public List<string> GetListMethodsOfFile(string pathRepo, Solution solution, string pathFileRelative)
        {
            var result = new List<string>();

            var fullPathFile = string.Format("{0}{1}", pathRepo, pathFileRelative.Replace('/', '\\'));

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
            List<Component> components = null;

            using (var methodAnalyzer = new MethodAnalyzer())
            {
                components = methodAnalyzer.GetComponents(solution);
            }

            return components;
        }

        public List<string> GetListMethodsOfFileModified(string pathRepo, string branchName, Solution solution, string pathFileRelative)
        {
            var fullPathFile = string.Format("{0}{1}", pathRepo, pathFileRelative.Replace('/', '\\')); 

            var result = new List<string>();

            ProcessStartInfo startInfo = new ProcessStartInfo("git.exe");

            startInfo.WindowStyle = ProcessWindowStyle.Normal;

            startInfo.UseShellExecute = false;

            startInfo.WorkingDirectory = pathRepo;

            startInfo.RedirectStandardInput = true;

            startInfo.RedirectStandardOutput = true;

            startInfo.Arguments = string.Format("checkout {0}", branchName);

            Process process = new Process();

            process.StartInfo = startInfo;

            process.Start();

            pathFileRelative = pathFileRelative.Replace("\\", "/");

            startInfo.Arguments = string.Format("diff {0}:.{1} origin/master:.{2}", branchName, pathFileRelative, pathFileRelative);

            process.Start();

            var output = new List<string>();

            string lineValue = process.StandardOutput.ReadLine();

            while (lineValue != null)
            {
                if (lineValue.Contains("@@"))
                {
                    var tokens = lineValue.Split(',');

                    if (tokens.Any())
                    {
                        var numberString = tokens.First().Remove(0, 4).Trim();

                        int numberLine = 0;

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

            int val = output.Count();

            process.WaitForExit();

            return result;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Private Methods
        private void Workspace_WorkspaceFailed(object sender, WorkspaceDiagnosticEventArgs e)
        {
            Debug.WriteLine(e.Diagnostic.Message);
        }
        #endregion
    }
}
