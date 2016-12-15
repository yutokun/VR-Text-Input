using UnityEngine;
using System.Collections;

public class JapaneseInputHandler : MonoBehaviour {

	//hand は針
	[SerializeField] GameObject handParent, consonantObj, vowelObj, controller;
	TextMesh[] consonants, vowels;

	//Touch の振動強度・長さ調整
	//[Header ("Length and Strength of Oculus Touch")]
	[Header ("Touch の振動する長さと強度を調整")]
	[SerializeField, Tooltip ("It vibrates 320 times per second.")] int hapticsLength = 4;
	[SerializeField, Range (0, 255)] byte hapticsStrength = 128;
	byte[] hapticsBytes;
	OVRHapticsClip hapticsClip;

	//針の状態を管理
	float eulerTemp;
	int currentState, prevState;
	Quaternion handRotation;

	//テキスト入力部分
	bool consonantIsSecond = false;
	TextMesh enteredText;

	void Start () {
		enteredText = GetComponent<TextMesh> ();
		enteredText.text = "";
		consonants = consonantObj.GetComponentsInChildren<TextMesh> ();
		vowels = vowelObj.GetComponentsInChildren<TextMesh> ();

		//振動用のデータを作成
		hapticsBytes = new byte[hapticsLength];
		for (int i = 0; i < hapticsBytes.Length; i++) {
			hapticsBytes [i] = hapticsStrength;
		}
		hapticsClip = new OVRHapticsClip (hapticsBytes, hapticsBytes.Length);
	}

	void Update () {
		//現在の状態を判定
		eulerTemp = controller.transform.rotation.eulerAngles.z;
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

		FeedbackHaptics ();

		//入力処理
		if (OVRInput.GetDown (OVRInput.RawButton.RIndexTrigger) || OVRInput.GetDown (OVRInput.RawButton.A)) {
			//子音のセットを読んで入力文字を切り替え
			if (consonantIsSecond) {
				currentState += 5;
			}
			//押下で子音に切り替え
			for (int i = 0; i < vowels.Length; i++) {
				vowels [i].text = jpChars [currentState, i];
			}
			consonantObj.SetActive (false);
			vowelObj.SetActive (true);
		} else if (OVRInput.GetUp (OVRInput.RawButton.RIndexTrigger) || OVRInput.GetUp (OVRInput.RawButton.A)) {
			//離して文字を入力
			EnterCharacter ();
			consonantObj.SetActive (true);
			vowelObj.SetActive (false);
		} else if (OVRInput.GetDown (OVRInput.RawButton.B) && enteredText.text.Length != 0) {
			//Bで最後の文字を削除
			enteredText.text = enteredText.text.Remove (enteredText.text.Length - 1, 1);
		}

		//子音のセットを切り替え
		if (OVRInput.GetUp (OVRInput.RawButton.RHandTrigger)) {
			consonantIsSecond = false;
			for (int i = 0; i < consonants.Length; i++) {
				consonants [i].text = jpConsonantsChars [0, i];
			}
		} else if (OVRInput.GetDown (OVRInput.RawButton.RHandTrigger)) {
			consonantIsSecond = true;
			for (int i = 0; i < consonants.Length; i++) {
				consonants [i].text = jpConsonantsChars [1, i];
			}
		}
	}

	string[,] jpChars = new string[10, 5] {
		{ "あ", "い", "う", "え", "お" },
		{ "か", "き", "く", "け", "こ" },
		{ "さ", "し", "す", "せ", "そ" },
		{ "た", "ち", "つ", "て", "と" },
		{ "な", "に", "ぬ", "ね", "の" },
		{ "は", "ひ", "ふ", "へ", "ほ" },
		{ "ま", "み", "む", "め", "も" },
		{ "や", "　", "ゆ", "　", "よ" },
		{ "ら", "り", "る", "れ", "ろ" },
		{ "わ", "　", "を", "　", "ん" }
	};

	string[,] jpConsonantsChars = new string[2, 5] {
		{ "あ", "か", "さ", "た", "な" },
		{ "は", "ま", "や", "ら", "わ" }
	};

	/// <summary>
	/// Oculus Touch の振動とパネルの変化によるフィードバックを行います。
	/// 毎フレーム呼び出す必要があります。
	/// ToDo:子音と母音の処理を1つにまとめる。
	/// </summary>
	void FeedbackHaptics () {
		//子音パネルの色を変えて振動させる
		consonants [currentState].color = new Color (255, 0, 0);
		if (currentState != prevState) {
			OVRHaptics.RightChannel.Mix (hapticsClip);
			consonants [prevState].color = new Color (255, 255, 255);
		}

		//母音パネルの色を変えて振動させる
		vowels [currentState].color = new Color (255, 0, 0);
		if (currentState != prevState) {
			vowels [prevState].color = new Color (255, 255, 255);
		}

		//ステート管理
		prevState = currentState;
		handRotation = handParent.transform.rotation;
		handRotation.z = controller.transform.rotation.z;
		handParent.transform.rotation = handRotation;
	}

	/// <summary>
	/// 文字列を入力します。
	/// </summary>
	void EnterCharacter () {
		enteredText.text += vowels [currentState].text;
		//文字数が19以上の場合は最初の1文字を削除
		if (enteredText.text.Length > 19) {
			enteredText.text = enteredText.text.Remove (0, 1);
		}
	}
}
