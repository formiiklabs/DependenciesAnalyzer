using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using AIMS.Libraries.Scripting.Dom;
using AIMS.Libraries.Scripting.NRefactory;
using AIMS.Libraries.Scripting.CodeCompletion;
using AIMS.Libraries.Scripting.Dom.CSharp;
using AIMS.Libraries.Scripting.Dom.VBNet;
using Formiik.DependenciesAnalyzer.Core.Entities;

namespace AIMS.Libraries.Scripting.ScriptControl.Parser
{
    public class ProjectParser
    {
        static IProjectContent projectContent = null;
        static ProjectContentRegistry projectContentRegistry = null;
        static Dictionary<string, ProjectContentItem> projContentInfo = null;
        
        static string domPersistencePath;
        static string projectPath;
        static SupportedLanguage language;
        static NRefactory.Parser.Errors lastParserError = null;
        static public void Initilize(SupportedLanguage lang)
        {
            language = lang;
            projContentInfo = new Dictionary<string, ProjectContentItem>();
            projectPath = AppDomain.CurrentDomain.BaseDirectory;
            projectContentRegistry = new ProjectContentRegistry();
            domPersistencePath = Path.Combine(Path.GetTempPath(), "AIMSDomCache" );
            Directory.CreateDirectory(domPersistencePath);
            projectContentRegistry.ActivatePersistence(domPersistencePath);
            projectContent = new DefaultProjectContent();
            projectContent.ReferencedContents.Add(projectContentRegistry.Mscorlib);
        }
        

        public static string DomPersistencePath
        {
            get
            {
                return domPersistencePath;
            }
        }

        public static  string ProjectPath
        {
            get
            {
                return projectPath;
            }
        }

        public static  IProjectContent CurrentProjectContent
        {
            get { return projectContent; }
        }

        public static  ProjectContentRegistry ProjectContentRegistry
        {
            get
            {
                return projectContentRegistry;
            }
        }

        public static SupportedLanguage Language
        {
            get { return language; }
            set { 
                ConvertToLanguage(language,value);
                language = value;
            }
        }

        private static void ConvertToLanguage(SupportedLanguage OldLang, SupportedLanguage NewLang)
        {
            Dictionary<string, ProjectContentItem>  projInfo = new Dictionary<string, ProjectContentItem>();
            foreach (ProjectContentItem pc in projContentInfo.Values)
            {
                string fileName = pc.FileName;
                ClearParseInformation(fileName); //Remove last unit from project
                pc.FileName = Path.GetFileNameWithoutExtension(fileName) + (NewLang == SupportedLanguage.CSharp ? ".cs" : ".vb");
                //Change Contents
                pc.Contents = Converter.CodeConverter.ConvertCode(pc.Contents, (OldLang==SupportedLanguage.CSharp?ScriptLanguage.CSharp:ScriptLanguage.VBNET), (NewLang==SupportedLanguage.CSharp?ScriptLanguage.CSharp:ScriptLanguage.VBNET));
                projInfo.Add(pc.FileName, pc);
            }
            language = NewLang; //Set New Language
            projContentInfo = projInfo; //Reset new proj Contents
            foreach (ProjectContentItem pc in projContentInfo.Values)
            {
                //Now parse
                ParseProjectContents(pc.FileName, pc.Contents,pc.IsOpened);
            }
        }
        public static IParser GetParser(string fileName)
        {
            if (Path.GetExtension(fileName).ToLower().Trim() == ".cs")
                return new CSharpParser();
            else
                return new VbParser();
        }

        public static IResolver CreateResolver(string fileName)
        {
            IParser parser = GetParser(fileName);
            if (parser != null)
            {
                return parser.CreateResolver();
            }
            return null;
        }

        public static ResolveResult Resolve(ExpressionResult expressionResult,
                                            int caretLineNumber,
                                            int caretColumn,
                                            string fileName,
                                            string fileContent)
        {
            IResolver resolver = CreateResolver(fileName);
            if (resolver != null)
            {
                return resolver.Resolve(expressionResult, caretLineNumber, caretColumn, fileName, fileContent);
            }
            return null;
        }

        public static string GetFileContents(string fileName)
        {
            if (projContentInfo.ContainsKey(fileName))
            {
                return projContentInfo[fileName].Contents;
            }
            else
                return string.Empty;

        }

