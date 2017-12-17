using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.VR;

public class KanjiConverter : MonoBehaviour {

	TextMesh textMesh;
	TextHandler textHandler;
	[SerializeField] GameObject kanjiPrefab;
	[SerializeField] List<TextMesh> kanji;
	OVRHapticsClip hapticsClip;
	[SerializeField, Range (0.1f, 0.2f)] float space = 0.1763988f;

	void Start () {
		//テキスト入力欄への参照を取得
		textMesh = GetComponent<TextMesh> ();

		//漢字変換がオフなら変換エリアを非表示
		textHandler = FindObjectOfType<TextHandler> ();
		if (textHandler.kanjiConversion == false)
			transform.parent.GetComponent<MeshRenderer> ().enabled = false;

		//振動準備
		byte[] hapticsBytes = new byte[4];
		for (int i = 0; i < hapticsBytes.Length; i++) {
			hapticsBytes [i] = 128;
		}
		hapticsClip = new OVRHapticsClip (hapticsBytes, hapticsBytes.Length);

		//漢字の変換候補枠を作成
		for (int i = 0; i < 5; i++) {
			var obj = Instantiate (kanjiPrefab, transform.position, transform.rotation, transform);
			obj.transform.Translate (0, (i + 1) * -space, 0);
			kanji.Add (obj.GetComponent<TextMesh> ());
			kanji [i].text = "";
		}
	}

	int prev = 0, current = 0;
	[HideInInspector] public bool isConverting = false;

	void Update () {
		//Aボタンで漢字変換を行う
		if (OVRInput.GetDown (OVRInput.RawButton.A) && isConverting == false && textHandler.temporary.text != "")
			StartCoroutine (Convert ());

		//色と振動の処理
		float euler = InputTracking.GetLocalRotation (VRNode.RightHand).eulerAngles.x;
		if (euler < 300) {
			current = current;
		} else if (euler < 310) {
			current = 0;
		} else if (euler < 320) {
			current = 1;
		} else if (euler < 330) {
			current = 2;
		} else if (euler < 340) {
			current = 3;
		} else if (euler < 350) {
			current = 4;
		}
		if (isConverting && prev != current) {
			OVRHaptics.RightChannel.Mix (hapticsClip);
			kanji [prev].color = Color.white;
			kanji [current].color = Color.red;
		}
		prev = current;
	}

	IEnumerator Convert () {
		UnityWebRequest www = UnityWebRequest.Get ("http://www.google.com/transliterate?langpair=ja-Hira|ja&text=" + WWW.EscapeURL (textMesh.text));
		yield return www.Send ();

		if (www.isError) {
			Debug.Log (www.error);
		} else {
			//変換候補を取得
			//例：[["とうきょうの",["東京の","TOKYOの","tokyoの","Tokyoの","トウキョウの"]],["おでんや",["おでん屋","おでんや","お田野","オデンや","お田や"]]]
			string result = www.downloadHandler.text;

			//文節に分割
			var phrases = result.Split (new string[] { "]],[" }, System.StringSplitOptions.None);

			//不要な文字列（",[]）を削除
			for (int i = 0; i < phrases.Length; i++) {
				phrases [i] = phrases [i].Replace ("\"", "");
				phrases [i] = phrases [i].Replace ("[", "");
				phrases [i] = phrases [i].Replace ("]", "");
			}

			//不要文字削除後の例
			//とうきょうの,東京の,TOKYOの,tokyoの,Tokyoの,トウキョウの
			//おでん,おでん,オデン,お田,お伝,おデン

			//Aボタンの挙動を変更
			isConverting = true;

			for (int i = 0; i < phrases.Length; i++) {
				//変換候補
				var candidates = new List<string> (phrases [i].Split (','));

				//変換前文字列
				string original = candidates [0];

				//変換候補から変換前文字列を除去
				candidates.RemoveAt (0);

				//変換候補を TextMesh kanji に並べる
				for (int j = 0; j < candidates.Count; j++)
					kanji [j].text = candidates [j];
				
				yield return new WaitUntil (() => OVRInput.GetDown (OVRInput.RawButton.A));

				//選んだ候補を入力
				textHandler.Send (candidates [current], original.Length);

				//変換表示をクリア
				foreach (var item in kanji)
					item.text = "";
				
				yield return null;
			}

			//変換候補枠の色をリセット
			foreach (var item in kanji)
				item.color = Color.white;
			
			isConverting = false;
		}
	}
}
