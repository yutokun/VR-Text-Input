using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class TextInputSample : MonoBehaviour {

	[SerializeField] TextMesh textMesh;

	//漢字変換後の入力を受け取るサンプルです。
	//OnJPInput(string) は漢字変換を確定するたびにコールされ、
	//確定した文字列を引数として取得できます。
	void OnJPInput (string str) {
		textMesh.text += str;
	}

	//後ろの文字を消去します。仮。デリゲートでコールバックに変更する予定。
	void OnBackspace () {
		textMesh.text = textMesh.text.Remove (textMesh.text.Length - 1, 1);
	}

	void Update () {
		//シーンのリロード。入力とは関係ありません。
		if (OVRInput.GetDown (OVRInput.RawButton.RThumbstick))
			SceneManager.LoadScene ("Example");
		
		//19文字を超えないように調整
		if (textMesh.text.Length > 19) {
			textMesh.text = textMesh.text.Remove (0, 1);
		}
	}
}
