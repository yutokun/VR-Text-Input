using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.XR;

public class ENSlideInput : MonoBehaviour {

	[SerializeField] Transform anchor, frame, focusFrame;
	[SerializeField] Transform[] parents;
	TextMesh[] [] textMeshes;
	[SerializeField] float distance;
	Transform focusTarget;
	RaycastHit hit;
	string inputCache;

	//Vibration data.
	OVRHapticsClip hapticsClip;

	ENTextHandler textHandler;

	string[,] chars = new string[4, 12] {
		{ "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "-", "=" },
		{ "q", "w", "e", "r", "t", "y", "u", "i", "o", "p", "[", "]" },
		{ "a", "s", "d", "f", "g", "h", "j", "k", "l", ";", "'", "~" },
		{ "z", "x", "c", "v", "b", "n", "m", ",", ".", "/", "?", "!" }
	};

	string[,] shiftedChars = new string[4, 12] {
		{ "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "-", "+" },
		{ "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P", "{", "}" },
		{ "A", "S", "D", "F", "G", "H", "J", "K", "L", ":", "'", "~" },
		{ "Z", "X", "C", "V", "B", "N", "M", "<", ">", "?", "?", "!" }
	};

	bool Hand_Down {
		get {
			if (OVRInput.GetDown (OVRInput.RawButton.LHandTrigger) || OVRInput.GetDown (OVRInput.RawButton.RHandTrigger)) {
				return true;
			} else {
				return false;
			}
		}
	}

	bool Hand_Up {
		get {
			if (OVRInput.GetUp (OVRInput.RawButton.LHandTrigger) || OVRInput.GetUp (OVRInput.RawButton.RHandTrigger)) {
				return true;
			} else {
				return false;
			}
		}
	}

	bool LThumbstick_Down {
		get {
			if (OVRInput.GetDown (OVRInput.RawButton.LThumbstick)) {
				return true;
			} else {
				return false;
			}
		}
	}

	bool RThumbstick_Down {
		get {
			if (OVRInput.GetDown (OVRInput.RawButton.RThumbstick)) {
				return true;
			} else {
				return false;
			}
		}
	}

	bool AorXButton_Down {
		get {
			if (OVRInput.GetDown (OVRInput.RawButton.A) || OVRInput.GetDown (OVRInput.RawButton.X)) {
				return true;
			} else {
				return false;
			}
		}
	}

	void Start () {
		// Get the References.
		textMeshes = new TextMesh[parents.Length][];
		for (int i = 0; i < parents.Length; i++) {
			textMeshes [i] = parents [i].GetComponentsInChildren<TextMesh> ();
		}

		textHandler = GetComponent<ENTextHandler> ();

		// Create a haptic data.
		byte[] hapticsBytes = new byte[4];
		for (int i = 0; i < hapticsBytes.Length; i++) {
			hapticsBytes [i] = 128;
		}
		hapticsClip = new OVRHapticsClip (hapticsBytes, hapticsBytes.Length);

		//Initialize state.
		Reposition ();
		HandButtonDown ();
	}

	void Update () {
		// Update position and rotation of transform.
		// Y position.
		Vector3 pos = transform.position;
		pos.y -= Mathf.DeltaAngle (0, InputTracking.GetLocalRotation (XRNode.RightHand).eulerAngles.x) / 200f;
		anchor.position = pos;
		// Y rotation.
		var euler = Vector3.zero;
		euler.y = InputTracking.GetLocalRotation (XRNode.RightHand).eulerAngles.y;
		anchor.eulerAngles = euler;

		// Change character set.
		if (Hand_Up) {
			HandButtonUp ();
		} else if (Hand_Down) {
			HandButtonDown ();
		}

		// Focusing and caching a character.
		Ray ray = new Ray (transform.position, transform.forward);
		if (Physics.Raycast (ray, out hit) && hit.collider.CompareTag ("English Character")) {
			focusTarget = hit.transform;
			inputCache = focusTarget.GetComponent<TextMesh> ().text;
		}
		try {
			focusFrame.position = focusTarget.position;
		} catch {
		}

		// Entering a character.
		if (AorXButton_Down)
			textHandler.Send (inputCache);
		
		// Reposition.
		if (LThumbstick_Down)
			Reposition ();
	}

	void Reposition () {
		// Reset rotation.
		var euler = Vector3.zero;
		euler.y = InputTracking.GetLocalRotation (XRNode.RightHand).eulerAngles.y;
		transform.eulerAngles = euler;

		// Reset position.
		// Get and set a world position of the right hand.
		transform.position = Camera.main.transform.root.position + InputTracking.GetLocalPosition (XRNode.RightHand);
		foreach (var item in parents) {
			var pos = Vector3.zero;
			pos.x = item.localPosition.x;
			pos.y = item.localPosition.y;
			pos.z = distance;
			item.localPosition = pos;
		}
		frame.localPosition = new Vector3 (0, 0, distance);
	}

	void HandButtonUp () {
		for (int i = 0; i < 4; i++) {
			for (int j = 0; j < textMeshes [i].Length; j++) {
				textMeshes [i] [j].text = chars [i, j];
			}
		}
	}

	void HandButtonDown () {
		for (int i = 0; i < 4; i++) {
			for (int j = 0; j < textMeshes [i].Length; j++) {
				textMeshes [i] [j].text = shiftedChars [i, j];
			}
		}
	}
}