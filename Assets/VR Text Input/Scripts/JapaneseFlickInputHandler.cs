using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class JapaneseFlickInputHandler : MonoBehaviour {

	GameObject hand;
	RaycastHit hit;

	//入力対象文字のキャッシュ
	string inputCache;

	//振動データ
	OVRHapticsClip hapticsClip;

	TextHandler textHandler;
	KanjiConverter kanji;

	void Start () {
		hand = GameObject.Find ("RightHandAnchor");
		textHandler = FindObjectOfType<TextHandler> ();
		kanji = FindObjectOfType<KanjiConverter> ();

		//振動用のデータを作成
		byte[] hapticsBytes = new byte[4];
		for (int i = 0; i < hapticsBytes.Length; i++) {
			hapticsBytes [i] = 128;
		}
		hapticsClip = new OVRHapticsClip (hapticsBytes, hapticsBytes.Length);
	}

	void Update () {

		//手に合わせて入力システムを移動
//		Vector3 pos = hand.transform.position;
//		transform.position = pos;
//		transform.Translate (hand.transform.forward);


		//振動させる
//		if (kanji.isConverting == false && currentHandPosition != prevPosition) {
//			OVRHaptics.RightChannel.Mix (hapticsClip);
//			selectorTexts [prevPosition].color = baseColor;
//			selectorTexts [currentHandPosition].color = highlightColor;
//		}

		//主に可読性のためにOVRInput系をキャッシュ
		bool RIndex_Down = OVRInput.GetDown (OVRInput.RawButton.RIndexTrigger);
		bool RHand_Down = OVRInput.GetDown (OVRInput.RawButton.RHandTrigger);
		bool RHand_Up = OVRInput.GetUp (OVRInput.RawButton.RHandTrigger);
		bool RThumbstickUp_Down = OVRInput.Get (OVRInput.RawButton.RThumbstickUp);
		bool RThumbstickDown_Down = OVRInput.Get (OVRInput.RawButton.RThumbstickDown);
		bool RThumbstickLeft_Down = OVRInput.Get (OVRInput.RawButton.RThumbstickLeft);
		bool RThumbstickRight_Down = OVRInput.Get (OVRInput.RawButton.RThumbstickRight);
		bool RThumbstickTouch_Down = OVRInput.GetDown (OVRInput.RawTouch.RThumbstick);
		bool RThumbstickTouch_Up = OVRInput.GetUp (OVRInput.RawTouch.RThumbstick);

		//iueo 母音を表示、他の母音を非表示にする。または戻す。
		Ray ray = new Ray (hand.transform.position, hand.transform.forward);
		Physics.Raycast (ray, out hit, 10);
		if (hit.collider.tag == "Keyboard") {
			TextMesh target = hit.collider.gameObject.GetComponentInChildren<TextMesh> ();
			inputCache = target.text;
			if (RThumbstickLeft_Down) {
				inputCache = "い";
			} else if (RThumbstickUp_Down) {
				inputCache = "う";
			} else if (RThumbstickRight_Down) {
				inputCache = "え";
			} else if (RThumbstickDown_Down) {
				inputCache = "お";
			}
		}

		TextMesh[] targets = GetComponentsInChildren<TextMesh> ();
		foreach (TextMesh item in targets) {
			if (item == hit.collider.gameObject.GetComponentInChildren<TextMesh> ()) {
				item.color = Color.red;
			} else {
				item.color = Color.white;
			}
		}

		//入力
		if (RIndex_Down) {
			// TODO: 漢字変換に送るように変更
			textHandler.SendChar (inputCache);
		}
	}
}
