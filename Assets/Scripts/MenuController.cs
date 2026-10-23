using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using DG.Tweening;
using TMPro;

public class MenuController : MonoBehaviour
{

    [SerializeField] GameModeData GameModeData;
    [SerializeField] private Example JoyconController;
    [SerializeField] private Text GameModetext;
    [SerializeField] private Text ExplanationText;
    [SerializeField] private Text LoadText;
    [SerializeField] private GameObject GameObjectEuler;
    [SerializeField] private GameObject GameObject0;
    [SerializeField] private GameObject GameObject1;
    [SerializeField] private Image GameObjectImage0;
    [SerializeField] private Image GameObjectImage1;

    [SerializeField] private AudioClip sound1;
    [SerializeField] private AudioSource AudioSource;

    int GameModeNumberMax = 1;
    private float[] time = { 0 ,0};
    float x = 0;

    private AsyncOperation async;
    [SerializeField] private GameObject loadUI;

    [SerializeField] private Slider slider;

    bool one = false;



    // Start is called before the first frame update
    void Start()
    {
        GameModeData.GameModeNumber = 0;
        TextChange();
        LoadText.rectTransform.localScale = new Vector3(0, 0, 0);
        one = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        time[0] += Time.deltaTime;
        time[1] += Time.deltaTime;

        if (time[1] > 0.03f)
        {
            time[1] = 0;
            x++;
            if(x >= 360)
            {
                x = 0;
            }
        }
        GameObjectEuler.transform.eulerAngles = new Vector3(0,x,0);


        if (time[0] > 0.5f)
        {

            if (Input.GetKeyDown(KeyCode.LeftArrow) || JoyconController.Rstick() < -0.3)
            {
                AudioSource.PlayOneShot(sound1);
                time[0] = 0;
                GameModeData.GameModeNumber--;
                if(GameModeData.GameModeNumber < 0)
                {
                    GameModeData.GameModeNumber = GameModeNumberMax;
                }
                TextChange();
            }

            if (Input.GetKeyDown(KeyCode.RightArrow) || JoyconController.Rstick() > 0.3)
            {
                AudioSource.PlayOneShot(sound1);
                time[0] = 0;
                GameModeData.GameModeNumber++;
                if(GameModeData.GameModeNumber > GameModeNumberMax)
                {
                    GameModeData.GameModeNumber = 0;
                }
                TextChange();
            }

            //スペースキーでゲーム画面へ
            if ((Input.GetKeyDown(KeyCode.Space) || JoyconController.AButtonR()) && !one)
            {
                LoadText.rectTransform.localScale = new Vector3(1, 1, 1);
                AudioSource.PlayOneShot(sound1);
                time[0] = 0;
                Debug.Log("遷移！");
                one = true;
                StartCoroutine("LoadData");
            }

        }
    }

    IEnumerator LoadData()
    {
        async = SceneManager.LoadSceneAsync("MainScene");

        while (!async.isDone)
        {
            var progressVal = Mathf.Clamp01(async.progress / 0.9f);
            slider.value = progressVal;
            yield return null;
        }
    }

    private void TextChange()
    {
        switch(GameModeData.GameModeNumber)
        {
            case 0:
                GameModetext.text = "ミッションモード";
                ExplanationText.text = "全5ステージ\n目標ピン数を倒して\n最後までクリアできるか";
                GameObject0.transform.localScale = new Vector3(1,1,1);
                GameObject1.transform.localScale = new Vector3(0, 0, 0);
                GameObjectImage0.rectTransform.localScale = new Vector3(1, 1, 1);
                GameObjectImage1.rectTransform.localScale = new Vector3(0, 0, 0);
                x = 0;
                break;
            case 1:
                GameModetext.text = "チャレンジ！105Pins";
                ExplanationText.text = "立ちはだかる105個のピン\nあなたはいくつ倒せるかな？";
                GameObject0.transform.localScale = new Vector3(0, 0, 0);
                GameObject1.transform.localScale = new Vector3(1, 1, 1);
                GameObjectImage0.rectTransform.localScale = new Vector3(0, 0, 0);
                GameObjectImage1.rectTransform.localScale = new Vector3(1, 1, 1);
                x = 0;
                break;
        }
    }
}
