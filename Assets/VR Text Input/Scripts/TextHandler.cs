using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class TextHandler : MonoBehaviour {

	//一時入力エリア
	public TextMesh temporary;

	[Header ("コールバックの含まれる GameObject")]
	[Tooltip ("ここで指定した GameObject に含まれる OnJPKanjiInput() に対して文字列が送られます。")]
	public GameObject[] gameObjects;

	[System.Serializable]
	public class OnJPKanjiInput : UnityEvent<string> {
	}

	public OnJPKanjiInput onJpKanjiInput;

	[System.Serializable]
	public class OnJPKanaInput : UnityEvent<string> {
	}

	public OnJPKanaInput onJpKanaInput;

	public bool kanjiConversion = true;

	[Tooltip ("GameObject を自動検索します。\n！重要：文字列を確定する度にシーン上の全 GameObject に対して SendMessage を行うため、パフォーマンスが落ちる可能性があります。")]
	public bool autoMode = false;

	void Start () {
		temporary = GameObject.Find ("TemporaryText").GetComponent<TextMesh> ();
		temporary.text = "";

		if (autoMode)
			gameObjects = Resources.FindObjectsOfTypeAll<GameObject> ();
	}

	void Update () {
		//Bボタンで最後の1文字を削除
		if (OVRInput.GetDown (OVRInput.RawButton.B) && temporary.text.Length != 0) {
			//最後の1文字を消去する処理。
			temporary.text = temporary.text.Remove (temporary.text.Length - 1, 1);
		} else if (OVRInput.GetDown (OVRInput.RawButton.B) && temporary.text.Length == 0) {
			foreach (var item in gameObjects) {
				item.SendMessage ("OnBackspace", SendMessageOptions.DontRequireReceiver);
			}
		}

		//文字数が19以上の場合は最初の1文字を削除
		if (temporary.text.Length > 19) {
			temporary.text = temporary.text.Remove (0, 1);
		}
	}

	/// <summary>
	/// Inspector で指定したGameObject の OnJPKanjiInput() に変換済みテキストを送信。
	/// </summary>
	/// <param name="str">送信する文字</param>
	/// <param name="deleteCount">一時入力エリアから削除する文字数</param>
	public void Send (string str, int deleteCount) {
		if (temporary.text.Length < deleteCount)
			deleteCount = temporary.text.Length;
		temporary.text = temporary.text.Remove (0, deleteCount);
		foreach (var item in gameObjects) {
			item.SendMessage ("OnJPKanjiInput", str, SendMessageOptions.DontRequireReceiver);
		}
		onJpKanjiInput.Invoke (str);
	}

	/// <summary>
	/// Inspector で指定したGameObject の OnJPKanaInput() に変換前のひらがなを送信。
	/// </summary>
	/// <param name="str">String.</param>
	public void SendChar (string str) {
		foreach (var item in gameObjects) {
			item.SendMessage ("OnJPKanaInput", str, SendMessageOptions.DontRequireReceiver);
		}
		onJpKanaInput.Invoke (str);
	}
}
