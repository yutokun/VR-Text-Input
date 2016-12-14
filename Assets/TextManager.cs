using UnityEngine;
using System.Collections;

public class TextManager : MonoBehaviour {

	VowelSelector vowel;
	ConsonantSelector consonant;

	TextMesh enteredText;
	string[,] japaneseSyllable = new string[10, 5] {
		{ "あ", "い", "う", "え", "お" },
		{ "か", "き", "く", "け", "こ" },
		{ "さ", "し", "す", "せ", "そ" },
		{ "た", "ち", "つ", "て", "と" },
		{ "な", "に", "ぬ", "ね", "の" },
		{ "は", "ひ", "ふ", "へ", "ほ" },
		{ "ま", "み", "む", "め", "も" },
		{ "や", "", "ゆ", "", "よ" },
		{ "ら", "り", "る", "れ", "ろ" },
		{ "わ", "", "を", "", "ん" },
	};

	void Start () {
		enteredText = GetComponent<TextMesh> ();
		vowel = GameObject.FindObjectOfType<VowelSelector> ();
		consonant = GameObject.FindObjectOfType<ConsonantSelector> ();
	}

	//現状、Vower の保持している TextMesh 配列の Text を InputCharacter に渡す処理。
	void Update () {
		if (OVRInput.GetDown (OVRInput.RawButton.A)) {
			InputCharacter (vowel.vowels [vowel.currentState].text);
		}
	}

	//文字列入力メソッド。
	//他クラスからデータを読むので、可読性のために分離しています。
	void InputCharacter (string vowel) {
		enteredText.text += vowel;

		//文字数が35以上の場合は最初の1文字を削除
		if (enteredText.text.Length > 35) {
			enteredText.text = enteredText.text.Remove (0, 1);
		}
	}
}
