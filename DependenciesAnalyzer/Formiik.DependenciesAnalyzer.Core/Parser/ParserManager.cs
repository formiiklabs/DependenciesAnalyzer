using AIMS.Libraries.Scripting.Dom;
using Formiik.DependenciesAnalyzer.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Formiik.DependenciesAnalyzer.Core.Parser
{
    public class ParserManager : IDisposable
    {
        #region Constructor
        public ParserManager()
        {
        }
        #endregion

        #region Public Methods
        public List<string> GetCodeMehtods(string fileName)
        {
            var methods = new List<string>();

            var method = string.Empty;

            try
            {
                ICompilationUnit currentCompilationUnit;

                var classes = new List<UnitCode>();

                ParseInformation parseInfo =
                    AIMS.Libraries.Scripting.ScriptControl.Parser.ProjectParser.GetParseInformation2(fileName);

                if (parseInfo != null)
                {
                    currentCompilationUnit = parseInfo.MostRecentCompilationUnit;

                    if (currentCompilationUnit != null)
                    {
                        foreach (IClass @class in currentCompilationUnit.Classes)
                        {
                            classes.Add(new UnitCode
                            {
                                Class = @class
                            });
                        }
                    }
                }
                if (classes.Any())
                {
                    var methodInside = string.Empty;

                    foreach (var c in classes)
                    {
                        foreach (var m in c.Class.Methods)
                        {
                            methods.Add(m.FullyQualifiedName);
                        }
                    }
                }
            }
            catch
            {
                throw;
            }

            return methods;
        }

        public string GetCodeMethodByLineNumber(string fileName, int lineNumber)
        {
            var method = string.Empty;

            try
            {
                ICompilationUnit currentCompilationUnit;

                var classes = new List<UnitCode>();

                ParseInformation parseInfo =
                    AIMS.Libraries.Scripting.ScriptControl.Parser.ProjectParser.GetParseInformation2(fileName);

                if (parseInfo != null)
                {
                    currentCompilationUnit = parseInfo.MostRecentCompilationUnit;

                    if (currentCompilationUnit != null)
                    {
                        foreach (IClass @class in currentCompilationUnit.Classes)
                        {
                            classes.Add(new UnitCode
                            {
                                Class = @class
                            });
                        }
                    }
                }

                if (classes.Any())
                {
                    var methodInside = string.Empty;

                    foreach (var c in classes)
                    {
                        foreach (var m in c.Class.Methods)
                        {
                            var isInside = this.IsInside(lineNumber, m, out methodInside);

                            if (isInside)
                            {
                                break;
                            }
                        }

                        if (!string.IsNullOrEmpty(methodInside))
                        {
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(methodInside))
                    {
                        method = methodInside;
                    }
                }
            }
            catch
            {
                throw;
            }

            return method;
        }

        public bool IsInside(int lineNumber, object item, out string methodInsise)
        {
            methodInsise = string.Empty;

            IMember member = item as IMember;

            if (member == null || member.Region.IsEmpty)
            {
                return false;
            }

            bool isInside = member.Region.BeginLine - 1 <= lineNumber;

            if (member is IMethodOrProperty)
            {
                if (((IMethodOrProperty)member).BodyRegion.EndLine >= 0)
                {
                    isInside &= lineNumber <= ((IMethodOrProperty)member).BodyRegion.EndLine - 1;
                }
                else
                {
                    isInside = member.Region.BeginLine - 1 == lineNumber;
                }
            }
            else
            {
                isInside &= lineNumber <= member.Region.EndLine - 1;
            }

            if (isInside)
            {
                methodInsise = member.FullyQualifiedName;
            }

            return isInside;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
