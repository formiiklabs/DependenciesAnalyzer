using Formiik.DependenciesAnalyzer.Core.Entities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
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
        public Node Analyze(Solution solution, string fullNameMethod)
        {
            try
            {
                var node = new Node
                {
                    MethodName = fullNameMethod
                };

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

        public string GetMethodFromLineCode(Solution solution, string pathFile, int lineCode)
        {
            var isFound = false;

            var methodName = string.Empty;

            var tokensFilePath = pathFile.Split('/');

            if (tokensFilePath.Count() >= 2)
            {
                var documentName = tokensFilePath.Last();

                foreach (var project in solution.Projects)
                {
                    foreach (var document in project.Documents)
                    {
                        if (document.Name.Equals(documentName))
                        {
                            try
                            {
                                var model = document.GetSemanticModelAsync().Result;

                                var methodInvocation = document.GetSyntaxRootAsync().Result;

                                var span = document.GetSyntaxTreeAsync().Result.GetText().Lines[lineCode].Span;

                                var nodes = from node in methodInvocation.DescendantNodes().OfType<MethodDeclarationSyntax>()
                                            let symbol = model.GetDeclaredSymbol(node) as IMethodSymbol
                                            where symbol != null
                                            where node.Span.IntersectsWith(span)
                                            select new
                                            {
                                                node,
                                                symbol
                                            };

                                if (nodes.Any())
                                {
                                    foreach (var node in nodes)
                                    {
                                        methodName = node.symbol.ToString();

                                        isFound = true;

                                        break;
                                    }
                                }

                                break;
                            }
                            catch (Exception exception)
                            {
                                Debug.WriteLine(exception.Message);
                            }
                        }
                    }

                    if (isFound)
                    {
                        break;
                    }
                }
            }

            return methodName;
        }

        public List<string> GetMethods(Solution solution, string pathFile)
        {
            var isFound = false;

            var methodsName = new List<string>();

            var tokensFilePath = pathFile.Split('\\');

            if (tokensFilePath.Length == 1)
            {
                tokensFilePath = pathFile.Split('/');
            }

            if (tokensFilePath.Count() >= 2)
            {
                var documentName = tokensFilePath.Last();

                foreach (var project in solution.Projects)
                {
                    foreach (var document in project.Documents)
                    {
                        if (document.Name.Equals(documentName))
                        {
                            var model = document.GetSemanticModelAsync().Result;

                            var methodInvocation = document.GetSyntaxRootAsync().Result;

                            var nodes = from node in methodInvocation.DescendantNodes().OfType<MethodDeclarationSyntax>()
                                        let symbol = model.GetDeclaredSymbol(node) as IMethodSymbol
                                        where symbol != null
                                        select new
                                        {
                                            node,
                                            symbol
                                        };

                            if (nodes.Any())
                            {
                                foreach (var node in nodes)
                                {
                                    methodsName.Add(node.symbol.ToString());
                                }
                            }

                            isFound = true;

                            break;
                        }
                    }

                    if (isFound)
                    {
                        break;
                    }
                }
            }

            return methodsName;
        }

        public List<Component> GetComponents(Solution solution)
        {
            var components = new List<Component>();

            foreach (var project in solution.Projects)
            {
                foreach (var document in project.Documents)
                {
                    try
                    {
                        var model = document.GetSemanticModelAsync().Result;

                        var methodInvocation = document.GetSyntaxRootAsync().Result;

                        var nodes = from node in methodInvocation.DescendantNodes().OfType<MethodDeclarationSyntax>()
                                    let symbol = model.GetDeclaredSymbol(node) as IMethodSymbol
                                    where symbol != null
                                    select new
                                    {
                                        node,
                                        symbol
                                    };

                        if (nodes.Any())
                        {
                            foreach (var node in nodes)
                            {
                                var attributes = node.symbol.GetAttributes();

                                if (attributes.Any())
                                {
                                    foreach (var attribute in attributes)
                                    {
                                        if (attribute.AttributeClass.Name.Equals("ComponentAffectedAttribute"))
                                        {
                                            if (attribute.ConstructorArguments.Length == 2)
                                            {
                                                components.Add(new Component
                                                {
                                                    Description = attribute.ConstructorArguments[0].Value.ToString(),
                                                    Action = attribute.ConstructorArguments[1].Value.ToString()
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        Debug.WriteLine(exception.Message);
                    }
                }
            }

            return components;
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

                        var nodes = from node in methodInvocation.DescendantNodes()
                            .OfType<InvocationExpressionSyntax>()
                                    let symbol = model.GetSymbolInfo(node.Expression).Symbol as IMethodSymbol
                                    where symbol != null
                                    where symbol.ToString().Equals(nodeTree.MethodName)
                                    select new
                                    {
                                        node,
                                        symbol
                                    };

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

                                    var modulesAffected = new List<Component>();

                                    if (attributes.Any())
                                    {
                                        foreach (var attribute in attributes)
                                        {
                                            if (attribute.AttributeClass.Name.Equals("ComponentAffectedAttribute"))
                                            {
                                                if (attribute.ConstructorArguments.Length == 2)
                                                {
                                                    modulesAffected.Add(new Component
                                                    {
                                                        Description = attribute.ConstructorArguments[0].Value.ToString(),
                                                        Action = attribute.ConstructorArguments[1].Value.ToString()
                                                    });
                                                }
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
