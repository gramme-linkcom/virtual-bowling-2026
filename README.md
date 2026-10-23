# Virtual Bowling（Unity プロジェクト）

Unity 2021 LTS で制作したボウリングゲームです。

---

## 特長
- 2つのゲームモード
  - Standard: ノルマ本数をクリアして次のレーンへ。全5ステージある。（ボーリング台を使用）
  - 105Pins: 105本のピンに挑むモード（Joyconを使用）
- Standardモードは`vector.txt` の更新を監視して画面上で投球する。

---

## 動作環境
- Unity: 2021.3.23f1（LTS）
- OS: Windows 11で開発

> プロジェクトの Unity バージョンは `ProjectSettings/ProjectVersion.txt` に記載（2021.3.23f1）。

---

## フォルダ構成（抜粋）
- `Assets/Scenes/`
  - `MenuScene.unity` … モード選択シーン
  - `MainScene.unity` … ゲーム本編
- `Assets/Scripts/`
  - `GameController.cs`: ゲーム進行、ボール生成、カメラ、判定、リプレイ
  - `UIController.cs`: スコア/ターゲット/ピン情報/リザルト/白フラッシュ演出
  - `MenuController.cs`: モード選択、ロード UI、Joy-Con/キー入力
  - `SoundManager.cs`: 投球音、ピンヒット音、歓声、ボール転がり音のフェード
  - `Pin.cs`: ピン個別の倒れ判定・サウンド制御
  - `Ranking.cs`: JSON によるスコアの保存/読み込み/ソート
  - `Example.cs`: Joy-Con 入力（ZR 長押し→リリースで投球、R スティック取得 等）
- `vector.txt` … 外部投球ベクトル入力ファイル（プロジェクト直下、変更可）

---

## セットアップ手順（初回）
1) Unity Hub で本リポジトリを開く（Unity 2021.3.23f1 推奨）
2) DOTween のセットアップ（未済の場合）
   - メニュー: Tools > Demigiant > DOTween Utility Panel > Setup DOTween を実行
3) Scenes を確認
   - `Assets/Scenes/MenuScene.unity` を開いて再生（Play）
4) Joy-Con（105ピンモードを使用する際のみ）
   - Windows の Bluetooth 設定から Joy-Con (R) をペアリング
   - ゲーム実行時に自動認識（JoyconLib を使用）

---

## 遊び方・操作

### メニュー（MenuScene）
- モード選択: 方向キー 左/右 または Joy-Con 右スティック 左/右
- 決定: Space キー または Joy-Con A（DPAD_RIGHT）
  - 決定すると `MainScene` をロードします
- デバッグ投球（キーボード）: S + Tab 同時押しでサンプル投球
- リトライ（ステージや投球のリセット）: Esc（内部的にシーン再読込）

---

## 外部ファイル連携（vector.txt）
`GameContoller` はインスペクターの `velocityDataPath` で指定したファイルを監視し、更新されると3行の数値を読み込んで投球します。

- 形式（3 行の float 値、改行区切り）:
  1. front（前方向）
  2. height（高さ方向）
  3. side（左右方向）
- ベクトル変換（実装抜粋）:
  - `new Vector3(z * 200, x * -40, y * -40)` を AddForce に使用
- 共有アクセス:
  - 読み込みは `FileShare.ReadWrite` で行うため、他プロセスからの同時書き込みと共存可能

---

## スコア・ランキング
- 保存先: `Application.persistentDataPath`
  - Standard: `scoresStandard.json`
  - 105Pins: `scores.json`
- 表示: タイトル（スタートメニュー）にトップ5を表示
- 105Pins の自己ベストは一時的に UI へも反映されます（`Pins105AllCount`）

> Windows の `persistentDataPath` は概ね
> `C:\\Users\\<User>\\AppData\\LocalLow\\<CompanyName>\\<ProductName>` 配下です（プロジェクト設定に依存）。

---

## シーン構成と推奨フロー
1) `MenuScene` でモード選択 → 決定
2) `MainScene` でスタート → プレイ
3) リザルトへ遷移（必要に応じて名前入力 → ランキング保存）
4) タイトルに戻ってリプレイ

> ビルド時は `MenuScene` と `MainScene` の2つを Build Settings に含めてください。

---

## ビルド手順（Windows）
1) File > Build Settings
2) Scenes In Build に `MenuScene` と `MainScene` を追加
3) Platform: PC, Mac & Linux Standalone → Target Platform: Windows
4) Build または Build And Run

ビルド先のフォルダは、Unity_VB2025以下である。
ビルドによるフォルダは、[text](UnityCrashHandler64.exe) [text](UnityPlayer.dll) [text](VB2025.exe)である。

ビルド後のアプリを開いたら(VB2025)、Alt + Enterを押して、全画面表示をする。

---

## カメラのセットアップ
本プロジェクトでは `GameContoller` がカメラの位置・回転・追従を自動制御します。シーン上では以下を満たせばOKです。

