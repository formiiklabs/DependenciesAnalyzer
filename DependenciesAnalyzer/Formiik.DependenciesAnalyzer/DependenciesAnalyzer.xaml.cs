using Formiik.DependenciesAnalyzer.Core;
using Formiik.DependenciesAnalyzer.Core.Entities;
using Formiik.DependenciesAnalyzer.Entities;
using LibGit2Sharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.WpfGraphControl;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;

namespace Formiik.DependenciesAnalyzer
{
    public partial class DependenciesAnalyzer : Window
    {
        #region Members
        private RemoteRepo remoteRepo = null;
        private List<Branch> remoteBranches = null;
        private ObservableCollection<ComboBoxItem> itemsRemoteBranches = null;
        #endregion

        public DependenciesAnalyzer()
        {
            InitializeComponent();

            this.itemsRemoteBranches = new ObservableCollection<ComboBoxItem>();

            this.lblPathRepo.Content =
                UtilsGit.CanonizeGitPath(Properties.Settings.Default.RepoPath);

            this.lblRemoteRepoUrl.Text =
                string.IsNullOrEmpty(Properties.Settings.Default.RemoteRepoUrl) ? "(Sin Información)" : Properties.Settings.Default.RemoteRepoUrl;

            this.lblUserRemoteRepo.Text =
                string.IsNullOrEmpty(Properties.Settings.Default.UserRemoteRepo) ? "(Sin Información)" : Properties.Settings.Default.UserRemoteRepo;

            this.GetRemoteBranches();
        }

        private void BtnSeleccionarRepo_Click(object sender, RoutedEventArgs e)
        {
            this.SeleccionarRepoLocal();
        }

        private void BtnSetRepository_Click(object sender, RoutedEventArgs e)
        {
            if (this.remoteRepo == null)
            {
                this.remoteRepo = new RemoteRepo();

                this.remoteRepo.RemoteRepoInfoEvent += RemoteRepo_RemoteRepoInfoEvent;

                this.remoteRepo.Closed += (a, b) => this.remoteRepo = null;

                this.remoteRepo.Show();

                this.remoteRepo.Focus();
            }
            else
            {
                this.remoteRepo.Show();

                this.remoteRepo.Focus();
            }
        }

