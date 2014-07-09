using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;

namespace UnityEditor.XCodeEditor
{
	public class XCodePostProcess
	{
	    [PostProcessBuild]
	    public static void OnPostProcessBuild( BuildTarget target, string path )
	    {
	        // Create a new project object from build target
	        XCProject project = new XCProject( path );

	        // Find and run through all projmods files to patch the project
	        var files = Directory.GetFiles( Application.dataPath, "*.projmods", SearchOption.AllDirectories );
	        foreach( var file in files ) {
	            project.ApplyMod( file );
	        }

	        // Finally save the xcode project
	        project.Save();
	    }
	}
}