using System;
using System.Collections.Generic;
using System.Text;
using AIMS.Libraries.Scripting.Dom;
namespace AIMS.Libraries.Scripting.ScriptControl.Parser
{
    public class ProjectContentItem
    {
        string filename = "";
        string content = "";
        bool isopened = false;
        ParseInformation parseInfo=null;
        public ProjectContentItem(string fileName):this(fileName,"",false)
        {
        }

        public ProjectContentItem(string fileName,bool Isopened):this(fileName,"",Isopened)
        {
        }

        public ProjectContentItem(string fileName,string filecontent):this(fileName,filecontent,false)
        {
        }

        public ProjectContentItem(string fileName, string filecontent,bool Isopened)
        {
            filename = fileName;
            content = filecontent;
            isopened = Isopened;
        }

        public string FileName
        {
            get { return filename; }
            set { filename = value; }
        }
        public bool IsOpened
        {
            get { return isopened; }
            set { isopened = value; }
        }
        public string Contents
        {
            get { return content; }
            set { content = value; }
        }

        public ParseInformation ParsedContents
        {
            get {return parseInfo; }
            set { parseInfo = value; }
        }
    }
}
