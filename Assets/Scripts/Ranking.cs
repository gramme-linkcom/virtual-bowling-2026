using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;  //StreamWriterなどを使うために追加
using Newtonsoft.Json;



[System.Serializable]
public class PlayerScore
{
    [SerializeField]
    public string playerName;
    
    [SerializeField]
    public int score;
}
public class Ranking : MonoBehaviour
{
    public List<PlayerScore> scores = new List<PlayerScore>();
    [SerializeField] private GameContoller gameController;
    private static Ranking instance;
    public static Ranking Instance
    {
      get
      {
        if(instance == null)
        {
          instance = FindObjectOfType<Ranking>();
        }
        return instance;
      }
    }

        private void Start()
    {
        LoadScoreData(); // スコアデータを読み込む

        if(scores.Count < 6)
        {
          // 新しいスコアを追加
          PlayerScore newScore = new PlayerScore
            {
                playerName = "six",
                score = 8
            };
          scores.Add(newScore);
          SaveScoreData(); // スコアデータを保存
        }
    }

     // スコアの追加または変更後にランキングを再ソートする
    public void UpdateRankingScore()
    {
        scores.Sort((score1, score2) => score2.score.CompareTo(score1.score));
        SaveScoreData(); // ソート後、スコアデータを保存
    }

    public void AddNewScore(PlayerScore newScore)
    {
        scores.Add(newScore);
        UpdateRankingScore();
    }

    public void LoadScoreData()
    {
        string filePath;
        if (gameController.gameMode==GameContoller.GameMode.Standard)
        {
            filePath = Path.Combine(Application.persistentDataPath, "scoresStandard.json");
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                scores = JsonConvert.DeserializeObject<List<PlayerScore>>(json);
            }
            Debug.Log("defaltLOAD");
        }
        else if(gameController.gameMode == GameContoller.GameMode.Pins105)
        {
            filePath = Path.Combine(Application.persistentDataPath, "scores.json");
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                scores = JsonConvert.DeserializeObject<List<PlayerScore>>(json);
            }
            Debug.Log("OK");
        }
        
    }

    public void SaveScoreData()
    {
        string filePath;
        if (gameController.gameMode == GameContoller.GameMode.Standard)
        {
            filePath = Path.Combine(Application.persistentDataPath, "scoresStandard.json");
            // リスト全体をJSONに変換してファイルに書き込む
            string json = JsonConvert.SerializeObject(scores, Formatting.Indented);
            File.WriteAllText(filePath, json);

            Debug.Log("スコアデータをJSONファイルに保存しました：" + filePath);
            scores.Sort((score1, score2) => score2.score.CompareTo(score1.score));
            Debug.Log("defaltSAVE");

        }
        else if(gameController.gameMode == GameContoller.GameMode.Pins105)
        {
            filePath = Path.Combine(Application.persistentDataPath, "scores.json");

            // リスト全体をJSONに変換してファイルに書き込む
            string json = JsonConvert.SerializeObject(scores, Formatting.Indented);
            File.WriteAllText(filePath, json);

            Debug.Log("スコアデータをJSONファイルに保存しました：" + filePath);
            scores.Sort((score1, score2) => score2.score.CompareTo(score1.score));
            Debug.Log("105SAVE");
        }

    }





}