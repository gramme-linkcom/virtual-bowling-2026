using System.Collections;
using System.Collections.Generic;
using TMPro;
// TMPro.EditorUtilitiesはエディタ拡張用なので、ビルド時に含めないようにする
#if UNITY_EDITOR
using TMPro.EditorUtilities;
#endif
using UnityEngine;
using UnityEngine.UI;

public class InputName : MonoBehaviour
{
    private static InputName instance;
    public static InputName Instance
    {
        get
        {
            if(instance == null)
            {
                instance = FindObjectOfType<InputName>();
            }
            return instance;
        }
    }

    // InputFieldのText参照用
    public TMP_InputField Field;
    public Button button;

    // Start と Update は省略

    public void OnEndEdit()
    {
        string input = Field.text;
        if(input != null){
            Debug.Log("名前:" + input);
        }
        // Rankingクラスのインスタンスを取得
        Ranking ranking = Ranking.Instance;
        if(ranking.scores.Count >= 6)
        {
            input = input.Replace("\n", ""); // 改行文字を削除
            ranking.scores[5].playerName = input;
            ranking.SaveScoreData(); // 代入後、スコアデータを保存する
        }
        
    }

        // 入力フォームを表示する
    public void ShowInputField()
    {
        Field.gameObject.SetActive(true); // InputFieldを表示
        button.gameObject.SetActive(true);
    }

    // 入力フォームを非表示にする
    public void HideInputField()
    {
        Field.gameObject.SetActive(false); // InputFieldを非表示
        button.gameObject.SetActive(false);
    }
}