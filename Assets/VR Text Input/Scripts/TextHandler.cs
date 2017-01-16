using UnityEngine;
using System.Collections;

public class TextHandler : MonoBehaviour {

	//一時入力エリア
	public TextMesh temporary;

	[Header ("OnJPInput() の含まれる GameObject")]
	[Tooltip ("ここで指定した GameObject に含まれる OnJPInput() に対して文字列が送られます。")]
	public GameObject[] gameObjects;

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
	/// Inspector で指定したGameObject の OnJPInput() にテキストを送信。
	/// </summary>
	/// <param name="str">送信する文字</param>
	/// <param name="deleteCount">一時入力エリアから削除する文字数</param>
	public void Send (string str, int deleteCount) {
		temporary.text = temporary.text.Remove (0, deleteCount);
		foreach (var item in gameObjects) {
			item.SendMessage ("OnJPInput", str, SendMessageOptions.DontRequireReceiver);
		}
	}
}
