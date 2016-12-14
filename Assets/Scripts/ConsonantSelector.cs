using UnityEngine;
using System.Collections;

public class ConsonantSelector : MonoBehaviour {

	float eulerTemp;
	int currentState, prevState;
	GameObject hand;
	Quaternion handRotation;

	[SerializeField, Tooltip ("It vibrates 320 units per second")] int hapticsLength = 24;
	[SerializeField, Range (0, 255)] byte hapticsStrength = 128;
	byte[] hapticsBytes = new byte[8];
	OVRHapticsClip hapticsClip;

	TextMesh[] vowels;
	TextMesh enteredText;

	void Start () {
		vowels = GameObject.Find ("Consonant").GetComponentsInChildren<TextMesh> ();
		hand = GameObject.Find ("Consonant Hand Parent");
		enteredText = GameObject.Find ("Entered Text").GetComponent<TextMesh> ();
		enteredText.text = "";

		//振動用のデータを作成
		hapticsBytes = new byte[hapticsLength];
		for (int i = 0; i < hapticsBytes.Length; i++) {
			hapticsBytes [i] = hapticsStrength;
		}
		hapticsClip = new OVRHapticsClip (hapticsBytes, hapticsBytes.Length);
	}

	void Update () {
		//現在の状態を判定
		eulerTemp = transform.rotation.eulerAngles.z;
		if (300 < eulerTemp && eulerTemp <= 324) {
			currentState = 4;
		} else if (324 < eulerTemp && eulerTemp <= 348) {
			currentState = 3;
		} else if (348 < eulerTemp && eulerTemp <= 360 || 0 < eulerTemp && eulerTemp <= 12) {
			currentState = 2;
		} else if (12 < eulerTemp && eulerTemp <= 36) {
			currentState = 1;
		} else if (36 < eulerTemp && eulerTemp <= 60) {
			currentState = 0;
		}

		//子音パネルの色を変える
		vowels [currentState].color = new Color (255, 0, 0);
		if (currentState != prevState) {
			OVRHaptics.LeftChannel.Mix (hapticsClip);
			vowels [prevState].color = new Color (255, 255, 255);
		}

		//ステート管理
		prevState = currentState;
		handRotation = hand.transform.rotation;
		handRotation.z = transform.rotation.z;
		hand.transform.rotation = handRotation;
	}
}