        private void RemoteRepo_RemoteRepoInfoEvent(string remoteRepo, string user, string password)
        {
            while (string.IsNullOrEmpty(Properties.Settings.Default.RepoPath))
            {
                System.Windows.MessageBox.Show(
                    "No ha seleccionado una ruta de repositorio local.",
                    "Información",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);

                this.SeleccionarRepoLocal();
            }

            try
            {
                using (var gitActionsManager = new GitActionsManager())
                {
                    gitActionsManager.CloneRepository(user, password, Properties.Settings.Default.RepoPath, remoteRepo);
                }

                this.SetRepository(remoteRepo, user, password);

                System.Windows.MessageBox.Show(
                    "Repositorio local configurado correctamente",
                    "Información",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("exists and is not an empty directory"))
                {
                    this.SetRepository(remoteRepo, user, password);
                }
                else
                {
                    Debug.WriteLine(ex.Message);

                    System.Windows.MessageBox.Show(
                        "Ocurrió un error al clonar el repositorio.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void SetRepository(string urlRemoteRepo, string user, string password)
        {
            Properties.Settings.Default.RemoteRepoUrl = urlRemoteRepo;

            Properties.Settings.Default.UserRemoteRepo = user;

            Properties.Settings.Default.PasswordRemoteRepo = password;

            Properties.Settings.Default.Save();

            this.lblRemoteRepoUrl.Text = urlRemoteRepo;

            this.lblUserRemoteRepo.Text = user;

            this.GetRemoteBranches();

            System.Windows.MessageBox.Show(
                "Repositorio local configurado correctamente",
                "Información",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            System.Windows.Application.Current.Shutdown();
        }

        private void SeleccionarRepoLocal()
        {
            var folderPath = string.Empty;

            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();

            folderBrowser.Description = "Seleccione la ruta del repositorio Git";

            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                folderPath = folderBrowser.SelectedPath;

                Properties.Settings.Default.RepoPath = folderPath;

                Properties.Settings.Default.Save();

                this.lblPathRepo.Content = UtilsGit.CanonizeGitPath(folderPath);
            }
        }

        private void GetRemoteBranches()
        {
            try
            {
                using (var gitActions = new GitActionsManager())
                {
                    this.remoteBranches = gitActions.GetBranches(Properties.Settings.Default.RepoPath).Where(x => x.IsRemote).ToList();

                    this.remoteBranches.ForEach(rb =>
                    {
                        this.itemsRemoteBranches.Add(new ComboBoxItem
                        {
                            Content = rb.FriendlyName
                        });
                    });

                    this.cmbRemoteBranches.ItemsSource = this.itemsRemoteBranches;
                }
            }
            catch (RepositoryNotFoundException)
            {
                System.Windows.MessageBox.Show(
                    string.Format("La ruta {0} no es un repositorio válido de trabajo Git.", Properties.Settings.Default.RepoPath),
                    "Información",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);

                System.Windows.MessageBox.Show(
                    "Ocurrió un error al obtener los remote branches.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void BtnFetchCheckout_Click(object sender, RoutedEventArgs e)
        {
            var selectedBranch = string.Empty;

            try
            {
                selectedBranch = 
                    string.IsNullOrEmpty(Properties.Settings.Default.SelectedBranch) ? 
                    this.cmbRemoteBranches.Text : 
                    Properties.Settings.Default.SelectedBranch;

                using (var gitActions = new GitActionsManager())
                {
                    gitActions.FetchAndCheckout(Properties.Settings.Default.RepoPath, selectedBranch);
                }

                System.Windows.MessageBox.Show(
                    "Se ha hecho checkout exitosamente al branch.",
                    "Información",
                    MessageBoxButton.OK,
                    MessageBoxImage.None);
            }
            catch (NameConflictException)
            {
                System.Windows.MessageBox.Show(
                    string.Format("El brach seleccionado {0} ya está seleccionado.", selectedBranch),
                    "Información",
                    MessageBoxButton.OK,
                    MessageBoxImage.Asterisk);
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show(
                    "Ocurrió un error al hacer checkout al branch.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void BtnPull_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var gitActions = new GitActionsManager())
                {
                    gitActions.PullChanges(
                        Properties.Settings.Default.RepoPath,
                        Properties.Settings.Default.UserRemoteRepo,
                        Properties.Settings.Default.PasswordRemoteRepo,
                        Properties.Settings.Default.UserRemoteRepo);

                    System.Windows.MessageBox.Show(
                        "Se ha hecho pull exitosamente a los cambios.",
                        "Información",
                        MessageBoxButton.OK,
                        MessageBoxImage.None);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("There is no tracking information for the current branch."))
                {
                    System.Windows.MessageBox.Show(
                        "Se ha hecho pull exitosamente a los cambios.",
                        "Información",
                        MessageBoxButton.OK,
                        MessageBoxImage.None);
                }
                else
                {
                    Debug.WriteLine(ex.Message);

                    System.Windows.MessageBox.Show(
                        "Ocurrió un error al hacer pull al branch seleccionado.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void BtnAnalyze_Click(object sender, RoutedEventArgs e)
        {
            this.stackModules.Children.Clear();

            this.treeViewRoot.Items.Clear();

            var selectedBranch = string.Empty;

            try
            {
                this.lblArchivosModificados.Content = string.Empty;

                this.txtRenamed.Text = string.Empty;

                this.txtModified.Text = string.Empty;

                this.txtAdded.Text = string.Empty;

                this.txtDeleted.Text = string.Empty;

                this.txtCopied.Text = string.Empty;

                this.txtUpdateButUnmerged.Text = string.Empty;

                var backgroundWorker = new BackgroundWorker();

                backgroundWorker.DoWork += BackgroundWorker_DoWork;

                backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;

                var remoteBranch = string.IsNullOrEmpty(Properties.Settings.Default.SelectedBranch) ?
                    this.cmbRemoteBranches.Text :
                    Properties.Settings.Default.SelectedBranch;

                selectedBranch = remoteBranch.Remove(0, 7);

                var data = new DataWorkerAnalyze
                {
                    RepoPath = Properties.Settings.Default.RepoPath,

                    SelectedBranch = selectedBranch
                };

                this.btnAnalyze.IsEnabled = false;

                this.btnExportarTextoAfectados.Visibility = Visibility.Collapsed;

                this.btnExportarTextoComponentes.Visibility = Visibility.Collapsed;

                this.pgbIndeterminate.IsIndeterminate = true;

                backgroundWorker.RunWorkerAsync(data);
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show(
                    "Ocurrió un error al intentar hacer el análisis del branch seleccionado.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
            }
            else if (e.Cancelled)
            {
            }
            else
            {
                var result = (ResultWorkerAnalyze)e.Result;

                if (result.TreeNodes.Any())
                {
                    result.TreeNodes.ForEach(n =>
                    {
                        var treeViewItem = new TreeViewItem();

                        treeViewItem.Selected += TreeViewItem_Selected;

                        treeViewItem.IsExpanded = true;

                        treeViewItem.Header = n.Value;

                        treeViewRoot.Items.Add(treeViewItem);

                        this.CreateNodeTreeView(n, treeViewItem);
                    });
                }

                if (result.ModulesAffected.Any())
                {
                    this.lblModulosAfectados.Content = "Módulos Afectados:";

                    this.btnExportarTextoAfectados.Visibility = Visibility.Visible;

                    result.ModulesAffected.ForEach(moduleAffected =>
                    {
                        TextBlock moduleAffectedBlock = new TextBlock();

                        Thickness margin = moduleAffectedBlock.Margin;

                        margin.Left = 5;

                        margin.Top = 5;

                        margin.Right = 5;

                        margin.Bottom = 5;

                        moduleAffectedBlock.Margin = margin;

                        moduleAffectedBlock.Text = string.Format("{0}-{1}", moduleAffected.Description, moduleAffected.Action);

                        this.stackModules.Children.Add(moduleAffectedBlock);
                    });
                }

                this.lblArchivosModificados.Content = "Archivos Modificados";

                this.txtRenamed.Text = string.Format("Renamed: {0}", result.FileSet.Renamed.Count);

                this.txtModified.Text = string.Format("Modified: {0}", result.FileSet.Modified.Count);

                this.txtAdded.Text = string.Format("Added: {0}", result.FileSet.Added.Count);

                this.txtDeleted.Text = string.Format("Deleted: {0}", result.FileSet.Deleted.Count);

                this.txtCopied.Text = string.Format("Copied: {0}", result.FileSet.Copied.Count);

                this.txtUpdateButUnmerged.Text = string.Format("Update But Unmerged: {0}", result.FileSet.UpdateButUnmerged.Count);

                this.pgbIndeterminate.IsIndeterminate = false;
            }

            this.btnAnalyze.IsEnabled = true;
        }

        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = e.OriginalSource as TreeViewItem;

            if (item != null)
            {
                if (item.Header.ToString().EndsWith(".cs (Added)") || item.Header.ToString().EndsWith(".cs (Modified)"))
                {
                    var isAdded = false;

                    var isModified = false;

                    if (item.Header.ToString().EndsWith("(Added)"))
                    {
                        isAdded = true;

                        isModified = !isAdded;
                    }

                    if (item.Header.ToString().EndsWith(".cs (Modified)"))
                    {
                        isAdded = false;

                        isModified = !isAdded;
                    }

                    var path = string.Empty;

                    ItemsControl parent = null;

                    do
                    {
                        parent = GetSelectedTreeViewItemParent(item);

                        if (parent != null)
                        {
                            TreeViewItem treeItem = parent as TreeViewItem;

                            if (treeItem == null)
                            {
                                break;
                            }

                            string val = treeItem.Header.ToString();

                            path = "\\" + val + path;

                            item = treeItem;
                        }
                    }
                    while (parent != null);

                    if (!string.IsNullOrEmpty(path))
                    {
                        var backgroundWorkerTree = new BackgroundWorker();

                        backgroundWorkerTree.DoWork += BackgroundWorkerTree_DoWork;

                        backgroundWorkerTree.RunWorkerCompleted += BackgroundWorkerTree_RunWorkerCompleted;

                        string[] solutionFiles = Directory.GetFiles(
                            Properties.Settings.Default.RepoPath, 
                            "*.sln", 
                            SearchOption.AllDirectories);

                        if (solutionFiles.Length > 0)
                        {
                            var solutionFile = solutionFiles[0];

                            if (isAdded)
                            {
                                var methodsAdded = new List<string>();

                                using (var gitActionsManager = new GitActionsManager())
                                {
                                    var workspace = MSBuildWorkspace.Create();

                                    workspace.WorkspaceFailed += Workspace_WorkspaceFailed;

                                    var solution = workspace.OpenSolutionAsync(solutionFile).Result;

                                    var result = gitActionsManager.GetListMethodsOfFile(
                                        Properties.Settings.Default.RepoPath,
                                        solution,
                                        path);

                                    workspace.CloseSolution();

                                    if (result.Any())
                                    {
                                        result.ForEach(r =>
                                        {
                                            if (!methodsAdded.Any(x => x.Equals(r)))
                                            {
                                                methodsAdded.Add(r);
                                            }
                                        });

                                        var data = new TreeFilter
                                        {
                                            Methods = methodsAdded,
                                            RepoPath = Properties.Settings.Default.RepoPath,
                                            TreeFilterAction = TreeFilterActionEnum.Added
                                        };

                                        this.treeViewRoot.IsEnabled = false;

                                        this.pgbIndeterminate.IsIndeterminate = true;

                                        backgroundWorkerTree.RunWorkerAsync(data);
                                    }
                                }
                            }
                            else if (isModified)
                            {
                                var methodsModified = new List<string>();

                                using (var gitActionsManager = new GitActionsManager())
                                {
                                    var remoteBranch = string.IsNullOrEmpty(Properties.Settings.Default.SelectedBranch) ?
                                        this.cmbRemoteBranches.Text :
                                        Properties.Settings.Default.SelectedBranch;

                                    var selectedBranch = remoteBranch.Remove(0, 7);

                                    var workspace = MSBuildWorkspace.Create();

                                    workspace.WorkspaceFailed += Workspace_WorkspaceFailed;

                                    var solution = workspace.OpenSolutionAsync(solutionFile).Result;

                                    var result = gitActionsManager.GetListMethodsOfFileModified(
                                        Properties.Settings.Default.RepoPath,
                                        selectedBranch, 
                                        solution,
                                        path);

                                    workspace.CloseSolution();

                                    if (result.Any())
                                    {
                                        result.ForEach(r =>
                                        {
                                            if (!methodsModified.Any(x => x.Equals(r)))
                                            {
                                                methodsModified.Add(r);
                                            }
                                        });

                                        var data = new TreeFilter
                                        {
                                            Methods = methodsModified,
                                            RepoPath = Properties.Settings.Default.RepoPath,
                                            TreeFilterAction = TreeFilterActionEnum.Modified
                                        };

                                        this.treeViewRoot.IsEnabled = false;

                                        this.pgbIndeterminate.IsIndeterminate = true;

                                        backgroundWorkerTree.RunWorkerAsync(data);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void BackgroundWorkerTree_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
            }
            else if (e.Cancelled)
            {
            }
            else
            {
                this.pgbIndeterminate.IsIndeterminate = false;

                var result = (List<Core.Entities.Node>)e.Result;

                if (result.Any())
                {
                    DockPanel graphViewerPanel = new DockPanel();

                    GraphViewer graphViewer = new GraphViewer();

                    graphViewerPanel.ClipToBounds = true;

                    this.mainGrid.Children.Add(graphViewerPanel);

                    graphViewer.BindToPanel(graphViewerPanel);

                    Graph graph = new Graph();

                    foreach (var node in result)
                    {
                        this.DrawGraph(node, ref graph);
                    }

                    graph.Attr.LayerDirection = LayerDirection.TB;

                    graphViewer.Graph = graph;

                    this.treeViewRoot.IsEnabled = true;
                }
            }
        }

        private void DrawGraph(Core.Entities.Node node, ref Graph graph)
        {
            if (node.Nodes.Any())
            {
                foreach (var child in node.Nodes)
                {
                    graph.AddEdge(node.MethodName, child.MethodName);

                    foreach (var otherChild in node.Nodes)
                    {
                        this.DrawGraph(otherChild, ref graph);
                    }
                }
            }
        }

        private void BackgroundWorkerTree_DoWork(object sender, DoWorkEventArgs e)
        {
            var result = new List<Core.Entities.Node>();

            var treeFilter = e.Argument as TreeFilter;

            if (treeFilter != null)
            {
                if (treeFilter.Methods.Any())
                {
                    string[] solutionFiles = Directory.GetFiles(
                        treeFilter.RepoPath,
                        "*.sln",
                        SearchOption.AllDirectories);

                    if (solutionFiles.Length > 0)
                    {
                        var solutionFile = solutionFiles[0];

                        using (var methodAnalyzer = new MethodAnalyzer())
                        {
                            var workspace = MSBuildWorkspace.Create();

                            workspace.WorkspaceFailed += Workspace_WorkspaceFailed;

                            var solution = workspace.OpenSolutionAsync(solutionFile).Result;

                            foreach (var mo in treeFilter.Methods)
                            {
                                var node = methodAnalyzer.Analyze(solution, mo);

                                if (node != null)
                                {
                                    result.Add(node);
                                }
                            }

                            workspace.CloseSolution();
                        }
                    }
                }
            }

            e.Result = result;
        }

        private ItemsControl GetSelectedTreeViewItemParent(TreeViewItem item)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(item);

            while (!(parent is TreeViewItem || parent is System.Windows.Controls.TreeView))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as ItemsControl;
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var arguments = (DataWorkerAnalyze)e.Argument;

            var modulesAffectedFinally = new List<Core.Entities.Component>();

            string[] solutionFiles = Directory.GetFiles(
                            arguments.RepoPath,
                            "*.sln",
                            SearchOption.AllDirectories);

            if (solutionFiles.Length > 0)
            {
                var solutionFile = solutionFiles[0];

                using (var treeGraph = new TreeGraph())
                {
                    var fileSet = treeGraph.Build(arguments.RepoPath, arguments.SelectedBranch);

                    var treeNodes = treeGraph.TreeNodes;
                    
                    if (fileSet != null)
                    {
                        var methodsObserved = new List<string>();
                        
                        if (fileSet.Modified.Any())
                        {
                            fileSet.Modified.ForEach(file =>
                            {
                                if (!file.StartsWith("/"))
                                {
                                    file = string.Format("/{0}", file);
                                }

                                if (file.EndsWith(".cs"))
                                {
                                    using (var gitActionsManager = new GitActionsManager())
                                    {
                                        var workspace = MSBuildWorkspace.Create();

                                        workspace.WorkspaceFailed += Workspace_WorkspaceFailed;

                                        var solution = workspace.OpenSolutionAsync(solutionFile).Result;

                                        var result = gitActionsManager.GetListMethodsOfFileModified(
                                            arguments.RepoPath,
                                            arguments.SelectedBranch,
                                            solution,
                                            file);

                                        workspace.CloseSolution();

                                        if (result.Any())
                                        {
                                            result.ForEach(r =>
                                            {
                                                if (!methodsObserved.Any(x => x.Equals(r)))
                                                {
                                                    methodsObserved.Add(r);
                                                }
                                            });
                                        }
                                    }
                                }
                            });
                        }

                        if (fileSet.Added.Any())
                        {
                            fileSet.Added.ForEach(file =>
                            {
                                if (!file.StartsWith("/"))
                                {
                                    file = string.Format("/{0}", file);
                                }

                                if (file.EndsWith(".cs"))
                                {
                                    using (var gitActionsManager = new GitActionsManager())
                                    {
                                        var workspace = MSBuildWorkspace.Create();

                                        workspace.WorkspaceFailed += Workspace_WorkspaceFailed;

                                        var solution = workspace.OpenSolutionAsync(solutionFile).Result;

                                        var result = gitActionsManager.GetListMethodsOfFile(
                                            arguments.RepoPath, 
                                            solution,
                                            file);

                                        workspace.CloseSolution();

                                        if (result.Any())
                                        {
                                            result.ForEach(r =>
                                            {
                                                if (!methodsObserved.Any(x => x.Equals(r)))
                                                {
                                                    methodsObserved.Add(r);
                                                }
                                            });
                                        }
                                    }
                                }
                            });
                        }

                        if (methodsObserved.Any())
                        {
                            using (var methodAnalyzer = new MethodAnalyzer())
                            {
                                var workspace = MSBuildWorkspace.Create();

                                workspace.WorkspaceFailed += Workspace_WorkspaceFailed;

                                var solution = workspace.OpenSolutionAsync(solutionFile).Result;

                                foreach (var mo in methodsObserved)
                                {
                                    var node = methodAnalyzer.Analyze(solution, mo);

                                    if (node != null)
                                    {
                                        var modulesAffected = new List<Core.Entities.Component>();

                                        WalkNodes(node, ref modulesAffected);

                                        if (modulesAffected.Any())
                                        {
                                            foreach (var moduleAffected in modulesAffected)
                                            {
                                                if (!modulesAffectedFinally.Any(x => x.Description.Equals(moduleAffected.Description) 
                                                    && x.Action.Equals(moduleAffected.Action)))
                                                {
                                                    modulesAffectedFinally.Add(moduleAffected);
                                                }
                                            }
                                        }
                                    }
                                }

                                workspace.CloseSolution();
                            }
                        }
                    }

                    e.Result = new ResultWorkerAnalyze
                    {
                        FileSet = fileSet,
                        TreeNodes = treeNodes,
                        ModulesAffected = modulesAffectedFinally
                    };
                }
            }
        }

        public static void WalkNodes(Core.Entities.Node node, ref List<Core.Entities.Component> modulesAffected)
        {
            if (node.ModulesAffected.Any())
            {
                foreach (var ma in node.ModulesAffected)
                {
                    if (!modulesAffected.Any(x => x.Equals(ma)))
                    {
                        modulesAffected.Add(ma);
                    }
                }
            }

            if (node.Nodes.Any())
            {
                foreach(var child in node.Nodes)
                {
                    WalkNodes(child, ref modulesAffected);
                }
            }
        }

        private void CreateNodeTreeView(
            Core.Entities.TreeNode treeNode,
            System.Windows.Controls.TreeViewItem treeViewItemParent)
        {
            if (treeNode.Nodes.Any())
            {
                treeNode.Nodes.ForEach(n =>
                {
                    var treeViewItem = new TreeViewItem();

                    treeViewItem.IsExpanded = true;

                    treeViewItem.Header = n.Value;

                    treeViewItemParent.Items.Add(treeViewItem);

                    this.CreateNodeTreeView(n, treeViewItem);
                });
            }
            else
            {
                var labelTreeViewItem = new System.Windows.Controls.Label();

                labelTreeViewItem.FontWeight = FontWeights.SemiBold;

                labelTreeViewItem.Content = treeNode.Value;

                switch (treeNode.StateFileGit)
                {
                    case StatesFileGitEnum.Added:
                        labelTreeViewItem.Content = labelTreeViewItem.Content + " (Added)";
                        labelTreeViewItem.Foreground = Brushes.Navy;
                        break;
                    case StatesFileGitEnum.Modified:
                        labelTreeViewItem.Content = labelTreeViewItem.Content + " (Modified)";
                        labelTreeViewItem.Foreground = Brushes.DarkGreen;
                        break;
                    case StatesFileGitEnum.Renamed:
                        labelTreeViewItem.Content = labelTreeViewItem.Content + " (Renamed)";
                        labelTreeViewItem.Foreground = Brushes.DarkOrange;
                        break;
                    case StatesFileGitEnum.Copied:
                        labelTreeViewItem.Content = labelTreeViewItem.Content + " (Copied)";
                        labelTreeViewItem.Foreground = Brushes.DarkViolet;
                        break;
                    case StatesFileGitEnum.Deleted:
                        labelTreeViewItem.Content = labelTreeViewItem.Content + " (Deleted)";
                        labelTreeViewItem.Foreground = Brushes.DarkRed;
                        break;
                    case StatesFileGitEnum.UpdateButUnmerged:
                        labelTreeViewItem.Content = labelTreeViewItem.Content + " (Unmerged)";
                        labelTreeViewItem.Foreground = Brushes.DarkSlateGray;
                        break;
                }

                treeViewItemParent.Items.Add(labelTreeViewItem);
            }
        }

        private void cmbRemoteBranches_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.SelectedBranch = (this.cmbRemoteBranches.SelectedItem as ComboBoxItem).Content.ToString();

            Properties.Settings.Default.Save();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var selectedBranch = Properties.Settings.Default.SelectedBranch;

            if (!string.IsNullOrEmpty(selectedBranch))
            {
                foreach (ComboBoxItem item in this.cmbRemoteBranches.Items)
                {
                    if (item.Content.ToString().Equals(selectedBranch, StringComparison.InvariantCultureIgnoreCase))
                    {
                        this.cmbRemoteBranches.SelectedValue = item;
                        break;
                    }
                }
            }
        }

        private void Workspace_WorkspaceFailed(object sender, WorkspaceDiagnosticEventArgs e)
        {
            Debug.WriteLine(e.Diagnostic.Message);
        }

        private void BtnScan_Click(object sender, RoutedEventArgs e)
        {
            this.stackAllComponents.Children.Clear();

            this.lblTotalComponents.Content = "Total de Componentes";

            try
            {
                var backgroundWorkerAllComponents = new BackgroundWorker();

                backgroundWorkerAllComponents.DoWork += BackgroundWorkerAllComponents_DoWork;

                backgroundWorkerAllComponents.RunWorkerCompleted += BackgroundWorkerAllComponents_RunWorkerCompleted;

                this.btnExportarTextoAfectados.Visibility = Visibility.Collapsed;

                this.btnExportarTextoComponentes.Visibility = Visibility.Collapsed;

                backgroundWorkerAllComponents.RunWorkerAsync(Properties.Settings.Default.RepoPath);

                this.pgbIndeterminate.IsIndeterminate = true;
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show(
                    "Ocurrió un error al intentar obtener todos los componentes,", 
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void BackgroundWorkerAllComponents_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
            }
            else if (e.Cancelled)
            {
            }
            else
            {
                this.btnExportarTextoComponentes.Visibility = Visibility.Visible;

                this.pgbIndeterminate.IsIndeterminate = false;

                var result = (List<Core.Entities.Component>)e.Result;

                if (result.Any())
                {
                    this.btnExportarTextoComponentes.Visibility = Visibility.Visible;

                    this.stackAllComponents.Children.Clear();

                    foreach (var item in result)
                    {
                        TextBlock moduleAffectedBlock = new TextBlock();

                        Thickness margin = moduleAffectedBlock.Margin;

                        margin.Left = 5;

                        margin.Top = 5;

                        margin.Right = 5;

                        margin.Bottom = 5;

                        moduleAffectedBlock.Margin = margin;

                        moduleAffectedBlock.Text = string.Format("{0}-{1}", item.Description, item.Action);

                        this.stackAllComponents.Children.Add(moduleAffectedBlock);
                    }
                }
            }
        }

        private void BackgroundWorkerAllComponents_DoWork(object sender, DoWorkEventArgs e)
        {
            var repoPath = (string)e.Argument;

            string[] solutionFiles = Directory.GetFiles(repoPath, "*.sln", SearchOption.AllDirectories);

            if (solutionFiles.Length > 0)
            {
                try
                {
                    var solutionFile = solutionFiles[0];

                    using (var gitActionsManager = new GitActionsManager())
                    {
                        var workspace = MSBuildWorkspace.Create();

                        workspace.WorkspaceFailed += Workspace_WorkspaceFailed;

                        var solution = workspace.OpenSolutionAsync(solutionFile).Result;

                        var result = gitActionsManager.GetComponents(solution);

                        workspace.CloseSolution();

                        e.Result = result;
                    }
                }
                catch (AggregateException aggregateException)
                {
                    Debug.WriteLine(aggregateException.Message);
                }
            }
        }

        private void btnExportarTextoComponentes_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder stringBuilder = new StringBuilder();
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text file|*.txt";
            saveFileDialog.Title = "Exportar Texto";
            saveFileDialog.ShowDialog();
            
            if (saveFileDialog.FileName != "")
            {
                foreach (var item in this.stackAllComponents.Children)
                {
                    stringBuilder.Append((item as TextBlock).Text);
                    stringBuilder.Append(Environment.NewLine);
                }

                File.WriteAllText(saveFileDialog.FileName, stringBuilder.ToString());
            }
        }

        private void btnExportarTextoAfectados_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder stringBuilder = new StringBuilder();
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text file|*.txt";
            saveFileDialog.Title = "Exportar Texto";
            saveFileDialog.ShowDialog();

            if (saveFileDialog.FileName != "")
            {
                foreach (var item in this.stackModules.Children)
                {
                    stringBuilder.Append((item as TextBlock).Text);
                    stringBuilder.Append(Environment.NewLine);
                }

                File.WriteAllText(saveFileDialog.FileName, stringBuilder.ToString());
            }
        }
    }
}
