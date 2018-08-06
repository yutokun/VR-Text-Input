using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TextInputSample : MonoBehaviour {

	[SerializeField] TextMesh textMesh;

	//入力を受け取るサンプルです。
	//TextHandler の UnityEvent に登録して呼び出します。
	//確定した文字列を引数として取得します。
	//文字の種別は、TextHandler の JPInputType で変更できます。
	public void OnTextInput(string str) {
		textMesh.text += str;
	}

	//後ろの文字を消去します。
	//TextHandler の UnityEvent に登録して呼び出します。
	public void OnBackspace() {
		if (textMesh.text.Length != 0)
			textMesh.text = textMesh.text.Remove(textMesh.text.Length - 1, 1);
		else
			OVRManager.PlatformUIConfirmQuit();
	}

	void Update() {
		//シーンのリロード。入力とは関係ありません。
		if (OVRInput.GetDown(OVRInput.RawButton.RThumbstick))
			SceneManager.LoadScene("Example");

		//テキストエリアを超えないように調整
		if (textMesh.text.Length > 19) {
			textMesh.text = textMesh.text.Remove(0, 1);
		}
	}
}