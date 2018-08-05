using UnityEngine;
using System.Collections;
using UnityEditor;

public class CaptureScreenshot : MonoBehaviour {

	int current = 1;

	string Platform {
		get { return EditorUserBuildSettings.activeBuildTarget.ToString ().Contains ("Standalone") ? "rift-" : "mobile-"; }
	}

	void Update () {
		if (Input.GetKeyDown (KeyCode.Space)) {
			ScreenCapture.CaptureScreenshot (Platform + "image" + current + ".png");
			Debug.Log (Application.platform.ToString ());
		}
	}
}
