using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class KanjiConverter : MonoBehaviour {

	TextMesh textMesh;
	TextHandler textHandler;
	[SerializeField] GameObject kanjiPrefab;
	[SerializeField] List<TextMesh> kanji;
	GameObject controller;
	OVRHapticsClip hapticsClip;

	void Start () {
		//テキスト入力欄への参照を取得
		textMesh = GetComponent<TextMesh> ();

		//漢字変換がオフなら変換エリアを非表示
		textHandler = FindObjectOfType<TextHandler> ();
		if (textHandler.kanjiConversion == false)
			transform.parent.GetComponent<MeshRenderer> ().enabled = false;

		//振動準備
		controller = GameObject.Find ("RightHandAnchor");
		byte[] hapticsBytes = new byte[4];
		for (int i = 0; i < hapticsBytes.Length; i++) {
			hapticsBytes [i] = 128;
		}
		hapticsClip = new OVRHapticsClip (hapticsBytes, hapticsBytes.Length);

		//漢字の変換候補枠を作成
		for (int i = 0; i < 5; i++) {
			Instantiate (kanjiPrefab, transform.position, transform.rotation, transform.parent);
		}

		//漢字候補枠を取得してクリア
		GameObject[] kanjis = GameObject.FindGameObjectsWithTag ("Kanji");
		for (int i = 0; i < kanjis.Length; i++) {
			kanji.Add (kanjis [i].GetComponent<TextMesh> ());
			kanjis [i].transform.Translate (0, (i + 1) * -0.1763988f, 0);
			kanji [i].text = "";
		}
	}

	int prev = 0, current = 0;
	[SerializeField] float euler;
	[HideInInspector] public bool isConverting = false;

	void Update () {
		//Aボタンで漢字変換を行う
		if (OVRInput.GetDown (OVRInput.RawButton.A) && isConverting == false && textHandler.temporary.text != "")
			StartCoroutine (Convert ());

		//色と振動の処理
		euler = controller.transform.rotation.x;
		if (euler > -0.1f) {
			current = 4;
		} else if (euler > -0.2f) {
			current = 3;
		} else if (euler > -0.3f) {
			current = 2;
		} else if (euler > -0.4f) {
			current = 1;
		} else if (euler > -0.5f) {
			current = 0;
		}
		if (isConverting && prev != current) {
			OVRHaptics.RightChannel.Mix (hapticsClip);
			kanji [prev].color = new Color (255, 255, 255);
			kanji [current].color = new Color (255, 0, 0);
		}
		prev = current;
	}

	IEnumerator Convert () {
		UnityWebRequest www = UnityWebRequest.Get ("http://www.google.com/transliterate?langpair=ja-Hira|ja&text=" + WWW.EscapeURL (textMesh.text));
		yield return www.Send ();

		if (www.isError) {
			Debug.Log (www.error);
		} else {
			//返ってくるJSONが配列[]のみで処理できないため変換
			string result = www.downloadHandler.text.Replace ("[", "{");
			result = result.Replace ("]", "}");

			//JSONをプログラムから扱える形式に変換
			JSONObject j = new JSONObject (result);

//			j.list [?].list [0]に変換前の文字が、
//			j.list [?].list [1].keys [?]に変換後の文字が入っている。
//			Debug.Log ("変換文節の数：" + j.list.Count);
//			Debug.Log ("候補の数：" + j.list [0].list [1].keys.Count);
//			Debug.Log ("変化前の文字数" + j.list [0].list [0].ToString ().Length);
			int numberOfCandidate = j.list [0].list [1].keys.Count;

			//Aボタンの挙動を変更
			isConverting = true;

			//前回変換時の赤が残ってしまうのでリセット
			foreach (var item in kanji) {
				item.color = new Color (255, 255, 255);
			}

			for (int phrase = 0; phrase < j.list.Count; phrase++) {
				for (int i = 0; i < numberOfCandidate; i++) {
					kanji [i].text = j.list [phrase].list [1].keys [i]; //変換候補をKanjiに表示
				}
				yield return new WaitUntil (() => OVRInput.GetDown (OVRInput.RawButton.A));
				textHandler.Send (j.list [phrase].list [1].keys [current], j.list [phrase].list [0].ToString ().Length - 2); //選んだ候補を入力
				yield return null;
			}
			foreach (var item in kanji)
				item.text = "";
			isConverting = false;
		}
	}
}
