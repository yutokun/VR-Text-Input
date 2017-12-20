using UnityEngine;
using UnityEngine.Events;

public class ENTextHandler : MonoBehaviour {

	[System.Serializable]
	public class OnENInput : UnityEvent<string> {
	}

	public OnENInput onEnInput;
	public UnityEvent onBackspace;

	void Update () {
		//Delete last character.
		if (OVRInput.GetDown (OVRInput.RawButton.B) || OVRInput.GetDown (OVRInput.RawButton.Y)) {
			onBackspace.Invoke ();
		}
	}

	/// <summary>
	/// Send a letter to UnityEvent onEnInput().
	/// </summary>
	/// <param name="str">String.</param>
	public void Send (string str) {
		onEnInput.Invoke (str);
	}
}
