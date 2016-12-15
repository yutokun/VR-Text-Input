using UnityEngine;
using System.Collections;

public class JapaneseInputHandler : MonoBehaviour {

	//hand は針
	[SerializeField] GameObject handParent, consonantObj, vowelObj, controller, upperVarObj, lowerVerObj;
	TextMesh[] consonants, vowels, upperVars, lowerVars;

	//Touch の振動強度・長さ調整
	//[Header ("Length and Strength of Oculus Touch")]
	[Header ("Touch の振動する長さと強度を調整")]
	[SerializeField, Tooltip ("It vibrates 320 times per second.")] int hapticsLength = 4;
	[SerializeField, Range (0, 255)] byte hapticsStrength = 128;
	byte[] hapticsBytes;
	OVRHapticsClip hapticsClip;

	//針の状態を管理
	float eulerTemp;
	int currentPosition, prevPosition;
	Quaternion handRotation;

	//テキスト入力部分
	bool consonantIsSecond = false, variationEntered;
	int consonantIndex;
	TextMesh enteredText;

	string[,] jpChars = new string[10, 5] {
		{ "あ", "い", "う", "え", "お" },
		{ "か", "き", "く", "け", "こ" },
		{ "さ", "し", "す", "せ", "そ" },
		{ "た", "ち", "つ", "て", "と" },
		{ "な", "に", "ぬ", "ね", "の" },
		{ "は", "ひ", "ふ", "へ", "ほ" },
		{ "ま", "み", "む", "め", "も" },
		{ "や", "（", "ゆ", "）", "よ" },
		{ "ら", "り", "る", "れ", "ろ" },
		{ "わ", "ー", "を", "～", "ん" }
	};

	string[,] jpCharsUpperVariation = new string[10, 5] {
		{ "ぁ", "ぃ", "ぅ", "ぇ", "ぉ" },
		{ "が", "ぎ", "ぐ", "げ", "ご" },
		{ "ざ", "じ", "ず", "ぜ", "ぞ" },
		{ "だ", "ぢ", "づ", "で", "ど" },
		{ "", "", "", "", "" },
		{ "ば", "び", "ぶ", "べ", "ぼ" },
		{ "", "", "", "", "" },
		{ "ゃ", "「", "ゅ", "」", "ょ" },
		{ "", "", "", "", "" },
		{ "ゎ", "", "、", "", "。" }
	};

	string[,] jpCharsLowerVariation = new string[10, 5] {
		{ "", "", "", "", "" },
		{ "", "", "", "", "" },
		{ "", "", "", "", "" },
		{ "", "", "っ", "", "" },
		{ "", "", "", "", "" },
		{ "ぱ", "ぴ", "ぷ", "ぺ", "ぽ" },
		{ "", "", "", "", "" },
		{ "", "【", "", "】", "" },
		{ "", "", "", "", "" },
		{ "", "", "", "", "" }
	};

	string[,] jpConsonantsChars = new string[2, 5] {
		{ "あ", "か", "さ", "た", "な" },
		{ "は", "ま", "や", "ら", "わ" }
	};

	void Start () {
		enteredText = GetComponent<TextMesh> ();
		enteredText.text = "";
		consonants = consonantObj.GetComponentsInChildren<TextMesh> ();
		vowels = vowelObj.GetComponentsInChildren<TextMesh> ();
		upperVars = upperVarObj.GetComponentsInChildren<TextMesh> ();
		lowerVars = lowerVerObj.GetComponentsInChildren<TextMesh> ();

		//バリエーションを非表示
		for (int i = 0; i < 5; i++) {
			upperVars [i].text = "";
			lowerVars [i].text = "";
		}

		//振動用のデータを作成
		hapticsBytes = new byte[hapticsLength];
		for (int i = 0; i < hapticsBytes.Length; i++) {
			hapticsBytes [i] = hapticsStrength;
		}
		hapticsClip = new OVRHapticsClip (hapticsBytes, hapticsBytes.Length);
	}

