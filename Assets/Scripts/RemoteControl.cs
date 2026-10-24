using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

// Web UI (vb2026_utility_tools) の「ゲーム操作」ボタンを、キー入力の代わりとして受け取る (2026 追加)。
// シーンに置かなくても、ゲームを起動したときに自動で作られる。
// ツールが起動していないときは何もしない (キーボード操作は今まで通り使える)。

// Input.GetKeyDown の代わり。本物のキーが押されたフレームか、Web UI のボタンが押されたフレームだけ true。
public static class RemoteInput
{
    private static readonly Dictionary<KeyCode, int> pressedFrame = new Dictionary<KeyCode, int>();

    public static bool GetKeyDown(KeyCode key)
    {
        if (Input.GetKeyDown(key)) return true;
        return pressedFrame.TryGetValue(key, out int frame) && frame == Time.frameCount;
    }

    // 次のフレームで key が押されたことにする。
    internal static void Press(KeyCode key)
    {
        pressedFrame[key] = Time.frameCount + 1;
    }
}

public class RemoteControl : MonoBehaviour
{
    // ツールの Web UI のアドレス (run.bat の --port を変えたときはここも変える)
    public static string ToolUrl = "http://127.0.0.1:8080";
    private const float PollInterval = 0.2f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateOnStartup()
    {
        var go = new GameObject("RemoteControl");
        DontDestroyOnLoad(go);  // Esc でシーンを読み直しても消えないようにする
        go.AddComponent<RemoteControl>();
    }

    [System.Serializable]
    private class PollResponse
    {
        public string[] commands;
        public bool paused;  // 管理画面の「休止」ボタン
    }

    // 休止中 (管理画面の「休止」ボタン)。画面全体に「休止中」を出す。
    private static bool isPaused;
    private int failCount;  // ツールに続けてつながらなかった回数

    private IEnumerator Start()
    {
        var wait = new WaitForSecondsRealtime(PollInterval);
        while (true)
        {
            yield return Poll();
            yield return wait;
        }
    }

    // ツールに今の状態を伝え、Web UI で押されたボタン (命令) を受け取る。
    private IEnumerator Poll()
    {
        GameContoller game = FindObjectOfType<GameContoller>();
        string scene = SceneManager.GetActiveScene().name;
        string state = game != null ? game.CurrentStateName : "Menu";
        string url = ToolUrl + "/api/unity/poll?scene=" + UnityWebRequest.EscapeURL(scene)
                     + "&state=" + UnityWebRequest.EscapeURL(state);
        if (game != null)
        {
            url += "&gutter=" + (game.IsGutterWallUp ? "1" : "0");
            url += "&score=" + game.Pins105AllCount;
        }
        url += "&name_input=" + (IsWaitingName(game) ? "1" : "0");

        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            req.timeout = 1;
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                // ツールが動いていない。しばらくつながらなければ休止の表示も消す (出しっぱなしにしない)
                if (++failCount >= 5) isPaused = false;
                yield break;
            }
            failCount = 0;

