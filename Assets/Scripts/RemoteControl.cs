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
    }

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
        url += "&name_input=" + (IsWaitingName() ? "1" : "0");

        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            req.timeout = 1;
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success) yield break;  // ツールが動いていない

            PollResponse res = JsonUtility.FromJson<PollResponse>(req.downloadHandler.text);
            if (res == null || res.commands == null) yield break;
            foreach (string cmd in res.commands)
            {
                Execute(cmd, game);
            }
        }
    }

    // ハイスコアの名前入力欄が出ているか
    private static bool IsWaitingName()
    {
        InputName input = InputName.Instance;
        return input != null && input.Field != null && input.Field.gameObject.activeInHierarchy;
    }

    // Web UI で入れた名前を、キーボードで入力して OK を押したのと同じように反映する
    private static void SubmitName(string name)
    {
        if (!IsWaitingName()) return;
        InputName input = InputName.Instance;
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
            SubmitName(cmd.Substring("name:".Length));
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
}