	void Update () {
		//針の状態を判定
		eulerTemp = controller.transform.rotation.eulerAngles.z;
		if (300 < eulerTemp && eulerTemp <= 324) {
			currentPosition = 4;
		} else if (324 < eulerTemp && eulerTemp <= 348) {
			currentPosition = 3;
		} else if (348 < eulerTemp && eulerTemp <= 360 || 0 < eulerTemp && eulerTemp <= 12) {
			currentPosition = 2;
		} else if (12 < eulerTemp && eulerTemp <= 36) {
			currentPosition = 1;
		} else if (36 < eulerTemp && eulerTemp <= 60) {
			currentPosition = 0;
		}

		FeedbackHaptics ();

		//入力処理
		if (OVRInput.GetDown (OVRInput.RawButton.RIndexTrigger) || OVRInput.GetDown (OVRInput.RawButton.A)) {
			//子音のセットを読んで入力文字を切り替え
			consonantIndex = currentPosition;
			if (consonantIsSecond) {
				currentPosition += 5;
				consonantIndex += 5;
			}
			//押下で子音に切り替え
			for (int i = 0; i < 5; i++) {
				vowels [i].text = jpChars [currentPosition, i];
				//バリエーションを表示
				upperVars [i].text = jpCharsUpperVariation [currentPosition, i];
				lowerVars [i].text = jpCharsLowerVariation [currentPosition, i];
			}
			consonantObj.SetActive (false);
			vowelObj.SetActive (true);
		} else if (OVRInput.GetUp (OVRInput.RawButton.RIndexTrigger) || OVRInput.GetUp (OVRInput.RawButton.A)) {
			//離して文字を入力
			//EnterCharacter ();
			if (variationEntered == false) {
				enteredText.text += vowels [currentPosition].text;
			}
			consonantObj.SetActive (true);
			vowelObj.SetActive (false);
			variationEntered = false;

			//バリエーションを非表示
			for (int i = 0; i < 5; i++) {
				upperVars [i].text = "";
				lowerVars [i].text = "";
			}
		} else if (OVRInput.GetDown (OVRInput.RawButton.B) && enteredText.text.Length != 0) {
			//Bで最後の文字を削除
			enteredText.text = enteredText.text.Remove (enteredText.text.Length - 1, 1);
		} else if (OVRInput.GetDown (OVRInput.RawButton.RThumbstickUp)) {
			//上部バリエーションの入力
			enteredText.text += jpCharsUpperVariation [consonantIndex, currentPosition];
			variationEntered = true;
		} else if (OVRInput.GetDown (OVRInput.RawButton.RThumbstickDown)) {
			//下部バリエーションの入力
			enteredText.text += jpCharsLowerVariation [consonantIndex, currentPosition];
			variationEntered = true;
		}

		//文字数が19以上の場合は最初の1文字を削除
		if (enteredText.text.Length > 19) {
			enteredText.text = enteredText.text.Remove (0, 1);
		}

		//子音のセットを切り替え
		if (OVRInput.GetUp (OVRInput.RawButton.RHandTrigger)) {
			consonantIsSecond = false;
			for (int i = 0; i < 5; i++) {
				consonants [i].text = jpConsonantsChars [0, i];
			}
		} else if (OVRInput.GetDown (OVRInput.RawButton.RHandTrigger)) {
			consonantIsSecond = true;
			for (int i = 0; i < 5; i++) {
				consonants [i].text = jpConsonantsChars [1, i];
			}
		}
	}

	/// <summary>
	/// Oculus Touch の振動とパネルの変化によるフィードバックを行います。
	/// 毎フレーム呼び出す必要があります。
	/// ToDo:子音と母音の処理を1つにまとめる。
	/// </summary>
	void FeedbackHaptics () {
		//子音パネルの色を変えて振動させる
		consonants [currentPosition].color = new Color (255, 0, 0);
		if (currentPosition != prevPosition) {
			OVRHaptics.RightChannel.Mix (hapticsClip);
			consonants [prevPosition].color = new Color (255, 255, 255);
		}

		//母音パネルの色を変えて振動させる
		vowels [currentPosition].color = new Color (255, 0, 0);
		if (currentPosition != prevPosition) {
			vowels [prevPosition].color = new Color (255, 255, 255);
		}

		//ステート管理
		prevPosition = currentPosition;
		handRotation = handParent.transform.rotation;
		handRotation.z = controller.transform.rotation.z;
		handParent.transform.rotation = handRotation;
	}

	/// <summary>
	/// 文字列を入力します。
	/// </summary>
	//	void EnterCharacter () {
	//		enteredText.text += vowels [currentPosition].text;
	//
	//		//文字数が19以上の場合は最初の1文字を削除
	//		if (enteredText.text.Length > 19) {
	//			enteredText.text = enteredText.text.Remove (0, 1);
	//		}
	//	}
}
