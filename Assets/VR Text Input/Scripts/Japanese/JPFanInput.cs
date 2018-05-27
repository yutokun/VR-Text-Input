using UnityEngine;
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

	//効果音
	[SerializeField] AudioSource audio;
	[SerializeField] AudioClip click, swipe;

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

	bool latestIsFirstSet;

	bool IsFirstSet {
		get {
			#if UNITY_STANDALONE
			return OVRInput.Get (OVRInput.RawButton.RHandTrigger);
			#elif UNITY_ANDROID
			return selectorTexts [0].text.Contains ("あ");
			#endif
		}
	}

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
			#if UNITY_STANDALONE
			OVRHaptics.RightChannel.Mix (hapticsClip);
			#endif
			selectorTexts [prevPosition].color = baseColor;
			selectorTexts [currentHandPosition].color = highlightColor;
			audio.PlayOneShot (click);
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
		bool RHand_Down = OVRInput.GetDown (OVRInput.RawButton.RHandTrigger) || OVRInput.GetDown (OVRInput.RawTouch.RTouchpad);
		bool RHand_Up = OVRInput.GetUp (OVRInput.RawButton.RHandTrigger) || OVRInput.GetUp (OVRInput.RawTouch.RTouchpad);
		bool RThumbstickUp_Down = OVRInput.GetDown (OVRInput.RawButton.RThumbstickUp) || OVRInput.GetDown (OVRInput.Button.DpadUp);
		bool RThumbstickDown_Down = OVRInput.GetDown (OVRInput.RawButton.RThumbstickDown) || OVRInput.GetDown (OVRInput.Button.DpadDown);

		//入力処理
		if (RIndex_Down) {
			//子音のセットを読んで入力文字を切り替え
			consonantIndex = currentHandPosition;

			//右中指を押している場合ははまやらわの母音セットに切り替え
			if (IsFirstSet == false) {
				currentHandPosition += 5;
				consonantIndex += 5;
			}

			//押下で母音リストに切り替え
			for (int i = 0; i < 5; i++) {
				selectorTexts [i].text = jpChars [currentHandPosition, i];
			}

			//バリエーションを表示
			EnableVariation (currentHandPosition);

			//効果音再生
			audio.PlayOneShot (click);

		} else if (RIndex_Up) {
			//離して文字を入力
			if (afterVariationEntered == false) {
				textHandler.SendChar (selectorTexts [currentHandPosition].text);
			}
			afterVariationEntered = false;

			//バリエーションを非表示
			DisableVariations ();

			//効果音再生
			audio.PlayOneShot (click);

			//子音に戻す
			#if UNITY_STANDALONE
			var i = IsFirstSet ? 0 : 5;
			#elif UNITY_ANDROID
			var i = latestIsFirstSet ? 0 : 5;
			#endif
			for (int j = 0; j < 5; j++) {
				selectorTexts [j].text = jpChars [j + i, 0];
			}
		}

		//右人差し指が押されており、
		if (RIndex_Hold) {
			//かつ右親指が上下されたときにバリエーション入力
			if (RThumbstickUp_Down) {
				//上部バリエーションの入力
				textHandler.SendChar (upperVariations [consonantIndex, currentHandPosition]);
				afterVariationEntered = true;

				//効果音再生
				audio.PlayOneShot (click);
			} else if (RThumbstickDown_Down) {
				//下部バリエーションの入力
				textHandler.SendChar (lowerVariations [consonantIndex, currentHandPosition]);
				afterVariationEntered = true;

				//効果音再生
				audio.PlayOneShot (click);
			}
		}

		//子音のセットを切り替え
		#if UNITY_STANDALONE
		if (RHand_Up && !RIndex_Hold) {
			for (int i = 0; i < 5; i++) {
				selectorTexts [i].text = jpChars [i, 0];
			}
		} else if (RHand_Down && !RIndex_Hold) {
			for (int i = 0; i < 5; i++) {
				selectorTexts [i].text = jpChars [i + 5, 0];
			}
		}
		#elif UNITY_ANDROID
		if (OVRInput.GetDown (OVRInput.Button.DpadLeft) || OVRInput.GetDown (OVRInput.Button.DpadRight)) {
			var i = IsFirstSet ? 5 : 0;
			latestIsFirstSet = !IsFirstSet;
			for (int j = 0; j < 5; j++)
				selectorTexts [j].text = jpChars [j + i, 0];

			//効果音再生
			audio.PlayOneShot (swipe);
		}
		#endif
	}

	//バリエーションを表示
	void EnableVariation (int currentPosition) {
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
