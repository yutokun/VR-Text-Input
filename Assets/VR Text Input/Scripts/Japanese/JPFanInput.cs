﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.XR;

public class JPFanInput : MonoBehaviour {

	[SerializeField]JPTextHandler textHandler;
	KanjiConverter kanji;

	//hand は針
	[SerializeField] GameObject handParent, upperVariationParent, lowerVariationParent;
	TextMesh[] selectorTexts, upperTexts, lowerTexts;

	//振動データ
	OVRHapticsClip hapticsClip;

	//針の位置
	int currentHandPosition, prevPosition;

	//テキスト入力部分
	bool afterVariationEntered;
	int consonantIndex;

	public Color baseColor = new Color (255, 255, 255), highlightColor = new Color (255, 0, 0);

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

	string[,] upperVariations = new string[10, 5] {
		{ "ぁ", "ぃ", "ぅ", "ぇ", "ぉ" },
		{ "が", "ぎ", "ぐ", "げ", "ご" },
		{ "ざ", "じ", "ず", "ぜ", "ぞ" },
		{ "だ", "ぢ", "づ", "で", "ど" },
		{ "", "", "", "", "" },
		{ "ば", "び", "ぶ", "べ", "ぼ" },
		{ "", "", "", "", "" },
		{ "ゃ", "「", "ゅ", "」", "ょ" },
		{ "", "", "", "", "" },
		{ "ゎ", "！", "？", "、", "。" }
	};

	string[,] lowerVariations = new string[10, 5] {
		{ "", "", "ゔ", "", "" },
		{ "", "", "", "", "" },
		{ "", "", "", "", "" },
		{ "", "", "っ", "", "" },
		{ "", "", "", "", "" },
		{ "ぱ", "ぴ", "ぷ", "ぺ", "ぽ" },
		{ "", "", "", "", "" },
		{ "", "【", "", "】", "" },
		{ "", "", "", "", "" },
		{ "", "", "…", "/", "" }
	};

	void Start () {
		//入力候補欄への参照を取得
		selectorTexts = GetComponentsInChildren<TextMesh> ();
		upperTexts = upperVariationParent.GetComponentsInChildren<TextMesh> ();
		lowerTexts = lowerVariationParent.GetComponentsInChildren<TextMesh> ();

		kanji = FindObjectOfType<KanjiConverter> ();

		//バリエーションを非表示
		DisableVariations ();

		//振動用のデータを作成
		byte[] hapticsBytes = new byte[4];
		for (int i = 0; i < hapticsBytes.Length; i++) {
			hapticsBytes [i] = 128;
		}
		hapticsClip = new OVRHapticsClip (hapticsBytes, hapticsBytes.Length);
	}

	void Update () {
		//針の状態を判定
		float eulerTemp = InputTracking.GetLocalRotation (XRNode.RightHand).eulerAngles.z;
		if (300 < eulerTemp && eulerTemp <= 324) {
			currentHandPosition = 4;
		} else if (324 < eulerTemp && eulerTemp <= 348) {
			currentHandPosition = 3;
		} else if (348 < eulerTemp || eulerTemp <= 12) {
			currentHandPosition = 2;
		} else if (eulerTemp <= 36) {
			currentHandPosition = 1;
		} else if (eulerTemp <= 60) {
			currentHandPosition = 0;
		}

		//パネルの色を変えて振動させる
		if (kanji.isConverting == false && currentHandPosition != prevPosition) {
			OVRHaptics.RightChannel.Mix (hapticsClip);
			selectorTexts [prevPosition].color = baseColor;
			selectorTexts [currentHandPosition].color = highlightColor;
		}

		//針の位置管理
		prevPosition = currentHandPosition;

		//針の回転管理
		Vector3 handEulerAngles = handParent.transform.eulerAngles;
		handEulerAngles.z = eulerTemp;
		handParent.transform.eulerAngles = handEulerAngles;

		//主に可読性のためにOVRInput系をキャッシュ
		bool RIndex_Down = OVRInput.GetDown (OVRInput.RawButton.RIndexTrigger);
		bool RIndex_Hold = OVRInput.Get (OVRInput.RawButton.RIndexTrigger);
		bool RIndex_Up = OVRInput.GetUp (OVRInput.RawButton.RIndexTrigger);
		bool RHand_Down = OVRInput.GetDown (OVRInput.RawButton.RHandTrigger);
		bool RHand_Hold = OVRInput.Get (OVRInput.RawButton.RHandTrigger);
		bool RHand_Up = OVRInput.GetUp (OVRInput.RawButton.RHandTrigger);
		bool RThumbstickUp_Down = OVRInput.GetDown (OVRInput.RawButton.RThumbstickUp);
		bool RThumbstickDown_Down = OVRInput.GetDown (OVRInput.RawButton.RThumbstickDown);

		//入力処理
		if (RIndex_Down) {
			//子音のセットを読んで入力文字を切り替え
			consonantIndex = currentHandPosition;

			//右中指を押している場合ははまやらわの母音セットに切り替え
			if (RHand_Hold) {
				currentHandPosition += 5;
				consonantIndex += 5;
			}

			//押下で母音リストに切り替え
			for (int i = 0; i < 5; i++) {
				selectorTexts [i].text = jpChars [currentHandPosition, i];
			}

			//バリエーションを表示
			EnableVariation (currentHandPosition);

		} else if (RIndex_Up) {
			//離して文字を入力
			if (afterVariationEntered == false) {
				textHandler.SendChar (selectorTexts [currentHandPosition].text);
			}
			afterVariationEntered = false;

			//バリエーションを非表示
			DisableVariations ();

			//子音に戻す
			if (RHand_Hold) {
				for (int i = 0; i < 5; i++)
					selectorTexts [i].text = jpChars [i + 5, 0];
			} else {
				for (int i = 0; i < 5; i++)
					selectorTexts [i].text = jpChars [i, 0];
			}
		}

		//右人差し指が押されており、
		if (RIndex_Hold) {
			//かつ右親指が上下されたときにバリエーション入力
			if (RThumbstickUp_Down) {
				//上部バリエーションの入力
				textHandler.SendChar (upperVariations [consonantIndex, currentHandPosition]);
				afterVariationEntered = true;
			} else if (RThumbstickDown_Down) {
				//下部バリエーションの入力
				textHandler.SendChar (lowerVariations [consonantIndex, currentHandPosition]);
				afterVariationEntered = true;
			}
		}

		//中指で子音のセットを切り替え
		if (RHand_Up && !RIndex_Hold) {
			for (int i = 0; i < 5; i++) {
				selectorTexts [i].text = jpChars [i, 0];
			}
		} else if (RHand_Down && !RIndex_Hold) {
			for (int i = 0; i < 5; i++) {
				selectorTexts [i].text = jpChars [i + 5, 0];
			}
		}
	}

	//バリエーションを表示
	public void EnableVariation (int currentPosition) {
		for (int i = 0; i < 5; i++) {
			upperTexts [i].text = upperVariations [currentPosition, i];
			lowerTexts [i].text = lowerVariations [currentPosition, i];
		}
	}

	//バリエーションを非表示
	void DisableVariations () {
		for (int i = 0; i < 5; i++) {
			upperTexts [i].text = "";
			lowerTexts [i].text = "";
		}
	}
}