        public static Dom.NRefactoryResolver.NRefactoryResolver GetResolver()
        {
            Dom.NRefactoryResolver.NRefactoryResolver resolver = new Dom.NRefactoryResolver.NRefactoryResolver(projectContent, (language == SupportedLanguage.CSharp ? LanguageProperties.CSharp : LanguageProperties.VBNet));
            return resolver;
        }

        public static Dictionary<string, ProjectContentItem> ProjectFiles
        {
            get{return projContentInfo;}
        }

        public static void RemoveContentFile(string fileName)
        {
            if (projContentInfo.ContainsKey(fileName))
            {
                ClearParseInformation(fileName);
                projContentInfo.Remove(fileName);
            }
        }

        public static ParseInformation ParseProjectContents(string fileName, string Content)
        {
            return Parser.ProjectParser.ParseProjectContents(fileName, Content, false);
        }

        public static ParseInformation ParseProjectContents(string fileName, string Content,bool IsOpened)
        {
            if (projContentInfo.ContainsKey(fileName) == false)
            {
                projContentInfo[fileName] = new ProjectContentItem(fileName, Content, IsOpened);
            }

            projContentInfo[fileName].Contents = Content;

            IParser parser = GetParser(fileName);
            if (parser == null)
            {
                return null;
            }

            ICompilationUnit parserOutput = null;
            parserOutput = parser.Parse(projectContent, fileName, Content);
            lastParserError = parser.LastErrors;
            
            if (projContentInfo.ContainsKey(fileName))
            {
                ParseInformation parseInformation = projContentInfo[fileName].ParsedContents;
                if (parseInformation == null)
                {
                    parseInformation = new ParseInformation();
                    projContentInfo[fileName].ParsedContents = parseInformation;
                }
                projectContent.UpdateCompilationUnit(parseInformation.MostRecentCompilationUnit, parserOutput, fileName);
            }
            else
            {
                projectContent.UpdateCompilationUnit(null, parserOutput, fileName);
            }
            
            return UpdateParseInformation(parserOutput, fileName);

            
        }

        public static NRefactory.Parser.Errors LastParserErrors
        {
           get{return lastParserError;}
        }

        public static ParseInformation GetParseInformation2(string fileName)
        {
            if (fileName == null || fileName.Length == 0)
            {
                return null;
            }

            var contents = string.Empty;

            using (StreamReader sr = new StreamReader(fileName))
            {
                contents = sr.ReadToEnd();
            }

            Initilize(SupportedLanguage.CSharp);

            return ParseProjectContents(fileName, contents);
        }

        public static ParseInformation GetParseInformation(string fileName)
        {
            if (fileName == null || fileName.Length == 0)
            {
                return null;
            }
            if (!projContentInfo.ContainsKey(fileName))
            {
                return ParseProjectContents(fileName,projContentInfo[fileName].Contents);
            }
            return projContentInfo[fileName].ParsedContents;
        }

        public static void ClearParseInformation(string fileName)
        {
            if (fileName == null || fileName.Length == 0)
            {
                return;
            }
            if (projContentInfo.ContainsKey(fileName))
            {
                ParseInformation parseInfo = projContentInfo[fileName].ParsedContents;
                if (parseInfo != null && parseInfo.MostRecentCompilationUnit != null)
                {
                    parseInfo.MostRecentCompilationUnit.ProjectContent.RemoveCompilationUnit(parseInfo.MostRecentCompilationUnit);
                }
                projContentInfo[fileName].ParsedContents = null;
            }
        }

        public static ParseInformation UpdateParseInformation(ICompilationUnit parserOutput, string fileName)
        {
            ParseInformation parseInformation = projContentInfo[fileName].ParsedContents;

            if (parserOutput.ErrorsDuringCompile)
            {
                parseInformation.DirtyCompilationUnit = parserOutput;
            }
            else
            {
                parseInformation.ValidCompilationUnit = parserOutput;
                parseInformation.DirtyCompilationUnit = null;
            }
            projContentInfo[fileName].ParsedContents = parseInformation;
            return parseInformation;
        }

        public static AmbienceReflectionDecorator CurrentAmbience
        {
            get
            {
                IAmbience defAmbience = null;
                if (language==SupportedLanguage.CSharp)
                    defAmbience = new CSharpAmbience();
                else
                    defAmbience = new VBNetAmbience();

                return new AmbienceReflectionDecorator(defAmbience);
            }
        }

    }
}