1) カメラの用意
- シーンに 1 つだけカメラを配置し、Tag を `MainCamera` に設定
- 推奨設定: Projection = Perspective, Field of View = 60（任意）

2) コンポーネント
- Camera に `AudioListener`（デフォルト）を付与
- Camera に `AudioSource` を追加（ワンショットSE再生用）
- Camera に `SoundManager` を追加し、以下を割り当て
  - AudioSource: 上記で追加した AudioSource
  - Hit Pin SE: ピン衝突音（例: `Assets/Sound/HitPin.mp3`）
  - Throw Ball SE: 投球音（例: `Assets/Sound/throw_ball.mp3`）
  - Cheers SE: 歓声音（任意の音源）

3) 参照の接続
- `GameContoller` のインスペクターにある `SoundManager` フィールドへ、Camera 上の `SoundManager` をドラッグ＆ドロップ

4) 自動制御について（参考）
- デフォルトカメラ位置/回転はスクリプトから設定されます
  - Standard: 位置 `(9.5, 1.0, laneZ + cameraZ)` / 角度 `(16.045, -90, 0)`
  - 105Pins: 位置 `(9.5, 1.0, laneZ + cameraZ)` / 角度 `(16.045, -90, 0)`
  - Replay: 位置 `(-5.4, 0.8, 0.7)`（ボール注視）
- 待機中（ボール未生成時）は、Joy-Con 右スティック左右で `cameraZ` を微調整可能

注意:
- `Pin` スクリプトは `Camera.main` から `SoundManager` を取得します。必ずカメラに `SoundManager` を付与してください。
- ボールの転がり音はボール側の `AudioSource` を使用します（`SoundManager.PlayBallRollSE(ballAudioSource)`）。ボールPrefabに転がり音のクリップを割り当ててください（例: `Assets/Sound/ball_roll.mp3`）。

---

## 現実のボール認識カメラとの連携（外部トラッカー）
本プロジェクト自体には実カメラの映像処理は含まれていません。現実のボールをカメラで認識して投球させたい場合は、外部プログラム（例: Python + OpenCV）でボールの動きを解析し、投球のたびに `vector.txt` に数値を書き込む方式を推奨します。

### 連携の考え方
- Unity 側は `vector.txt` の最終更新時刻の変化を監視し、更新があれば「1 回だけ」ボールを生成して力を加えます。
- したがって、外部トラッカーは「投球1回につき1回だけ」`vector.txt` を更新してください（連続で書き換えると連続でボールが生成されます）。

### データ仕様（重要）
`vector.txt` は3行構成で、各行は float 値1つです。
1. front（前方向の量）
2. height（上方向の量）
3. side（横方向の量）

Unity 側では以下の力として使用します：
- ForceX = side × 200
- ForceY = -front × 40
- ForceZ = -height × 40

つまり、front を正にすると下方向に力がかかる（UnityのYは上が正）、height を正にすると手前（-Z）へ、side を正にすると +X 方向へ転がります。実際のカメラ座標系と一致しない場合は、符号やスケールを外部側で調整してください。

### セットアップ手順（例）
1) カメラとトラッカー用PC環境を用意（Webカメラ/産業用カメラなど）
2) 外部プログラムでボールのフレーム間移動から投球ベクトルを算出
   - 例: 天井カメラで俯瞰し、2D画面上の速度ベクトルを lane 方向（front）、横方向（side）に分解
   - height は上方向の量。単眼での厳密推定は難しいため 0 固定でも可。必要に応じて深度推定/複眼構成をご検討ください
3) Unity 側 `GameContoller` の `velocityDataPath` に `vector.txt` の絶対パスを設定
4) 投球のタイミングで `vector.txt` を「一度だけ」更新

### 出力例
```
0.85
0.10
-0.05
```
- 上記は front=0.85, height=0.10, side=-0.05 を意味します

### サンプル（擬似コード）
外部プログラムは以下のように「3行の数値を書いて閉じる」だけでOKです。Windowsでは文字コードはUTF-8推奨、改行は `\n` で問題ありません。

- 疑似コード:
```
open(filePath, writeTruncate)
writeLine(front)
writeLine(height)
writeLine(side)
close()
```

- Python（OpenCVでのトラッキングは省略、書き込みのみの最小例）:
```
front, height, side = 0.9, 0.0, -0.1
with open(r"C:\\Users\\<User>\\Unity_VB2025\\vector.txt", "w", encoding="utf-8") as f:
    f.write(f"{front}\n{height}\n{side}")
```

### よくある注意点
- 末尾に余計な空行や空白があると `float.Parse` で例外になる可能性があります（本プロジェクトのコードは前提として単純な `Split('\n')` を行います）。数値3行のみを書いてください
- 常時高頻度で上書きし続けると投球が連発されます。投球1回につき1回の更新にしてください
- パスは Unity の `velocityDataPath` で正しく設定（絶対パス推奨）。更新されるのは「最終更新時刻」で判定しています
- スケールは任意です。飛び過ぎる場合は front/side/height を小さくするか、Unity 側スケール（200/40）を調整してください
