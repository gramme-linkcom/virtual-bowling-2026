using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using DG.Tweening;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameContoller gameController;


    // GetComponentsInChildren()�ł̎擾�͏��Ԃ����ɂ���ĈقȂ邽�߁~
    [SerializeField] private List<Image> informationPinImageArr;
    [SerializeField] private List<ResultPinList> resultPinList;

    // Start Menu
    [SerializeField] private RectTransform startMenuRT;
    [SerializeField] private RectTransform startMenuTitleRT;
    [SerializeField] private TextMeshProUGUI startMenuTitletext;
    [SerializeField] private RectTransform startMenuBackgroundRT;
    [SerializeField] private RectTransform startMenuMessageRT;

    // Information Pin
    [SerializeField] private RectTransform informationPinRT;
    private CanvasGroup informationPinCG;

    // Message
    [SerializeField] private RectTransform messageBackgroundRT;
    private Image messageBackgroundImg;
    [SerializeField] private TextMeshProUGUI messageText;

    // Throw Count Text
    [SerializeField] private Image throwCountBackgroundImg;
    [SerializeField] private RectTransform throwCountRT;
    private TextMeshProUGUI throwCountText;

    //playing
    [SerializeField] private Image playinglifeBackgroundImg;
    [SerializeField] private RectTransform playinglifeBackgroundRT;
    [SerializeField] private TextMeshProUGUI StageNumberText;
    [SerializeField] private RectTransform StageNumberRT;
    [SerializeField] private List<Image> ballcountNumber;
    [SerializeField] private TextMeshProUGUI ballcountplasText;
    [SerializeField] private RectTransform ballcountplasTextRT;
    [SerializeField] private TextMeshProUGUI TargetText;
    [SerializeField] private RectTransform TargetTextRT;

    [SerializeField] private StageInfo[] stageinfoArr;

    // Result
    [SerializeField] private RectTransform resultRT;
    [SerializeField] private Image resultBackgroundImg;
    [SerializeField] private TextMeshProUGUI resultTitleText;
    private RectTransform resultTitleRT;
    [SerializeField] private TextMeshProUGUI knockedOutPinCountText;
    private RectTransform knockedOutPinCountRT;
    [SerializeField] private RectTransform resultScoreFrameRT;
    [SerializeField] private Image resultMessageBackgroundImg;
    [SerializeField] private TextMeshProUGUI resultMessageText;

    //Result�ǉ���
    [SerializeField] private TextMeshProUGUI CLEAR1Text;
    private RectTransform CLEAR1TextRT;
    [SerializeField] private TextMeshProUGUI CLEAR2Text;
    private RectTransform CLEAR2TextRT;
    [SerializeField] private TextMeshProUGUI CLEAR3Text;
    private RectTransform CLEAR3TextRT;
    [SerializeField] private TextMeshProUGUI CLEAR4Text;
    private RectTransform CLEAR4TextRT;
    [SerializeField] private TextMeshProUGUI CLEAR5Text;
    private RectTransform CLEAR5TextRT;
    [SerializeField] private TextMeshProUGUI knockedOutPinC1Text;
    private RectTransform knockedOutPinC1RT;
    [SerializeField] private TextMeshProUGUI knockedOutPinC2Text;
    private RectTransform knockedOutPinC2RT;
    [SerializeField] private TextMeshProUGUI knockedOutPinC3Text;
    private RectTransform knockedOutPinC3RT;
    [SerializeField] private TextMeshProUGUI knockedOutPinC4Text;
    private RectTransform knockedOutPinC4RT;
    [SerializeField] private TextMeshProUGUI knockedOutPinC5Text;
    private RectTransform knockedOutPinC5RT;

    [SerializeField] private TextMeshProUGUI Pins105CountText;
    private RectTransform Pins105CountRT;
    [SerializeField] private Image whitePanelImg;
    [SerializeField] private GameObject Result;

    //Ranking
    [SerializeField] private Ranking ranking;
    [SerializeField] private TMP_Text Players1;
    [SerializeField] private TMP_Text Players2;
    [SerializeField] private TMP_Text Players3;
    [SerializeField] private TMP_Text Players4;
    [SerializeField] private TMP_Text Players5;
    [SerializeField] private TMP_Text Score1;
    [SerializeField] private TMP_Text Score2;
    [SerializeField] private TMP_Text Score3;
    [SerializeField] private TMP_Text Score4;
    [SerializeField] private TMP_Text Score5;




    private void Start()
    {
        InputName inputName = InputName.Instance;
        //inputName.HideInputField(); // 入力フォームを非表示にする
        resultTitleRT = resultTitleText.gameObject.GetComponent<RectTransform>();
        knockedOutPinCountRT = knockedOutPinCountText.gameObject.GetComponent<RectTransform>();
        informationPinCG = informationPinRT.gameObject.GetComponent<CanvasGroup>();
        throwCountText = throwCountRT.gameObject.GetComponent<TextMeshProUGUI>();
        messageBackgroundImg = messageBackgroundRT.gameObject.GetComponent<Image>();
        playinglifeBackgroundImg = playinglifeBackgroundRT.gameObject.GetComponent<Image>();

        //Result�ǉ���
        CLEAR1TextRT = CLEAR1Text.gameObject.GetComponent<RectTransform>();
        CLEAR2TextRT = CLEAR2Text.gameObject.GetComponent<RectTransform>();
        CLEAR3TextRT = CLEAR3Text.gameObject.GetComponent<RectTransform>();
        CLEAR4TextRT = CLEAR4Text.gameObject.GetComponent<RectTransform>();
        CLEAR5TextRT = CLEAR5Text.gameObject.GetComponent<RectTransform>();
        knockedOutPinC1RT = knockedOutPinC1Text.gameObject.GetComponent<RectTransform>();
        knockedOutPinC2RT = knockedOutPinC2Text.gameObject.GetComponent<RectTransform>();
        knockedOutPinC3RT = knockedOutPinC3Text.gameObject.GetComponent<RectTransform>();
        knockedOutPinC4RT = knockedOutPinC4Text.gameObject.GetComponent<RectTransform>();
        knockedOutPinC5RT = knockedOutPinC5Text.gameObject.GetComponent<RectTransform>();
    }

    private void Update()
    {
    }

    internal void startMenuUI()
    {
        startMenuRT.localScale = Vector3.one;
        startMenuTitletext.text = "<color=#FF8E00>V</color>IRTUAL" + "\n"+ "BOWLING <color=#FF8E00>!</color>!" + "\n"+"<size=80> -2026- ";
        StageNumberRT.transform.localScale = Vector3.zero;
    }
    internal void startMenuUI1()//105pin
    {
        startMenuRT.localScale = Vector3.one;
        startMenuTitletext.text = "<color=#FF8E00>V</color>irtual"+"\n"+"<color=#FF8E00>B</color>OWLING<color=#FF8E00>!</color>!"+"\n"+"<size=70> -105Pins- ";
        StageNumberRT.transform.localScale = Vector3.zero;
    }

    // Information Pin�̕\�������Z�b�g����
    internal void ResetInformationPins()
    {
        for (int i = 0; i < informationPinImageArr.Count; i++)
        {
            informationPinImageArr[i].color = Color.white;
        }
    }
    internal void Pins105PlayingUI()
    {
        informationPinRT.transform.localScale = Vector3.zero;
        TargetTextRT.transform.localScale = Vector3.zero;
        StageNumberRT.transform.localScale = Vector3.zero;
        Pins105CountText.transform.localScale = Vector2.one;
        Pins105CountText.transform.localPosition = new Vector3(400, -170, 0);
        onesResult = false;
    }

    internal void Pins105RTOFFUI()
    {
        Pins105CountText.transform.localScale = Vector2.zero;
    }

    bool onesResult = false;
    internal void Pins105countUI()
    {
        if(gameController.ballCount >= 1 && !onesResult)
        {
            Pins105CountText.text = gameController.Pins105AllCount.ToString();
        }
        else if(!onesResult)
        {
            Pins105CountText.text = "??";
        }
        informationPinRT.transform.localScale = Vector3.zero;
        TargetTextRT.transform.localScale = Vector3.zero;
        StageNumberRT.transform.localScale = Vector3.zero;
    }

    internal void ShowPlayingLifeBackground()
    {
        playinglifeBackgroundRT.localScale = Vector3.one;
    }

    internal void HidePlayingLifeBackground()
    {
        playinglifeBackgroundRT.localScale = Vector3.zero;
    }

    internal void ShowInformationPins()
    {
        informationPinRT.localScale = Vector3.one;
    }

    internal void HideInformationPins()
    {
        informationPinRT.localScale = Vector3.zero;
    }

    internal void DisableInformationPinAt(int index)
    {
        informationPinImageArr[index].color = Color.black;
    }

    internal void DisableResultPinAt(int throwNumber, int pinNumber)
    {
        var v = 0.3529f;
        resultPinList[throwNumber].imageList[pinNumber].color = new Color(v, v, v);
        resultPinList[throwNumber].rectTransformList[pinNumber].localScale = new Vector3(0.65f, 0.65f, 1f);
    }

    internal void ShowResultUI()
    {
        
        informationPinRT.localScale = Vector3.zero;
        startMenuRT.localScale = Vector3.zero;
        playinglifeBackgroundRT.localScale = Vector3.zero;

        if (gameController.stageSerectNumber == 0)
        {
            float stagescoreCntArr = 0f;

            var stagescoreCnt = new TextMeshProUGUI[5] { knockedOutPinC1Text, knockedOutPinC2Text, knockedOutPinC3Text, knockedOutPinC4Text, knockedOutPinC5Text };
            for (int i = 0; i < 5; i++)
            {
                stagescoreCnt[i].text = gameController.knockedOutPinsCountArr[i].ToString();
                stagescoreCntArr = stagescoreCntArr + gameController.knockedOutPinsCountArr[i];
            }
            knockedOutPinCountText.text = stagescoreCntArr.ToString();
            resultRT.localScale = Vector3.one;

            PlayResultUIAnimation();
        }
        else if(gameController.stageSerectNumber == 1)
        {
            Pins105CountText.transform.localPosition = new Vector3(0, 0, 0);
            Pins105CountText.transform.localScale = Vector3.one;
            onesResult = true;
            StartCoroutine(Pins105UIAnimation());

        }
    }

    private IEnumerator InputformAnimation(){//遷移やアニメーション？系がよくわかってないｗ
        
        InputName inputName = InputName.Instance;
        inputName.ShowInputField();//入力フォームを表示
        //アニメーションの長さ分だけ待機   
        float animationDelay = 3.0f;
        yield return new WaitForSeconds(animationDelay);
    }

    private IEnumerator Pins105UIAnimation()
    {
        int y = 0;
        bool x = true;
        yield return new WaitForSeconds(1f);
        while (x)
        {
            yield return new WaitForSeconds(0.07f);
            Pins105CountText.text = y.ToString();
            y += 1;
            if(y >= gameController.Pins105AllCount)
            {
                x = false;
            }
        }
        //Debug.Log(y);

    }

    private void PlayResultUIAnimation()
    {
        // animation initialize
        Vector2 pos;
        resultBackgroundImg.color = Color.clear;
        resultTitleText.alpha = 0f;
        knockedOutPinCountText.alpha = 0f;
        CLEAR1Text.alpha = 0f;
        CLEAR2Text.alpha = 0f;
        CLEAR3Text.alpha = 0f;
        CLEAR4Text.alpha = 0f;
        CLEAR5Text.alpha = 0f;
        ballcountplasText.alpha = 0f;
        Vector3 CLEARsize;
        CLEARsize = new Vector3(0.4f, 0.4f, 0.4f);

        // do animation
        // result background
        resultBackgroundImg.DOFade(98f / 255f, 1f);

        // result title
        resultTitleText.DOFade(1f, 0f).SetDelay(1f);
        pos = resultTitleRT.anchoredPosition;
        pos.y -= 30f;
        resultTitleRT.anchoredPosition = pos;
        pos.y += 30f;
        resultTitleRT.DOAnchorPos(pos, 0.3f);


        var clearTexts = new TextMeshProUGUI[5] { CLEAR1Text, CLEAR2Text, CLEAR3Text, CLEAR4Text, CLEAR5Text };
        var clearTextRTs = new RectTransform[5] { CLEAR1TextRT, CLEAR2TextRT, CLEAR3TextRT, CLEAR4TextRT, CLEAR5TextRT };

        for (int i = 0; i < gameController.stageNumber; i++)
        {
            clearTextRTs[i].localScale = new Vector3(3f, 3f, 1f);
            clearTextRTs[i].DOScale(CLEARsize, 0.3f).SetEase(Ease.InQuart).SetDelay(2f);
            clearTextRTs[i].DOShakeAnchorPos(0.5f, 10f, 15).SetDelay(2f);
            clearTexts[i].DOFade(1f, 0f).SetDelay(2f);
        }



        Invoke("knockoutpinscounttext", 1.5f);
    }

    void knockoutpinscounttext()
    {
        // result knocked out pins count
        knockedOutPinCountRT.localScale = new Vector3(3f, 3f, 1f);
        knockedOutPinCountRT.DOScale(Vector3.one, 0.3f).SetEase(Ease.InQuart).SetDelay(2f);
        knockedOutPinCountRT.DOShakeAnchorPos(0.5f, 10f, 15).SetDelay(2f);
        knockedOutPinCountText.DOFade(1f, 0f).SetDelay(2f);
    }

    internal void ScreenWhiteOut(UnityAction callback)
    {
        float delay = 0.2f;
        whitePanelImg.DOFade(1f, delay);
        whitePanelImg.DOFade(0f, delay).SetDelay(delay);

        DOVirtual.DelayedCall(delay, () => callback());
    }

    internal void ShowStartMenu()
    {
        //�X�^�[�g���Ƀv���C��ʂ�UI���\������Ă��Ȃ��悤�ɂ��Ă��܂�
        informationPinRT.localScale = Vector3.zero;
        resultRT.localScale = Vector3.zero;
        playinglifeBackgroundRT.localScale = Vector3.zero;
        Pins105CountText.transform.localScale = Vector3.zero;
        Debug.Log("ShowStart");
        startMenuRT.localScale = Vector3.one;

        _FlowAwayText = StartCoroutine(FlowAwayText(startMenuMessageRT));
    }

    internal void ShowRanking() 
    {
        ranking.LoadScoreData();
        Players1.SetText(ranking.scores[0].playerName);
        Players2.SetText(ranking.scores[1].playerName);
        Players3.SetText(ranking.scores[2].playerName);
        Players4.SetText(ranking.scores[3].playerName);
        Players5.SetText(ranking.scores[4].playerName); 
        Score1.SetText(ranking.scores[0].score.ToString());
        Score2.SetText(ranking.scores[1].score.ToString());
        Score3.SetText(ranking.scores[2].score.ToString());
        Score4.SetText(ranking.scores[3].score.ToString());
        Score5.SetText(ranking.scores[4].score.ToString());

    }

    private Coroutine _FlowAwayText;
    private IEnumerator FlowAwayText(RectTransform textRT)
    {
        Vector2 pos = textRT.anchoredPosition;
        var shortWait = new WaitForSeconds(0.3f);
        var longWait = new WaitForSeconds(1.5f);
        while (true)
        {
            pos.y += 9f;
            textRT.DOAnchorPos(pos, 0.3f).SetEase(Ease.Linear);
            yield return shortWait;

            pos.y -= 9f;
            textRT.DOAnchorPos(pos, 1.5f);
            yield return longWait;
        }
    }

    internal void ShowPlayingUI()
    {
        var pos1 = startMenuTitleRT.anchoredPosition;
        var pos2 = startMenuBackgroundRT.anchoredPosition;
        startMenuTitleRT.DOAnchorPosY(500f, 0.5f);
        startMenuBackgroundRT.DOAnchorPosY(-400f, 0.5f);

        informationPinRT.localScale = Vector3.one;
        playinglifeBackgroundRT.localScale = Vector3.one;


        //stagenumber���Q�Ƃ�stage���ɔ��f����
        stageNumberUI();
        scoreTargetUI();

        ballcountUI(gameController.ballCount);

        var infoPos = informationPinRT.anchoredPosition;
        infoPos.x -= 30f;
        informationPinRT.DOAnchorPos(infoPos, 0f);
        infoPos.x += 30f;
        informationPinRT.DOAnchorPos(infoPos, 0.5f);
        informationPinCG.alpha = 0f;
        informationPinCG.DOFade(1f, 0.5f);



        DOVirtual.DelayedCall(0.5f, () =>
        {
            resultRT.localScale = Vector3.zero;
            startMenuRT.localScale = Vector3.zero;

            startMenuTitleRT.anchoredPosition = pos1;
            startMenuBackgroundRT.anchoredPosition = pos2;

            if (_FlowAwayText != null)
            {
                StopCoroutine(_FlowAwayText);
                _FlowAwayText = null;
            }
        });
    }

    internal void scoreTargetUI()
    {
        TargetTextRT.localScale = Vector3.one;
        if(gameController.stageNumber != 5)
            TargetText.text = $"<size=120%><color=#FF2E00>{stageinfoArr[gameController.stageNumber].passingLine.pinsCount}</size><size=120%> <color=#FF6F00>本以上倒せ! </size>";
    }

    internal void stageNumberUI()
    {
        StageNumberRT.localScale = Vector3.one;
        StageNumberText.text = $"<size=100%><color=#73E6F7>ステージ</size> <color=#73E6F7>{gameController.stageNumber + 1}";
    }

    //�Ăяo�����ƂŎw�肵�����̃{�[������\�����܂�
    internal void ballcountUI(int ballCnt)
    {

        if (ballCnt == 0)
        {
            ballcountplasTextRT.localScale = Vector3.zero;
            ballcountNumber[0].color = Color.black;
            ballcountNumber[1].color = Color.black;
        }
        else if (ballCnt == 1)
        {
            ballcountplasTextRT.localScale = Vector3.zero;
            ballcountNumber[0].color = Color.white;
            ballcountNumber[1].color = Color.black;
        }
        else if (ballCnt == 2)
        {
            ballcountplasTextRT.localScale = Vector3.zero;
            ballcountNumber[0].color = Color.white;
            ballcountNumber[1].color = Color.white;
        }
        else if (ballCnt >= 3)
        {
            ballcountNumber[0].color = Color.white;
            ballcountNumber[1].color = Color.white;
            ballcountplasTextRT.localScale = Vector3.one;
            ballcountplasText.text = $"<size=70%>+ {ballCnt - 2}</size>";
        }
    }

    internal void ShowThrowCountText(int throwCnt,int ballCnt)
    {
        var duration = 0.5f;
        var x = 30f;

        throwCountText.text = $"<color=#FF4906>{throwCnt}</color><size=70>投目</size>";
        

        // background
        throwCountBackgroundImg.color = Color.clear;
        throwCountBackgroundImg.DOFade(134f / 255f, duration);

        throwCountText.alpha = 0f;
        throwCountText.DOFade(1f, duration);

        var pos = throwCountRT.anchoredPosition;
        pos.x += x;
        throwCountRT.anchoredPosition = pos;
        pos.x -= x;
        throwCountRT.DOAnchorPos(pos, duration);

        DOVirtual.DelayedCall(2f, () =>
        {
            var pos2 = throwCountRT.anchoredPosition;
            pos2.x -= x;
            throwCountRT.DOAnchorPos(pos2, duration);

            throwCountBackgroundImg.DOFade(0f, duration);
            throwCountText.DOFade(0f, duration);

            pos2.x += x;
            throwCountRT.DOAnchorPos(pos2, 0f).SetDelay(duration);
        });
    }

    // replayAnimationImgArr��[1]����
    [SerializeField] private RectTransform canvasRT;
    [SerializeField] private RectTransform[] replayAnimationImgRTArr;
    [SerializeField] private CanvasGroup replayAnimationImgCG;
    internal IEnumerator PlayTransferringReplayScreenAnimation(float delay, UnityAction callback)
    {
        // initialize
        int i;
        var dPos = new Vector2(canvasRT.sizeDelta.x / -2, canvasRT.sizeDelta.y / 2);
        for (i = 1; i < replayAnimationImgRTArr.Length; i++)
        {
            replayAnimationImgRTArr[i].anchoredPosition = dPos;
        }

        yield return new WaitForSeconds(delay);

        var pos = new Vector3(canvasRT.sizeDelta.x / 2, canvasRT.sizeDelta.y / -2, 0f);
        var endX = canvasRT.sizeDelta.x / 2f;

        var angle = replayAnimationImgRTArr[1].eulerAngles.z;
        var height = replayAnimationImgRTArr[1].sizeDelta.y;

        var wait = new WaitForSeconds(0.1f);
        var sin = Mathf.Sin(angle * (Mathf.PI / 180)) * height;
        var cos = Mathf.Cos(angle * (Mathf.PI / 180)) * height;
        i = 1;
        replayAnimationImgCG.DOFade(1f, 1f);
        while (pos.x - cos < endX && i < replayAnimationImgRTArr.Length)
        {
            replayAnimationImgRTArr[i].DOLocalMove(pos, 0.35f).SetEase(Ease.OutCubic);

            pos.x -= sin;
            pos.y += cos;
            i++;

            yield return wait;
        }
        yield return new WaitForSeconds(0.3f);

        callback?.Invoke();

        replayAnimationImgCG.DOFade(0f, 0.15f);
    }

    internal void ShowMessage(string text)
    {
        messageBackgroundRT.localScale = Vector3.one;
        messageBackgroundImg.DOFade(1f, 0f);

        var bgDefaultSize = messageBackgroundRT.sizeDelta;
        var bgDefaultPos = messageBackgroundRT.anchoredPosition;
        var bgPos = bgDefaultPos;
        bgPos.x -= bgDefaultSize.x / 2;
        messageBackgroundRT.anchoredPosition = bgPos;
        messageBackgroundRT.sizeDelta = new Vector2(0f, bgDefaultSize.y);

        messageBackgroundRT.DOAnchorPos(bgDefaultPos, 1f).SetEase(Ease.OutQuart);
        messageBackgroundRT.DOSizeDelta(bgDefaultSize, 1f).SetEase(Ease.OutQuart);

        messageText.DOFade(0f, 0f);
        messageText.DOFade(1f, 1f).SetEase(Ease.OutQuart);

        messageText.text = text;
        messageText.characterSpacing = 0f;
        StartCoroutine(IncreaseCharacterSpace(messageText, 20f, 4f));
    }

    internal void HideMessage()
    {
        messageBackgroundImg.DOFade(0f, 0.2f);
        messageText.DOFade(0f, 0.2f);

        DOVirtual.DelayedCall(0.2f, () => messageBackgroundRT.localScale = Vector3.zero);
    }

    private IEnumerator IncreaseCharacterSpace(TextMeshProUGUI text, float toValue, float duration)
    {
        var diff = toValue - text.characterSpacing;
        var diffPerSecond = diff / duration;
        var time = 0f;
        while (time < duration)
        {
            text.characterSpacing += diffPerSecond * Time.deltaTime;
            time += Time.deltaTime;

            yield return null;
        }
        text.characterSpacing = toValue;
    }
}



// 2�������X�g���V���A���C�Y���邽�߂ɗ��p
[System.Serializable]
public class ResultPinList
{
    public List<Image> imageList;
    public List<RectTransform> rectTransformList;
}


