using UnityEngine;
using UnityEngine.XR;

public class CameraInitializer : MonoBehaviour {

	void Start () {
		XRSettings.eyeTextureResolutionScale = 1.25f;
		OVRManager.tiledMultiResLevel = OVRManager.TiledMultiResLevel.LMSHigh;
		OVRManager.display.displayFrequency = 72.0f;
	}
}
