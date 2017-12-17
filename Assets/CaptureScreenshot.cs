using UnityEngine;
using System.Collections;

public class CaptureScreenshot : MonoBehaviour {
	
	void Update () {
		if (Input.GetKeyDown (KeyCode.Space)) {
			ScreenCapture.CaptureScreenshot ("image.png");
		}
	}
}
