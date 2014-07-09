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
                var _libs = ToCast("libs").Select(fileRef => new XCModFile(fileRef)).ToList();

                if(_libs.Count == 0)
                    _libs.Add(new XCModFile(""));

                return _libs;
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