            PollResponse res = JsonUtility.FromJson<PollResponse>(req.downloadHandler.text);
            if (res == null) yield break;
            isPaused = res.paused;
            if (res.commands == null) yield break;
            foreach (string cmd in res.commands)
            {
                Execute(cmd, game);
            }
        }
    }

    // ハイスコアの名前入力欄が出ているか。
    // シーンには InputName が2つある (UIController にも付いている) ので、
    // GameContoller が実際に表示・非表示している方を見る。
    private static InputName GetNameInput(GameContoller game)
    {
        return game != null ? game.NameInput : null;
    }

    private static bool IsWaitingName(GameContoller game)
    {
        InputName input = GetNameInput(game);
        return input != null && input.Field != null && input.Field.gameObject.activeInHierarchy;
    }

    // Web UI で入れた名前を、キーボードで入力して OK を押したのと同じように反映する
    private static void SubmitName(string name, GameContoller game)
    {
        if (!IsWaitingName(game)) return;
        InputName input = GetNameInput(game);
        if (input.Field.characterLimit > 0 && name.Length > input.Field.characterLimit)
        {
            name = name.Substring(0, input.Field.characterLimit);
        }
        input.Field.text = name;

        // 入力確定 (Enter) と同じ。InputName.OnEndEdit でランキングに名前が入る
        if (input.Field.onEndEdit.GetPersistentEventCount() > 0)
            input.Field.onEndEdit.Invoke(name);
        else
            input.OnEndEdit();

        // OK ボタンを押したのと同じ。OKbutton.Onclick → ResetGame でスコアが保存される
        if (input.button != null && input.button.onClick.GetPersistentEventCount() > 0)
        {
            input.button.onClick.Invoke();
        }
        else
        {
            OKbutton ok = FindObjectOfType<OKbutton>();
            if (ok != null) ok.Onclick();
        }
    }

    private static void Execute(string cmd, GameContoller game)
    {
        if (cmd.StartsWith("name:"))
        {
            SubmitName(cmd.Substring("name:".Length), game);
            return;
        }
        switch (cmd)
        {
            case "start":  // スタート画面・メニュー画面のスペースキー
                RemoteInput.Press(KeyCode.Space);
                break;
            case "reset":  // Esc キー (1投目からやり直し)
                RemoteInput.Press(KeyCode.Escape);
                break;
            case "gutter_on":  // G キー (壁が下がっているときだけ押す)
                if (game != null && !game.IsGutterWallUp) RemoteInput.Press(KeyCode.G);
                break;
            case "gutter_off":  // G キー (壁が上がっているときだけ押す)
                if (game != null && game.IsGutterWallUp) RemoteInput.Press(KeyCode.G);
                break;
            case "mode_prev":  // メニュー画面の ← キー
                RemoteInput.Press(KeyCode.LeftArrow);
                break;
            case "mode_next":  // メニュー画面の → キー
                RemoteInput.Press(KeyCode.RightArrow);
                break;
        }
    }

    // ---- 休止中の表示 (お客さん用画面と同じデザイン) ----
    private Font pauseFont;
    private Texture2D backTex, panelTex, accentTex;
    private GUIStyle titleStyle, subStyle;
    private int styleHeight;

    private static Texture2D MakeTex(Color c)
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, c);
        tex.Apply();
        return tex;
    }

    private void OnGUI()
    {
        if (!isPaused) return;
        GUI.depth = -1000;  // ゲームの UI より手前に描く

        if (backTex == null)
        {
            backTex = MakeTex(new Color(0.03f, 0.04f, 0.055f, 0.94f));
            panelTex = MakeTex(new Color(0.086f, 0.106f, 0.141f, 1f));   // #161b24
            accentTex = MakeTex(new Color(0.353f, 0.663f, 0.902f, 1f));  // #5aa9e6
            // 日本語を出すため Windows のフォントを使う
            pauseFont = Font.CreateDynamicFontFromOSFont(new[] { "Yu Gothic UI", "Meiryo", "MS Gothic" }, 64);
        }
        int w = Screen.width, h = Screen.height;
        if (titleStyle == null || styleHeight != h)
        {
            styleHeight = h;
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                font = pauseFont, fontSize = Mathf.RoundToInt(h * 0.085f),
                fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter
            };
            titleStyle.normal.textColor = new Color(0.91f, 0.925f, 0.95f);
            subStyle = new GUIStyle(titleStyle)
            {
                fontSize = Mathf.RoundToInt(h * 0.03f), fontStyle = FontStyle.Normal
            };
            subStyle.normal.textColor = new Color(0.54f, 0.58f, 0.65f);
        }

        GUI.DrawTexture(new Rect(0, 0, w, h), backTex);
        float pw = Mathf.Min(w * 0.6f, h * 1.1f), ph = h * 0.38f;
        float px = (w - pw) / 2f, py = (h - ph) / 2f;
        GUI.DrawTexture(new Rect(px, py, pw, ph), panelTex);
        GUI.DrawTexture(new Rect(px, py, pw, h * 0.008f), accentTex);
        GUI.Label(new Rect(px, py + ph * 0.12f, pw, ph * 0.42f), "休 止 中", titleStyle);
        GUI.Label(new Rect(px, py + ph * 0.55f, pw, ph * 0.35f), "ただいま休止しています。\nしばらくお待ちください。", subStyle);
    }
}
