using UnityEngine;
using System.Collections;

public class JapaneseInputHandler : MonoBehaviour {

	//hand は針
	[SerializeField] GameObject handParent, selectorObj, controller, upperVarObj, lowerVerObj;
	TextMesh[] selectors, upperVars, lowerVars;

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

	//テスト
	[SerializeField] Animation testAnim;

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
		{ "ゎ", "", "？", "、", "。" }
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
		selectors = selectorObj.GetComponentsInChildren<TextMesh> ();
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

		//パネルの色を変えて振動させる
		selectors [currentPosition].color = new Color (255, 0, 0);
//		selectors [currentPosition].fontSize = 200;
		if (currentPosition != prevPosition) {
			OVRHaptics.RightChannel.Mix (hapticsClip);
			selectors [prevPosition].color = new Color (255, 255, 255);
//			selectors [prevPosition].fontSize = 150;
			testAnim.Play ();
		}

		//ステート管理
		prevPosition = currentPosition;
		handRotation = handParent.transform.rotation;
		handRotation.z = controller.transform.rotation.z;
		handParent.transform.rotation = handRotation;

		//入力処理
		if (OVRInput.GetDown (OVRInput.RawButton.RIndexTrigger)) {
			//子音のセットを読んで入力文字を切り替え
			consonantIndex = currentPosition;
			if (consonantIsSecond) {
				currentPosition += 5;
				consonantIndex += 5;
			}
			//押下で子音に切り替え
			for (int i = 0; i < 5; i++) {
				selectors [i].text = jpChars [currentPosition, i];
				//バリエーションを表示
				upperVars [i].text = jpCharsUpperVariation [currentPosition, i];
				lowerVars [i].text = jpCharsLowerVariation [currentPosition, i];
			}
		} else if (OVRInput.GetUp (OVRInput.RawButton.RIndexTrigger)) {
			//離して文字を入力
			if (variationEntered == false) {
				enteredText.text += selectors [currentPosition].text;
			}
			variationEntered = false;

			//バリエーションを非表示
			for (int i = 0; i < 5; i++) {
				upperVars [i].text = "";
				lowerVars [i].text = "";
			}

			//子音に戻す
			if (OVRInput.Get (OVRInput.RawButton.RHandTrigger)) {
				for (int i = 0; i < 5; i++)
					selectors [i].text = jpConsonantsChars [1, i];
			} else {
				for (int i = 0; i < 5; i++)
					selectors [i].text = jpConsonantsChars [0, i];
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
		if (OVRInput.GetUp (OVRInput.RawButton.RHandTrigger) && !OVRInput.Get (OVRInput.RawButton.RIndexTrigger)) {
			consonantIsSecond = false;
			for (int i = 0; i < 5; i++) {
				selectors [i].text = jpConsonantsChars [0, i];
			}
		} else if (OVRInput.GetDown (OVRInput.RawButton.RHandTrigger) && !OVRInput.Get (OVRInput.RawButton.RIndexTrigger)) {
			consonantIsSecond = true;
			for (int i = 0; i < 5; i++) {
				selectors [i].text = jpConsonantsChars [1, i];
			}
		}
	}
}
