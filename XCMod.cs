using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.IO;
using MiniJSON;

namespace UnityEditor.XCodeEditor
{
    public class XCMod
    {
        //		private string group;
        //		private ArrayList patches;
        //		private ArrayList libs;
        //		private ArrayList frameworks;
        //		private ArrayList headerpaths;
        //		private ArrayList files;
        //		private ArrayList folders;
        //		private ArrayList excludes;
        private Dictionary<string, object> _datastore;

        public string name { get; private set; }
        public string path { get; private set; }

        public string group
        {
            get
            {
                return (string)_datastore["group"];
            }
        }

        public List<string> patches
        {
            get
            {
                return ToCast("patches");
            }
        }

        public List<XCModFile> libs
        {
            get
            {
                return ToCast("libs").Select(fileRef => new XCModFile(fileRef)).ToList();
            }
        }

        public List<string> frameworks
        {
            get
            {
                return ToCast("frameworks");
            }
        }

        public List<string> headerpaths
        {
            get
            {
                return ToCast("headerpaths");
            }
        }

        public Dictionary<string, object> buildSettings
        {
            get
            {
                return (Dictionary<string, object>)_datastore["buildSettings"];
            }
        }

        public List<string> files
        {
            get
            {
                return ToCast("files");
            }
        }

        public List<string> folders
        {
            get
            {
                return ToCast("folders");
            }
        }

        public List<string> excludes
        {
            get
            {
                return ToCast("excludes");
            }
        }

        public XCMod(string filename)
        {
            if (!File.Exists(filename))
            {
                Debug.LogWarning("File does not exist.");
            }

            name = System.IO.Path.GetFileNameWithoutExtension(filename);
            path = System.IO.Path.GetDirectoryName(filename);

            string contents = File.ReadAllText(filename);
            _datastore = Json.Deserialize(contents) as Dictionary<string, object>;


            //			group = (string)_datastore["group"];
            //			patches = (ArrayList)_datastore["patches"];
            //			libs = (ArrayList)_datastore["libs"];
            //			frameworks = (ArrayList)_datastore["frameworks"];
            //			headerpaths = (ArrayList)_datastore["headerpaths"];
            //			files = (ArrayList)_datastore["files"];
            //			folders = (ArrayList)_datastore["folders"];
            //			excludes = (ArrayList)_datastore["excludes"];
        }

        List<string> ToCast(string key)
        {
            var list = new List<string>();

            if (!_datastore.ContainsKey(key)) return list;
            var objects = _datastore[key] as List<object>;

            if (objects != null)
            {
                list = objects.Cast<string>().ToList();
            }

            return list;
        }

        //	"group": "GameCenter",
        //	"patches": [],
        //	"libs": [],
        //	"frameworks": ["GameKit.framework"],
        //	"headerpaths": ["Editor/iOS/GameCenter/**"],					
        //	"files":   ["Editor/iOS/GameCenter/GameCenterBinding.m",
        //				"Editor/iOS/GameCenter/GameCenterController.h",
        //				"Editor/iOS/GameCenter/GameCenterController.mm",
        //				"Editor/iOS/GameCenter/GameCenterManager.h",
        //				"Editor/iOS/GameCenter/GameCenterManager.m"],
        //	"folders": [],	
        //	"excludes": ["^.*\\.meta$", "^.*\\.mdown^", "^.*\\.pdf$"]

    }

    public class XCModFile
    {
        public string filePath { get; private set; }
        public bool isWeak { get; private set; }

        public XCModFile(string inputString)
        {
            isWeak = false;

            if (inputString.Contains(":"))
            {
                string[] parts = inputString.Split(':');
                filePath = parts[0];
                isWeak = (parts[1].CompareTo("weak") == 0);
            }
            else
            {
                filePath = inputString;
            }
        }

        public override string ToString()
        {
            return filePath + (isWeak ? ":weak" : "");
        }
    }
}