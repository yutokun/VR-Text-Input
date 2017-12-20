# VR日本語入力

![概要.gif](http://yutokun.com/vr/jpinput/1-overview.gif)

VR Text Input Method for Japanese.

VR とハンドコントローラー向けの日本語入力システムです。  

[ダウンロードはこちら](https://github.com/yutokun/VR-Text-Input/releases)

## 対応環境

- Oculus Touch
- Unity 5.6以上

## 実装方法

[リリースページ](https://github.com/yutokun/VR-Text-Input/releases)から最新の `.unitypackage` をDLし、プロジェクトにインポートしたら次の手順に従います。

### 1. Prefab をシーンに置く

Prefab フォルダの中に言語と方式別のプレハブを用意してありますので、これをシーンに置きます。  

### 2. テキストの送り先を設定する

Unity Event で入力された文字列を受け取ることができます。

下記のように、string を引数とする public 関数を作成して下さい。

```
public void OnJPInput (string str) {
	Debug.Log(str); //例
}
```

これを Inspector で登録、あるいはコードから `AddListener()` することで文字列を受信することができます。

デフォルトでは、**漢字を確定する度に string を得る**ことができます。

また、プレハブの TextHandler に存在する `InputType` を `Kana` に切り替えることで、**1文字入力する度に平仮名の string を得る**ことができます。

![Japanese.png](Japanese.png)

また、文字列の削除用に `OnBackspace` イベントを用意しました。  
変換用テキストボックスが空であり、かつB（削除）ボタンが押されたときに呼び出されます。ここに都合に合わせて1文字削除を実装して下さい。

```
public void OnBackspace() {
	//最後の1文字を削除する。
	textMesh.text = textMesh.text.Remove (textMesh.text.Length - 1, 1);
}
```

### 何かおかしいときのチェックリスト

- OVRCameraRig はありますか？（Touchの操作に必要）
- Oculus Avatar はなくても構いませんが、手が見えません
- テキストの送り先は正しく設定されていますか？ 現在のところ、これがないと最初の変換を確定した時点でエラーとなります。

## 予定

- Vive Controller 対応（Vive を買ったら）
- ~~IME への接続~~

## Licenses

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

**Oculus Integration**  
Copyright © 2014-2017 Oculus VR, LLC. All rights reserved,

