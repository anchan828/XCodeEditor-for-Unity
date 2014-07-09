using System.Collections.Generic;
using System.IO;
using System.Linq;
using MiniJSON;
using UnityEditor.XCodeEditor;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
public class XCPreference : PopupWindowContent
{
    private Vector2 popupScroll = Vector2.zero;
    private static Vector2 mainScroll = Vector2.zero;

    static string group = "";
    static List<string> frameworks = new List<string>();
    static List<string> patches = new List<string> { "" };
    static List<XCModFile> libs = new List<XCModFile> { new XCModFile("") };
    static List<string> headerpaths = new List<string> { "" };
    static List<string> folders = new List<string> { "" };
    static List<string> excludes = new List<string> { "^.*.meta$" };
    static List<string> files = new List<string> { "" };
    static List<KeyValuePair<string, string>> buildSettings = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("", "") };
    private static ReorderableList frameworkList, patchList, libList, headerpathList, groupList, fileList, folderList, excludeList, buildSettingList;
    [PreferenceItem("XCode Settings")]
    static void OnXCPreference()
    {
        if (groupList == null)
        {
            DeSerialize();
            var _group = new List<string> { "" };
            groupList = new ReorderableList(_group, typeof(List<string>), false, true, false, false)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Group"),
                drawElementCallback = (rect, index, active, focused) =>
                {
                    rect.height -= 3;
                    @group = EditorGUI.TextField(rect, @group);
                }
            };
        }

        if (frameworkList == null)
        {
            frameworkList = new ReorderableList(frameworks, typeof(List<string>), false, true, true, true)
            {
                onAddCallback = list => PopupWindow.Show(new Rect(0, 0, 0, 0), new XCPreference()),
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Frameworks")
            };
        }

        if (patchList == null)
            InitReorderableList(ref patchList, patches, "Patches");

        if (libList == null)
        {
            libList = new ReorderableList(libs, typeof(List<XCModFile>), false, true, true, true)
            {
                drawElementCallback = (rect, index, active, focused) =>
                {
                    var xcModFile = libs[index];
                    EditorGUI.BeginChangeCheck();
                    rect.height -= 3;
                    rect.width *= 0.7f;
                    string filePath = EditorGUI.TextField(rect, xcModFile.filePath);
                    rect.x += rect.width + 20;
                    rect.width = 100;
                    bool isWeak = EditorGUI.ToggleLeft(rect, "isWeak", xcModFile.isWeak);

                    if (EditorGUI.EndChangeCheck())
                    {
                        var newModFile = new XCModFile(filePath + (isWeak ? ":weak" : ""));
                        libs.RemoveAt(index);
                        libs.Insert(index, newModFile);
                    }
                },
                onAddCallback = list => libs.Add(new XCModFile("")),
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Libs")
            };
        }


        if (headerpathList == null)
            InitReorderableList(ref headerpathList, headerpaths, "Headerpaths");

        if (fileList == null)
            InitReorderableList(ref fileList, files, "Files");

        if (folderList == null)
            InitReorderableList(ref folderList, folders, "Folders");

        if (excludeList == null)
            InitReorderableList(ref excludeList, excludes, "Excludes");

        if (buildSettingList == null)
        {
            buildSettingList = new ReorderableList(buildSettings, typeof(List<KeyValuePair<string, string>>), false, true, true, true)
            {
                onAddCallback = l => buildSettings.Add(new KeyValuePair<string, string>("", "")),
                drawElementCallback = (rect, index, active, focused) =>
                {
                    rect.height -= 3;
                    EditorGUI.BeginChangeCheck();
                    var buildSetting = buildSettings[index];
                    rect.width *= 0.49f;
                    string key = EditorGUI.TextField(rect, buildSetting.Key);
                    rect.x += rect.width + 6;
                    string val = EditorGUI.TextField(rect, buildSetting.Value);
                    if (EditorGUI.EndChangeCheck())
                    {
                        buildSettings.RemoveAt(index);
                        buildSettings.Insert(index, new KeyValuePair<string, string>(key, val));
                    }
                },
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "BuildSettings")
            };
        }

        mainScroll = EditorGUILayout.BeginScrollView(mainScroll, GUILayout.Width(Screen.width - 130));

        groupList.DoLayoutList();
        frameworkList.DoLayoutList();
        EditorGUILayout.Space();
        patchList.DoLayoutList();
        EditorGUILayout.Space();
        libList.DoLayoutList();
        EditorGUILayout.Space();
        headerpathList.DoLayoutList();
        EditorGUILayout.Space();
        fileList.DoLayoutList();
        EditorGUILayout.Space();
        folderList.DoLayoutList();
        EditorGUILayout.Space();
        excludeList.DoLayoutList();
        EditorGUILayout.Space();
        buildSettingList.DoLayoutList();
        EditorGUILayout.EndScrollView();
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginHorizontal();

        EditorGUI.BeginDisabledGroup(!File.Exists(configPath));
        if (GUILayout.Button("Load"))
        {
            DeSerialize();
            GUI.FocusControl("");
            EditorApplication.delayCall += EditorWindow.focusedWindow.Repaint;
        }
        EditorGUI.EndDisabledGroup();
        GUILayout.Space(Screen.width * 0.5f);
        if (GUILayout.Button("Save"))
        {
            Serialize();
            EditorWindow.focusedWindow.ShowNotification(new GUIContent("Saved"));
        }

        EditorGUILayout.EndHorizontal();
    }

    private static void InitReorderableList(ref ReorderableList list, List<string> elements, string header)
    {
        list = new ReorderableList(elements, typeof(List<string>), false, true, true, true)
        {
            drawElementCallback = (rect, index, active, focused) =>
            {
                rect.height -= 3;
                elements[index] = EditorGUI.TextField(rect, elements[index]);
            },
            onAddCallback = l => elements.Add(""),
            drawHeaderCallback = rect => EditorGUI.LabelField(rect, header)
        };
    }

    private static void Serialize()
    {
        var dictionary = new Dictionary<string, object>
        {
            {"group", @group},
            {"frameworks", frameworks.Where(s => !string.IsNullOrEmpty(s)).ToArray()},
            {"patches", patches.Where(s => !string.IsNullOrEmpty(s)).ToArray()},
            {"libs", libs.Where(s => !string.IsNullOrEmpty(s.filePath)).ToArray()},
            {"headerpaths", headerpaths.Where(s => !string.IsNullOrEmpty(s)).ToArray()},
            {"files", files.Where(s => !string.IsNullOrEmpty(s)).ToArray()},
            {"folders", folders.Where(s => !string.IsNullOrEmpty(s)).ToArray()},
            {"excludes", excludes.Where(s => !string.IsNullOrEmpty(s)).ToArray()}
        };

        var _buildSettings = new Dictionary<string, object>();

        foreach (var buildSetting in buildSettings)
        {
            if (string.IsNullOrEmpty(buildSetting.Key) || string.IsNullOrEmpty(buildSetting.Value)) continue;

            var strings = buildSetting.Value.Split(',', ';').Where(s => !string.IsNullOrEmpty(s)).ToArray();

            _buildSettings.Add(buildSetting.Key, strings.Length == 1 ? (object)buildSetting.Value.Trim() : strings);

        }
        dictionary.Add("buildSettings", _buildSettings);

        File.WriteAllText(configPath, Json.Serialize(dictionary));
    }

    private static void DeSerialize()
    {
        if (!File.Exists(configPath)) return;

        var xcMod = new XCMod(configPath);

        group = xcMod.group;
        frameworks = xcMod.frameworks;
        patches = xcMod.patches;
        libs = xcMod.libs;
        headerpaths = xcMod.headerpaths;
        files = xcMod.files;
        folders = xcMod.folders;
        excludes = xcMod.excludes;

        buildSettings.Clear();
        foreach (var key in xcMod.buildSettings.Keys)
        {
            var val = xcMod.buildSettings[key];

            if (val is string)
            {
                buildSettings.Add(new KeyValuePair<string, string>(key, val.ToString()));
            }
            else
            {
                var vs = ((List<object>)val).Cast<string>().Aggregate("", (current, v) => current + (v + ";"));
                buildSettings.Add(new KeyValuePair<string, string>(key, vs));
            }

        }
    }

    private Dictionary<string, bool> canSelectFrameworks = new Dictionary<string, bool>();


    public override void OnOpen()
    {
        canSelectFrameworks = FRAMEWORKS.Where(f => !frameworks.Contains(f)).ToDictionary(f => f, _ => false);
    }

    public override void OnClose()
    {
        canSelectFrameworks = new Dictionary<string, bool>();
    }

    public override Vector2 GetWindowSize()
    {
        return new Vector2(370, Mathf.Min(FRAMEWORKS.Length * 18, 330) + 25);
    }


    public override void OnGUI(Rect rect)
    {
        if (canSelectFrameworks.Count == 0) return;
        popupScroll = EditorGUILayout.BeginScrollView(popupScroll);
        for (int i = 0; i < canSelectFrameworks.Count; i++)
        {
            var name = canSelectFrameworks.Keys.ElementAt(i);
            canSelectFrameworks[name] = EditorGUILayout.ToggleLeft(name, canSelectFrameworks[name]);
            GUILayout.Space(3);
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(Screen.width * 0.4f);
        if (GUILayout.Button("Add"))
        {
            frameworks.AddRange(canSelectFrameworks.Where(f => f.Value).Select(f => f.Key).ToList());
            frameworks.Sort();
            editorWindow.Close();
        }

        if (GUILayout.Button("Cancel"))
        {
            editorWindow.Close();
        }

        EditorGUILayout.EndHorizontal();
    }
    private readonly string[] FRAMEWORKS = new[]
    {
          "Accelerate.framework",
          "Accounts.framework",
          "AddressBook.framework",
          "AddressBookUI.framework",
          "AdSupport.framework",
          "AssetsLibrary.framework",
          "AudioToolbox.framework",
          "AudioUnit.framework",
          "AVFoundation.framework",
          "CFNetwork.framework",
          "CoreAudio.framework",
          "CoreBluetooth.framework",
          "CoreData.framework",
          "CoreFoundation.framework",
          "CoreGraphics.framework",
          "CoreImage.framework",
          "CoreLocation.framework",
          "CoreMedia.framework",
          "CoreMIDI.framework",
          "CoreMotion.framework",
          "CoreTelephony.framework",
          "CoreText.framework",
          "CoreVideo.framework",
          "EventKit.framework",
          "EventKitUI.framework",
          "ExternalAccessory.framework",
          "Foundation.framework",
          "GameController.framework",
          "GameKit.framework",
          "GLKit.framework",
          "GSS.framework",
          "iAd.framework",
          "ImageIO.framework",
          "IOKit.framework",
          "JavaScriptCore.framework",
          "MapKit.framework",
          "MediaAccessibility.framework",
          "MediaPlayer.framework",
          "MediaToolbox.framework",
          "MessageUI.framework",
          "MobileCoreServices.framework",
          "MultipeerConnectivity.framework",
          "NewsstandKit.framework",
          "OpenAL.framework",
          "OpenGLES.framework",
          "PassKit.framework",
          "QuartzCore.framework",
          "QuickLook.framework",
          "SafariServices.framework",
          "Security.framework",
          "Social.framework",
          "SpriteKit.framework",
          "StoreKit.framework",
          "SystemConfiguration.framework",
          "Twitter.framework",
          "UIKit.framework",
    };

    static string configPath
    {
        get
        {
            string currentFilePath = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
            return "Assets" + currentFilePath.Substring(0, currentFilePath.LastIndexOf(Path.DirectorySeparatorChar) + 1).Replace(Application.dataPath.Replace("/", Path.DirectorySeparatorChar.ToString()), string.Empty).Replace("\\", "/") + "config.projmods";
        }
    }
}
