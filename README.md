# VR-Text-Input

![概要.gif](http://yutokun.com/vr/jpinput/1-overview.gif)

VR Text Input Method for Japanese.

VR とハンドコントローラー向けの日本語入力システムです。

[ダウンロードはこちら](https://github.com/yutokun/VR-Text-Input/releases)
※Unity 5.5.1p2 で作成しています。

## 対応する機材

- Oculus Touch

## 実装方法

リリースページから最新の `.unitypackage` をDLし、プロジェクトにインポートしたら次の手順に従います。

### 1. Asset Store から JSON パーサーを手に入れる。

下記より JSON Object （無料）をインポートして下さい。  
https://www.assetstore.unity3d.com/jp/#!/content/710

### 2. Prefab をシーンに置く

`JapaneseInputSystem` という Prefab がありますので、これをシーンに置きます。  

### 3. タグを作る

`Kanji` タグを作り、Kanji プレハブに設定します。  
*ややこしくてごめんなさい、この操作は不要になる予定です。*

### 4. テキストの送り先を設定する

この関数を使うと、漢字を確定する度に string を得ることができます。

```
void OnJPInput (string str) {
	Debug.Log(str); //例
}
```

また、次の関数を使うと、1文字入力する度に平仮名の string を得ることができます。

```
void OnJPCharInput (string str) {
Debug.Log(str); //例
}
```

また、変換用テキストボックスと実際の入力先で1文字削除ボタンを使い分けるため、下記の関数を用意しました。  
変換用テキストボックスが空であり、かつB（削除）ボタンが押されたときに呼び出されます。ここに都合に合わせて1文字削除を実装して下さい。

```
void OnBackspace() {
	//最後の1文字を削除する。
	textMesh.text = textMesh.text.Remove (textMesh.text.Length - 1, 1);
}
```

最後にとても重要な事ですが、上記関数の含まれる GameObject をシーン中の JapaneseInputSystem にある Text Handler コンポーネントに設定して下さい。ここで指定した GameObject 内で `OnJPInput()` やらを探します。

Auto Mode も実装していますが、これは全 GameObject 走査するため、シーンの規模によってはフレーム落ちの原因となる可能性があります。お試し用と思って下さい。

### 何かおかしいときのチェックリスト

- Virtual Reality Supported をオンにしていますか？
- JSON Object を Asset Store からインポートしましたか？
- OVRCameraRig はありますか？（Touchの操作に必要）
- Oculus Avatar はなくても構いませんが、手が見えません（そりゃそうだ）
- テキストの送り先は正しく設定されていますか？ 現在のところ、これがないと最初の変換を確定した時点でエラーとなります。

## 予定

- Vive Controller 対応（Vive を買ったら）
- ~~IME への接続~~

## License

MIT License

Copyright (c) 2017 yutoVR

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.