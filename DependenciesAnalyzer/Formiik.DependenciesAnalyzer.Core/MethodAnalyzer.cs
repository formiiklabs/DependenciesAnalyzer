using Formiik.DependenciesAnalyzer.Core.Entities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Formiik.DependenciesAnalyzer.Core
{
    public class MethodAnalyzer : IDisposable
    {
        public Node Analyze(string solutionPath, string fullNameMethod)
        {
            if (string.IsNullOrEmpty(solutionPath) || string.IsNullOrEmpty(fullNameMethod))
            {
                throw new ArgumentException(Messages.ArgumentInvalid);
            }

            try
            {
                var node = new Node
                {
                    MethodName = fullNameMethod
                };

                var workspace = MSBuildWorkspace.Create();

                workspace.WorkspaceFailed += Workspace_WorkspaceFailed;

                var solution = workspace.OpenSolutionAsync(solutionPath).Result;

                this.ComputeReferences(ref solution, node);

                return node;
            }
            catch (ReflectionTypeLoadException ex)
            {
                StringBuilder sb = new StringBuilder();

                foreach (Exception exSub in ex.LoaderExceptions)
                {
                    sb.AppendLine(exSub.Message);

                    FileNotFoundException exFileNotFound = exSub as FileNotFoundException;

                    if (exFileNotFound != null)
                    {
                        if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                        {
                            sb.AppendLine("Fusion Log:");

                            sb.AppendLine(exFileNotFound.FusionLog);
                        }
                    }
                    sb.AppendLine();
                }

                Debug.WriteLine(sb.ToString());

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

                return null;
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        private void ComputeReferences(ref Solution solution, Node nodeTree)
        {
            foreach (var project in solution.Projects)
            {
                foreach (var document in project.Documents)
                {
                    try
                    {
                        var model = document.GetSemanticModelAsync().Result;

                        var methodInvocation = document.GetSyntaxRootAsync().Result;

                        //var descendantNodes = methodInvocation.DescendantNodes();

                        var nameMethod = nodeTree.MethodName.Replace("()", "");

                        var nodes = from node in methodInvocation.DescendantNodes()
                            .OfType<InvocationExpressionSyntax>()
                                    let symbol = model.GetSymbolInfo(node.Expression).Symbol as IMethodSymbol
                                    where symbol != null
                                    where symbol.ToString().Contains(nameMethod)
                                    select new
                                    {
                                        node,
                                        symbol
                                    };

                        //var symbols = nodes.Select(x => x.symbol).ToList();
                        //continue;

                        if (nodes.Any())
                        {
                            foreach (var node in nodes)
                            {
                                SyntaxNode parent = node.node;

                                do
                                {
                                    parent = parent.Parent;
                                }
                                while (!parent.IsKind(SyntaxKind.MethodDeclaration));

                                if (parent != null)
                                {
                                    var symbolParent = model.GetDeclaredSymbol(parent);

                                    var symbolParentString = symbolParent.ToString();

                                    var attributes = symbolParent.GetAttributes();

                                    var modulesAffected = new List<string>();

                                    if (attributes.Any())
                                    {
                                        foreach (var attribute in attributes)
                                        {
                                            if (attribute.AttributeClass.Name.Equals("ComponentAffectedAttribute"))
                                            {
                                                modulesAffected.Add(attribute.ConstructorArguments.First().Value.ToString());
                                            }
                                        }
                                    }

                                    var nodeTreeChild = new Node
                                    {
                                        MethodName = symbolParentString,
                                        ModulesAffected = modulesAffected
                                    };

                                    ComputeReferences(ref solution, nodeTreeChild);

                                    nodeTree.Nodes.Add(nodeTreeChild);
                                }
                                else
                                {
                                    return;
                                }
                            }
                        }
                    }
                    catch (AggregateException ae)
                    {
                        Debug.WriteLine(ae.Message);
                    }
                }
            }
        }

        private void Workspace_WorkspaceFailed(object sender, WorkspaceDiagnosticEventArgs e)
        {
            Debug.WriteLine(e.Diagnostic.Message);
        }
    }
}
