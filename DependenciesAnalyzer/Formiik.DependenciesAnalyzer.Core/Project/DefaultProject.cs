using System;
using System.Collections.Generic;
using System.IO;
using AIMS.Libraries.Scripting.Dom;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace AIMS.Libraries.Scripting.ScriptControl.Project
{
    class DefaultProject:IProject
    {
        IList<ProjectItem> _defProjectItems= null;
        string _AssemblyName ="";
        string _OutputAssemblyFullPath = "";
        string _RootNamespace = "";
        string _FileName = "";
        #region IProject Members

       
        public DefaultProject()
        {
            _defProjectItems = new List<ProjectItem>();
            _AssemblyName = "AIMSScript.dll";
            _RootNamespace = "AIMS.Script";
            if (Parser.ProjectParser.Language == AIMS.Libraries.Scripting.NRefactory.SupportedLanguage.CSharp)
                _FileName = "AIMS Script.cs";
            else
                _FileName = "AIMS Script.vb";

            this._OutputAssemblyFullPath = Parser.ProjectParser.ProjectPath;

        }

        ReadOnlyCollection<ProjectItem> IProject.Items
        {
            get
            {
                return new ReadOnlyCollection<ProjectItem>(_defProjectItems);
            }
        }

        void IProject.AddProjectItem(ProjectItem item)
        {
            _defProjectItems.Add(item);
            try
            {
                lock (Parser.ProjectParser.CurrentProjectContent.ReferencedContents)
                {
                    Parser.ProjectParser.CurrentProjectContent.ReferencedContents.Add(Parser.ProjectParser.ProjectContentRegistry.GetProjectContentForReference(item.Include, item.FileName));
                }
                if (true)
                {
                    UpdateReferenceInterDependencies();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            
        }

        void UpdateReferenceInterDependencies()
        {
            // Use ToArray because the collection could be modified inside the loop
            IProjectContent[] referencedContents;
            lock (Parser.ProjectParser.CurrentProjectContent.ReferencedContents)
            {
                referencedContents = new IProjectContent[Parser.ProjectParser.CurrentProjectContent.ReferencedContents.Count];
                Parser.ProjectParser.CurrentProjectContent.ReferencedContents.CopyTo(referencedContents, 0);
            }
            foreach (IProjectContent referencedContent in referencedContents)
            {
                if (referencedContent is ReflectionProjectContent)
                {
                    ((ReflectionProjectContent)referencedContent).InitializeReferences();
                }
            }
        }
        

        bool IProject.RemoveProjectItem(ProjectItem item)
        {
            
            return _defProjectItems.Remove(item);
        }

        IEnumerable<ProjectItem> IProject.GetItemsOfType(ItemType type)
        {
            foreach (ProjectItem item in this._defProjectItems)
            {
                if (item.ItemType == type)
                {
                    yield return item;
                }
            }
        }

        LanguageProperties IProject.LanguageProperties
        {
            get
            {
                Dom.DefaultProjectContent p = new DefaultProjectContent();
                p.Language = Parser.ProjectParser.Language==AIMS.Libraries.Scripting.NRefactory.SupportedLanguage.CSharp?LanguageProperties.CSharp:LanguageProperties.VBNet;
                return p.Language;
                
            }
        }

        IAmbience IProject.Ambience
        {
            get
            {
                return Parser.ProjectParser.CurrentAmbience;
            }
        }

        string IProject.Directory
        {
            get { return Parser.ProjectParser.ProjectPath; }
        }

        string IProject.AssemblyName
        {
            get
            {
                return _AssemblyName;
            }
            set
            {
                _AssemblyName = value;
            }
        }
       
        string IProject.RootNamespace
        {
            get
            {
                return _RootNamespace;
            }
            set
            {
                _RootNamespace = value;
            }
        }
         
        string IProject.OutputAssemblyFullPath
        {
            get {return Path.Combine(this._OutputAssemblyFullPath, _AssemblyName); }
        }

        string IProject.FileName
        {
            get
            {
                return _FileName;
            }
            set
            {
                _FileName = value;
            }
        }

        #endregion
    }
}
