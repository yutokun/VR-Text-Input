using UnityEngine;

public class OculusEntitlementChecker : MonoBehaviour {

	void Awake() {
		var textMesh = GetComponentInChildren<TextMesh>();

		try {
			Oculus.Platform.Core.AsyncInitialize("2224852007532610");
			Oculus.Platform.Entitlements.IsUserEntitledToApplication().OnComplete(message => {
				if (message.IsError) {
					Debug.LogError("You are NOT entitled to use this app.");
					textMesh.text += "You are NOT entitled to use this app.\n";
					// Application.Quit();
				} else {
					Debug.Log("You are entitled to use this app.");
					textMesh.text += "You are entitled to use this app.\n";
				}
			});
		} catch (System.Exception ex) {
			Debug.LogError("Platform failed to initialize due to exception.");
			textMesh.text += "Platform failed to initialize due to exception.\n";
			Debug.LogException(ex);
			textMesh.text += ex.ToString() + "\n";
			// Application.Quit();
		}
	}
}