using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class JPTextHandler : MonoBehaviour {

	//一時入力エリア
	public TextMesh temporary;

	public enum JPInputType {
		Kanji,
		Kana
	}

	public JPInputType inputType;

	[System.Serializable]
	public class OnJPInput : UnityEvent<string> {
	}

	public OnJPInput onJpInput;
	public UnityEvent onBackspace;

	void Start () {
		temporary.text = "";
	}

	void Update () {
		//Bボタンで最後の1文字を削除
		if (OVRInput.GetDown (OVRInput.RawButton.B) && temporary.text.Length != 0) {
			//最後の1文字を消去する処理。
			temporary.text = temporary.text.Remove (temporary.text.Length - 1, 1);
		} else if (OVRInput.GetDown (OVRInput.RawButton.B) && temporary.text.Length == 0) {
			onBackspace.Invoke ();
		}

		//テキストエリアをはみ出さないように調整
		if (temporary.text.Length > 19) {
			temporary.text = temporary.text.Remove (0, 1);
		}
	}

	public void SendChar (string str) {
		if (inputType == JPInputType.Kanji) {
			temporary.text += str;
		} else if (inputType == JPInputType.Kana) {
			Send (str, 0);
		}
	}

	/// <summary>
	/// UnityEvent で変換済みテキストを送信。
	/// </summary>
	/// <param name="str">送信する文字</param>
	/// <param name="deleteCount">一時入力エリアから削除する文字数</param>
	public void Send (string str, int deleteCount) {
		if (temporary.text.Length < deleteCount)
			deleteCount = temporary.text.Length;
		temporary.text = temporary.text.Remove (0, deleteCount);
		onJpInput.Invoke (str);
	}
}
