using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using DG.Tweening;

public class GameContoller : MonoBehaviour
{
    [SerializeField] private UIController uiController;
    [SerializeField] private SoundManager soundManager;
    [SerializeField] private Example JoyconController;
    [SerializeField] private GameModeData GameModeData;
    [SerializeField] private Ranking ranking;
    [SerializeField] private InputName inputName;
    [SerializeField] private OKbutton oKbutton;

    [SerializeField] private StageInfo[] stageinfoArr;
    [SerializeField] private StageInfo[] stageInfoSPArr;

    [SerializeField] internal int stageNumber; 
    [SerializeField] internal int stageSerectNumber;

    [SerializeField] private bool showStartMenu;
    [SerializeField] internal int ballCount; // 残りの投球数
    private int initialBallCount;

    [SerializeField] private GameObject mainPinsPrefab;
    [SerializeField] private GameObject Pins105Prefab;
    [SerializeField] private GameObject ballPrefab;



    private Camera mainCamera;

    private int throwBallCountInCurrentStage; // 現在のステージで何投目か（UI用）
    private int knockedOutAllPinCount;
    internal int[] knockedOutPinsCountArr;

    [SerializeField] private string velocityDataPath;
    private float knockoutLevel = 0.2f;

    private string lastWriteTime; // vector.txt

    private GameObject ball;
    private Rigidbody ballRb;
    private List<GameObject> mainPinList = new List<GameObject>();
    private List<GameObject> Pins105List = new List<GameObject>();
    private List<Transform> mainPinTransformList = new List<Transform>();
    private List<Rigidbody> mainPinRigidbodyList = new List<Rigidbody>();
    private List<Transform> Pins105TransformList = new List<Transform>();
    private List<Rigidbody> Pins105RigidbodyList = new List<Rigidbody>();

    private GameObject[] pinsObjectsArray;
    private GameObject[] pinsObjectsSPArray;
    private bool[] prePinStatus;
    private bool[] pinStatus;
    internal bool[] isClear;
    internal List<List<bool>> pinStatusArrList = new List<List<bool>>();

    private Vector3 addedForceVector;
    private float vectorZ;

    private bool isBallRolling;
    private bool isAllPinKnockedOut;
    private bool shouldSkipStartMenu;
    private bool shouldPlayTrasnferStartMenuAnimation;
    internal int Pins105AllCount = 0;
    float cameraZ = 0;
    float CameraTime = 0;

    private enum GameStatus
    {
        StartMenu,
        Playing
    }
    private GameStatus state;

    private enum SplitMode
    {
        None,
        Mode1,
        Mode2,
        Mode3,
        Mode4
    }
    static private SplitMode splitMode;
    internal enum CameraMode
    {
        Default,
        Follow,
        LaneEnd,
        Replay,
        Pins105
    }
    internal CameraMode cameraMode;

    public enum GameMode
    {
        Standard,
        Pins105
    }
    public GameMode gameMode;

    // 2025年2年追加
    // ガーターの3Dオブジェクトを格納する変数です。
    [SerializeField] private GameObject[] Gutters;
    private bool IsGutterEasy = false;

    private void Start()
    {
        mainCamera = Camera.main;
        cameraZ = 0;
        knockedOutPinsCountArr = new int[stageinfoArr.Length];
        initialBallCount = ballCount;
        JoyconController.JoyConThrowRest();

        lastWriteTime = File.GetLastWriteTime(velocityDataPath).ToString();
        ChangeCameraMode(CameraMode.Default);
        state = GameStatus.StartMenu;

        stageSerectNumber = GameModeData.GameModeNumber;

        soundManager.Morning_BGM();
        if (gameMode == GameMode.Standard || stageSerectNumber == 0)
        {
            uiController.startMenuUI();
            gameMode = GameMode.Standard;
            stageSerectNumber = 0;
        }
        else if (gameMode == GameMode.Pins105 || stageSerectNumber == 1)
        {
            uiController.startMenuUI1();
            gameMode = GameMode.Pins105;
            stageSerectNumber = 1;
        }
        ResetGame();
        
    }

    private void Update()
    {
        if (state == GameStatus.StartMenu)
        {
            if ((Input.GetKeyDown(KeyCode.Space) || shouldSkipStartMenu) && showStartMenu || JoyconController.AButtonR())
            {
                TransferPlayMode();
                shouldSkipStartMenu = false;
                if (gameMode == GameMode.Pins105)
                {
                    uiController.Pins105PlayingUI();
                }
                JoyconController.JoyConThrowRest();
            }
        }
        else if (state == GameStatus.Playing)
        {
            if (gameMode == GameMode.Standard)
            {
                uiController.scoreTargetUI();
                uiController.stageNumberUI();

                UpdateInformationPins();

                // vector.txtが更新されたらBallを投げる
                ReadVelocityAndSpawnBallIfNeeded();
                if (ball == null)
                {
                    ChangeCameraMode(CameraMode.Default);
                }

            }
            else if (gameMode == GameMode.Pins105)
            {
                uiController.Pins105countUI();
            }

            ObserveBallVelocity();
            if (Input.GetKeyDown(KeyCode.W))//テストスロー(前投げ)
            {
                // vector3は第一引数が速さ(前後のベクトル)、第二引数が高さ、第三引数が左右
                Vector3 sample_vec = new Vector3(-4000f, 10f, 0f);
                if (ball == null)
                {
                    ThrowBall(sample_vec);
                }
            }
            else if (Input.GetKeyDown(KeyCode.D))//サイドスロー(右投げ)
            {
                Vector3 sample_vec = new Vector3(-1500f, 10f, 500f);//zの値を200fから500fに変更→ガーター出てる時にすごく跳ね返る！
                if (ball == null)
                {
                    ThrowBall(sample_vec);
                }
            }
            else if (Input.GetKeyDown(KeyCode.A))//サイドスロー(左投げ)
            {
                Vector3 sample_vec = new Vector3(-1500f, 10f, -200f);
                if (ball == null)
                {
                    ThrowBall(sample_vec);
                }
            }

            JoyconController.JoyConThrow();
            if (JoyconController.ballThrows)
            {
                JoyconController.JoyConThrowRest();
                Vector3 vec = new Vector3(JoyconController.Power, 10f, JoyconController.PowerZ);
                if (ball == null)
                {
                    ThrowBall(vec);
                }
            }

            CameraTime += Time.deltaTime;

            if (CameraTime > 0.1 && ball == null)
            {
                if (JoyconController.Rstick() > 0.1)
                {
                    cameraZ += 0.05f;
                    CameraTime = 0;
                }
                else if (JoyconController.Rstick() < -0.1)
                {
                    cameraZ -= 0.05f;
                    CameraTime = 0;
                }
            }


        }

        // Gを押下でガターが高くなって壁になる
        if (Input.GetKeyDown(KeyCode.G))
        {
            Vector3 EasyGutterScale = new Vector3(1.0f, 1.0f, 3.0f);
            Vector3 DefGutterScale = new Vector3(1.0f, 1.0f, 1.0f);

            // DOTweenを使ってスムーズにスケールをアニメーションします。
            foreach (GameObject Gutter in Gutters)
            {
                if (!IsGutterEasy)
                {
                    Gutter.transform.DOScale(EasyGutterScale, 1.0f).SetEase(Ease.OutQuad);
                    Debug.Log(IsGutterEasy);
                }
                else
                {
                    Gutter.transform.DOScale(DefGutterScale, 1.0f).SetEase(Ease.OutQuad);
                    Debug.Log(IsGutterEasy);
                }
            }
            IsGutterEasy = !IsGutterEasy;
        }
        else if (Input.GetKeyDown(KeyCode.Return)) //タイトル画面に移行
        {
            //SceneManager.LoadScene(1);
        }
        else if (Input.GetKeyDown(KeyCode.Escape)) // 1投目に移行
        {
            SceneManager.sceneLoaded += OnReloadSceneAndTransferPlayMode;
            SceneManager.LoadScene(1);
        }
        else if (Input.GetKeyDown(KeyCode.C)) // Split Modeに移行orモードチェンジ
        {
            //splitMode++;
            //if ((int)splitMode > 4) splitMode = SplitMode.None;
            //SetSplitMode(splitMode);
        }
        else if (Input.GetKeyDown(KeyCode.Z))//メニュー画面へ移行
        {
            //SceneManager.LoadScene(0);
        }
    }

    private void LateUpdate()
    {
        UpdateCameraMode();
    }

    // ボールの速さを監視する変数
    private float _keepingTimeBallVelocityPlusX;
    private void ObserveBallVelocity()
    {
        if (ballRb == null) return;
        Debug.Assert(ball != null);

        // ボールのX方向(前方向)の速度を"x"という変数に格納
        float x = ballRb.velocity.x;

        //ボールの速度が一定以上かどうか
        // ボールがほぼ止まっていことを条件に遷移(xの値は低いほどボールは早いため)
        if (x > -0.3f)
        {
            //経過時間を加算(Time.deltaTimeはフレーム間の時間(瞬間時間として扱う))
            _keepingTimeBallVelocityPlusX += Time.deltaTime;
        }
        else
        {
            //経過時間をリセット
            _keepingTimeBallVelocityPlusX = 0;
        }

        // ボールの速度の値が一定時間、高いままかどうか
        // ボールの速度が早すぎると、壁を貫通するため半永久的に_keepingTimeBallVelocityPlusXが0のままになり遷移しない。
        if (_keepingTimeBallVelocityPlusX > 3f && gameMode == GameMode.Standard || _keepingTimeBallVelocityPlusX > 9f && gameMode == GameMode.Pins105)
        {
            //監視している変数をリセットし、カメラをレーンエンドに移行する
            _keepingTimeBallVelocityPlusX = 0;

            cameraMode = CameraMode.LaneEnd;

            //次の投球へ移行またはゲーム終了の関数を実行
            TransferNextThrowingOrFinishGame();
        }
    }

    private void OnReloadSceneAndTransferPlayMode(Scene scene, LoadSceneMode mode)
    {
        GameObject.FindGameObjectWithTag("GameController").GetComponent<GameContoller>().shouldSkipStartMenu = true;
        SceneManager.sceneLoaded -= OnReloadSceneAndTransferPlayMode;
    }

    private void SetSplitMode(SplitMode mode)
    {
        if (isBallRolling)
        {
            Debug.LogWarning("SetSplitMode() : cannot set split mode white ball is rolling!");
            return;
        }
        Debug.Log($"Set Split Mode {mode}");

        InstantiateAllPins();
        switch (mode)
        {
            case SplitMode.Mode1:
                DestroyPinAt(1, 3, 5, 6, 8, 9);
                break;

            case SplitMode.Mode2:
                DestroyPinAt(1, 2, 4, 5, 8, 9);
                break;

            case SplitMode.Mode3:
                DestroyPinAt(1, 2, 3, 4, 5, 6, 7, 9);
                break;

            case SplitMode.Mode4:
                DestroyPinAt(1, 2, 3, 4, 5, 6, 8, 10);
                break;

            default:
                break;
        }
    }

    private void TransferPlayMode()
    {
        Debug.Log("To Playing Status");

        state = GameStatus.Playing;
        uiController.ShowPlayingUI();
        uiController.ShowThrowCountText(throwBallCountInCurrentStage + 1,  ballCount - 1);
        soundManager.PlayThrow_1_SE();
    }

    // Pin近くのTriggerにEnterしたときに呼ばれる
    private bool hasLaneTriggerEntered;
    internal void OnLaneTriggerEnter(float delay)
    {
        if (hasLaneTriggerEntered) return;
        hasLaneTriggerEntered = true;

        _ExcuteCallbackIfAllPinHasBeenStill =
            StartCoroutine(ExcuteCallbackIfAllPinHasBeenStill(TransferNextThrowingOrFinishGame, 1f));
    }

    // 全てのピンが静止したならcallbackを実行する
    private Coroutine _ExcuteCallbackIfAllPinHasBeenStill;
    private IEnumerator ExcuteCallbackIfAllPinHasBeenStill(UnityAction callback, float delay)
    {
        yield return new WaitForSeconds(delay);

        bool isStill = false;
        while (!isStill)
        {
            isStill = true;
            if(gameMode == GameMode.Standard)
            {
                for (int i = 0; i < mainPinRigidbodyList.Count; i++)
                {
                    if (mainPinList[i] != null)
                    {
                        if (!mainPinRigidbodyList[i].IsSleeping())
                        {
                            isStill = false;
                            break;
                        }
                    }
                }
            }
            else if(gameMode == GameMode.Pins105)
            {
                for (int i = 0; i < Pins105RigidbodyList.Count; i++)
                {
                    if (Pins105List[i] != null)
                    {
                        if (!Pins105RigidbodyList[i].IsSleeping())
                        {
                            isStill = false;
                            break;
                        }
                    }
                }
            }

            yield return null;
        }
        _ExcuteCallbackIfAllPinHasBeenStill = null;
        callback();
    }

    // 次の投球へ移行するかゲームを終了する
    private void TransferNextThrowingOrFinishGame()
    {
        void ClearProcess()
        {
            isClear[stageNumber] = true;

            ballCount++; // 残りの投球数
            throwBallCountInCurrentStage = 0; // 現在のステージで何投目か
        }

        hasLaneTriggerEntered = false;
        if(gameMode == GameMode.Standard)
        {
            // 全てのピンが倒されたか
            isAllPinKnockedOut = true;
            for (int i = 0; i < stageinfoArr[stageNumber].pinsInfo.status.Length; i++)
            {
                if (pinStatus[i])//ピンが立っていればfalseを返す
                {
                    isAllPinKnockedOut = false;
                }
                else if (mainPinList[i] != null)
                {
                    knockedOutPinsCountArr[stageNumber]++;
                }
            }
            if (isAllPinKnockedOut) knockedOutAllPinCount++;
        }
        else if(gameMode == GameMode.Pins105)
        {
            // 全てのピンが倒されたか
            isAllPinKnockedOut = true;
            for (int i = 0; i < stageInfoSPArr[1].pinsInfo.status.Length; i++)
            {
                if (pinStatus[i])//ピンが立っていればfalseを返す
                {
                    isAllPinKnockedOut = false;
                }
                else if (Pins105List[i] != null)
                {
                    knockedOutPinsCountArr[stageNumber]++;
                }
            }
            if (isAllPinKnockedOut) knockedOutAllPinCount++;
        }

        var isStrike = false;

        // ストライクならリプレイを再生する
        if (isAllPinKnockedOut)
        {
            isStrike = true;
            // 歓声を再生する
            soundManager.Kansei_SE();
            for (int i = 0; i < prePinStatus.Length; i++)
            {
                if (!prePinStatus[i])
                {
                    isStrike = false;
                    break;
                }
            }
            if (isStrike)
            {
                ClearProcess();
                StartCoroutine(uiController.PlayTransferringReplayScreenAnimation(0f, TransferReplay));
            }
        }

        if (!isStrike)
        {
            JoyconController.JoyConThrowRest();
            if (gameMode == GameMode.Pins105)
            {
                if (ballCount >= 1)//残球数が1以上
                {
                    TransferThrowing(false); // 2投目へ
                }
                else //２球投げてノルマ未達成
                {
                    TransferResult();
                }
            }
            else if (stageinfoArr.Length - 1 == stageNumber)//最終ステージならストライクを出した後にリザルトへ
            {
                TransferResult();
            }
            else if (knockedOutPinsCountArr[stageNumber] >= stageinfoArr[stageNumber].passingLine.pinsCount) // ノルマ達成
            {
                stageNumber++; // 次のステージへ
                uiController.ballcountUI(ballCount);

                ClearProcess();
                TransferThrowing(true); // 次のステージに遷移&投球数を+1
            }
            else if (ballCount >= 1)//残球数が1以上
            {
                TransferThrowing(false); // 2投目へ
            }
            else //２球投げてノルマ未達成
            {
                TransferResult();
            }
        }
    }

    private void TransferReplay()
    {
        ChangeCameraMode(CameraMode.Replay);
        uiController.HideInformationPins();
        uiController.HidePlayingLifeBackground();

        InstantiateAllPins();
        for (int i = 0; i < prePinStatus.Length; i++)
        {
            if (!prePinStatus[i])
            {
                DestroyPinAt(i + 1);
            }
        }
        Destroy(ball);

        // ピン設置後に実行。prePinStatusが上書きされるため
        ThrowBall(addedForceVector, true);
    }

    internal void FinishReplay()
    {
        uiController.HideMessage();

        if (stageinfoArr.Length - 1 == stageNumber)
        {
            // resultへ遷移
            TransferResult();
        }
        else
        {
            // 次のステージへ遷移
            stageNumber++;
            uiController.ballcountUI(ballCount);
            uiController.ScreenWhiteOut(() => TransferThrowing(true));

        }
    }

    private void TransferResult()//リザルト画面へ遷移
    {
        Destroy(ball);
        isBallRolling = false;
        ranking.LoadScoreData();
        
        soundManager.Hakusyu_SE();
        uiController.ShowResultUI();
        shouldPlayTrasnferStartMenuAnimation = true;

        if (knockedOutPinsCountArr[stageNumber] >= 10)
        {
            soundManager.PlayCheersSE();
        }
        if (ranking.scores[4].score < Pins105AllCount)
        {
            inputName.ShowInputField();
            if (gameMode == GameMode.Standard)
            {
                stageNumber = 0;//リザルト画面に遷移した後ステージ１へ戻す
            }

        }
        else
        {
            if (gameMode == GameMode.Standard)
            {
                _DelayedCall = StartCoroutine(DelayedCall(5f, ResetGame));
                stageNumber = 0;//リザルト画面に遷移した後ステージ１へ戻す
            }
            else if (gameMode == GameMode.Pins105)
            {

                _DelayedCall = StartCoroutine(DelayedCall(10f, ResetGame));
            }
        }
        
        

    }

    private void TransferThrowing(bool playNextStage)//
    {
        state = GameStatus.Playing;
        if(gameMode == GameMode.Standard)
        {
            ChangeCameraMode(CameraMode.Default);
        }
        else if(gameMode == GameMode.Pins105)
        {
            ChangeCameraMode(CameraMode.Pins105);
        }

        uiController.ShowInformationPins();
        uiController.ShowPlayingLifeBackground();
        uiController.ShowThrowCountText(throwBallCountInCurrentStage + 1,ballCount - 1);
        if(throwBallCountInCurrentStage % 2 == 0)
        {
            soundManager.PlayThrow_1_SE();
        }
        else
        {
            soundManager.PlayThrow_2_SE();
        }
        // uiController.

        isBallRolling = false;
        Destroy(ball);

        // For split mode
        if (splitMode != SplitMode.None)
        {
            SetSplitMode(splitMode);
            return;
        }

        // 全てのピンが倒されているなら再生成する
        if (playNextStage)
        {
            InstantiateStagePins();
        }
        else//現在倒れているピンを消す
        {
            DestroyKnockedOutPins();
        }
    }

    private void ReadVelocityAndSpawnBallIfNeeded()
    {
        if (ball != null) return;

        // 最終書き込みの時間が更新されれば、書き換えられたと判断してファイルを読み込む
        if (lastWriteTime != File.GetLastWriteTime(velocityDataPath).ToString())
        {
            lastWriteTime = File.GetLastWriteTime(velocityDataPath).ToString();

            FileStream stream = new FileStream(velocityDataPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader sr = new StreamReader(stream);
            string filedata = sr.ReadToEnd();
            sr.Close();

            string[] vec_str = filedata.Split('\n');

            float x = float.Parse(vec_str[0]); // front
            float y = float.Parse(vec_str[1]); // height
            float z = float.Parse(vec_str[2]); // side

            Vector3 read_vec = new Vector3(z * 200, x * -40, y * -40);
            ThrowBall(read_vec);
        }
    }

    private void UpdateInformationPins()
    {
        if(gameMode == GameMode.Standard)
        {
            for (int i = 0; i < stageinfoArr[stageNumber].pinsInfo.status.Length; i++)
            {
                if (mainPinList[i] != null)
                {
                    float x = mainPinTransformList[i].rotation.x;
                    float z = mainPinTransformList[i].rotation.z;
                    if (x > knockoutLevel || -knockoutLevel > x || z > knockoutLevel || -knockoutLevel > z)
                    {
                        pinStatus[i] = false;
                        uiController.DisableInformationPinAt(i);
                    }
                }
                else
                {
                    pinStatus[i] = false;
                    uiController.DisableInformationPinAt(i);
                }
            }
        }
        else if(gameMode == GameMode.Pins105)
        {
            for (int i = 0; i < stageInfoSPArr[1].pinsInfo.status.Length; i++)
            {
                if (Pins105List[i] != null)
                {
                    float x = Pins105TransformList[i].rotation.x;
                    float z = Pins105TransformList[i].rotation.z;
                    if (x > knockoutLevel || -knockoutLevel > x || z > knockoutLevel || -knockoutLevel > z)
                    {
                        pinStatus[i] = false;
                        uiController.DisableInformationPinAt(i);
                    }
                }
                else
                {
                    pinStatus[i] = false;
                    uiController.DisableInformationPinAt(i);
                }
            }
        }
    }

    internal void ChangeCameraMode(CameraMode mode)
    {
        Vector3 pos;
        Transform cameraT = mainCamera.transform;
        switch (mode)
        {
            case CameraMode.Default:
                pos = new Vector3(9.5f, 1.0f, stageinfoArr[stageNumber].lanePositionZ + cameraZ);
                cameraT.position = pos;
                cameraT.eulerAngles = new Vector3(16.045f, -90f, 0f);
                break;

            case CameraMode.Replay:
                pos = new Vector3(-5.4f, 0.8f, 0.7f);
                cameraT.position = pos;
                break;
            case CameraMode.Pins105:
                pos = new Vector3(9.5f, 1.0f, stageInfoSPArr[1].lanePositionZ + cameraZ);
                cameraT.position = pos;
                cameraT.eulerAngles = new Vector3(16.045f, -90f, 0f);
                break;
        }
        cameraMode = mode;
    }

    private void UpdateCameraMode()
    {
        // Set camera position
        Vector3 pos;
        Transform cameraT = mainCamera.transform;
        switch (cameraMode)
        {
            case CameraMode.Follow://Z軸はlanePositionZを参照
                if(gameMode == GameMode.Standard)
                {
                    pos = new Vector3(ball.transform.position.x + 1, 1.0f, stageinfoArr[stageNumber].lanePositionZ + cameraZ);
                    cameraT.position = pos;
                }
                else if (gameMode == GameMode.Pins105)
                {
                    pos = new Vector3(ball.transform.position.x + 1, 1.0f, stageInfoSPArr[1].lanePositionZ + cameraZ);
                    cameraT.position = pos;
                }
                break;
            case CameraMode.Replay:
                cameraT.LookAt(ball.transform.position);
                break;
            case CameraMode.Pins105:
                pos = new Vector3(9.5f, 1.0f, stageInfoSPArr[1].lanePositionZ + cameraZ);
                cameraT.position = pos;
                cameraT.eulerAngles = new Vector3(16.045f, -90f, 0f);
                break;
        }
    }

    private void DestroyKnockedOutPins()
    {
        if(gameMode == GameMode.Standard)
        {
            for (int i = 0; i < stageinfoArr[stageNumber].pinsInfo.status.Length; i++)
            {
                if (mainPinTransformList[i] != null)
                {
                    float x = mainPinTransformList[i].rotation.x;
                    float z = mainPinTransformList[i].rotation.z;
                    if (x > knockoutLevel || -knockoutLevel > x || z > knockoutLevel || -knockoutLevel > z)
                    {
                        Destroy(mainPinList[i]);
                    }
                }
            }
        }
        else if(gameMode == GameMode.Pins105)
        {
            for (int i = 0; i < stageInfoSPArr[1].pinsInfo.status.Length; i++)
            {
                if (Pins105TransformList[i] != null)
                {
                    float x = Pins105TransformList[i].rotation.x;
                    float z = Pins105TransformList[i].rotation.z;
                    if (x > knockoutLevel || -knockoutLevel > x || z > knockoutLevel || -knockoutLevel > z)
                    {
                        Destroy(Pins105List[i]);
                    }
                }
            }
        }
    }

    private void DestroyPinAt(params int[] pinIndices)
    {
        if(gameMode == GameMode.Standard)
        {
            foreach (int index in pinIndices)
            {
                Destroy(mainPinList[index - 1]);
            }
        }
        else if (gameMode == GameMode.Pins105)
        {
            foreach (int index in pinIndices)
            {
                Destroy(Pins105List[index - 1]);
            }
        }
    }

    public void ResetGame()
    {
        cameraZ = 0;
        if (ball != null)
        {
            Destroy(ball);
        }
        ChangeCameraMode(CameraMode.Default);
        throwBallCountInCurrentStage = 0;
        JoyconController.JoyConThrowRest();
        

        for (int i = 0; i <= stageinfoArr.Length - 1; i++)
        {
            knockedOutPinsCountArr[i] = 0;
        }
        ballCount = initialBallCount;
        knockedOutAllPinCount = 0;
        isBallRolling = false;
        hasLaneTriggerEntered = false;
        state = GameStatus.StartMenu;
        pinStatusArrList.Clear();
        Debug.Log("ピンスコア："+Pins105AllCount);//Pins105AllCountとUIで出る倒したピンの数の値が異なっている。
        Ranking ranking = Ranking.Instance;
        if(ranking.scores.Count >= 6)
        {
            ranking.scores[5].score = Pins105AllCount;
            ranking.UpdateRankingScore(); // 代入後、スコアデータを保存する
        }
        
        Pins105AllCount = 0;
        inputName.HideInputField();
        oKbutton.ResetText();

        isClear = new bool[stageinfoArr.Length];

        if (shouldPlayTrasnferStartMenuAnimation)
        {
            uiController.ScreenWhiteOut(uiController.ShowStartMenu);
            uiController.ShowRanking();
        }
        else
        {
            uiController.ShowStartMenu();
            uiController.ShowRanking();
        }
        shouldPlayTrasnferStartMenuAnimation = false;

        soundManager.StopFadeOutBallRollSECoroutine();
        if (_DelayedCall != null)
        {
            StopCoroutine(_DelayedCall);
            _DelayedCall = null;
        }
        if (_ExcuteCallbackIfAllPinHasBeenStill != null)
        {
            StopCoroutine(_ExcuteCallbackIfAllPinHasBeenStill);
            _ExcuteCallbackIfAllPinHasBeenStill = null;
        }

        if (gameMode == GameMode.Pins105)
        {
            ChangeCameraMode(CameraMode.Pins105);
            uiController.ShowStartMenu();
            uiController.ShowRanking();
        }

        InstantiateStagePins();
        if (splitMode != SplitMode.None)
            SetSplitMode(splitMode);

        if (!showStartMenu)
        {
            DOVirtual.DelayedCall(1f, () => TransferPlayMode());
        }
    }

    private void UpdateScoreAndName(string playerName, int playerScore)
    {
        if(Ranking.Instance.scores.Count > 0)
        {
            Ranking.Instance.scores[0].playerName = playerName;
            Ranking.Instance.scores[0].score = playerScore;
            //Ranking.Instance.SaveScoreData();
        }
    }

    private void InstantiateStagePins()
    {
        InstantiateAllPins();

        var status = stageinfoArr[stageNumber].pinsInfo.status;
        Debug.Assert(status.Length == 10);

        for (int i = 0; i < stageinfoArr[stageNumber].pinsInfo.status.Length; i++)
        {
            if (status[i] == PinsInfo.Status.None)
            {
                DestroyPinAt(i + 1);
            }
        }
    }

    // Main Pinsを既存のオブジェクトを破棄して再生成する
    private void InstantiateAllPins()
    {
        if (pinsObjectsArray != null)
        {
            for (int i = 0; i < stageinfoArr.Length; i++)
            {
                Destroy(pinsObjectsArray[i]);
            }
        }
        if (pinsObjectsSPArray != null)
        {
            for (int i = 0; i < stageInfoSPArr.Length; i++)
            {
                Destroy(pinsObjectsSPArray[i]);
            }
        }

        var pos = new Vector3(-6f, 0.2564f, 0f);
        if (gameMode == GameMode.Standard)
        {
            pinStatus = new[]{true, true, true, true, true, true, true, true, true, true};
            pinsObjectsArray = new GameObject[5];
            for (int i = 0; i < stageinfoArr.Length; i++)
            {
                pos.z = stageinfoArr[i].lanePositionZ;
                pinsObjectsArray[i] = Instantiate(mainPinsPrefab, pos, Quaternion.identity);
            }

            mainPinList = new List<GameObject>();
            mainPinTransformList = new List<Transform>();
            mainPinRigidbodyList = new List<Rigidbody>();
            for (int i = 0; i < stageinfoArr[stageNumber].pinsInfo.status.Length; i++)
            {
                GameObject pre_pin = pinsObjectsArray[stageNumber].transform.Find("Pin" + (i + 1).ToString()).gameObject;
                if (pre_pin != null)
                {
                    mainPinList.Add(pre_pin);
                    mainPinTransformList.Add(pre_pin.transform);
                    mainPinRigidbodyList.Add(pre_pin.GetComponent<Rigidbody>());
                }
            }
            SleepAllPins(pinsObjectsArray);
            uiController.ResetInformationPins();
        }
        if (gameMode == GameMode.Pins105)
        {
            pinStatus = new[] 
            {
            true, true, true, true, true, true, true, true, true, true, 
            true, true, true, true, true, true, true, true, true, true,
            true, true, true, true, true, true, true, true, true, true,
            true, true, true, true, true, true, true, true, true, true,
            true, true, true, true, true, true, true, true, true, true,
            true, true, true, true, true, true, true, true, true, true,
            true, true, true, true, true, true, true, true, true, true,
            true, true, true, true, true, true, true, true, true, true,
            true, true, true, true, true, true, true, true, true, true,
            true, true, true, true, true, true, true, true, true, true
            };
            pinsObjectsSPArray = new GameObject[2];
            pos.z = stageInfoSPArr[1].lanePositionZ;
            pinsObjectsSPArray[1] = Instantiate(Pins105Prefab, pos, Quaternion.identity);

            Pins105List = new List<GameObject>();
            Pins105TransformList = new List<Transform>();
            Pins105RigidbodyList = new List<Rigidbody>();
            for (int i = 0; i < stageInfoSPArr[1].pinsInfo.status.Length; i++)
            {
                GameObject pre_pin = pinsObjectsSPArray[1].transform.Find("Pin" + (i + 1).ToString()).gameObject;
                if (pre_pin != null)
                {
                    Pins105List.Add(pre_pin);
                    Pins105TransformList.Add(pre_pin.transform);
                    Pins105RigidbodyList.Add(pre_pin.GetComponent<Rigidbody>());
                }
            }
            SleepAllPins(pinsObjectsSPArray);
        }
    }

    // 置いた直後のピンを眠らせる (2026 追加)。
    // 床との小さなめり込みや段差で、ボールが当たる前にピンが傾いて倒れるのを防ぐ。
    // 眠ったピンはボールや他のピンが当たると自動で起きるので、倒れ方には影響しない。
    private void SleepAllPins(GameObject[] pinSets)
    {
        if (pinSets == null) return;
        foreach (GameObject pinSet in pinSets)
        {
            if (pinSet == null) continue;
            foreach (Rigidbody rb in pinSet.GetComponentsInChildren<Rigidbody>())
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.Sleep();
            }
        }
    }

    private void ThrowBall(Vector3 vector, bool isReplaying = false)
    {
        JoyconController.JoyConThrowRest();
        addedForceVector = vector;
        Vector3 spawn = new Vector3(9.5f, 0.5f, 0f);
        if(gameMode == GameMode.Standard)
        {
            spawn = new Vector3(9.5f, 0.5f, stageinfoArr[stageNumber].lanePositionZ + cameraZ);//Z軸はlanePsitionZを参照
            // Pinのサウンドは一回しかならないようにしているため、それをリセットする
            for (int i = 0; i < mainPinList.Count; i++)
            {
                if (mainPinList[i] != null)
                {
                    mainPinList[i].GetComponent<Pin>().ResetPinSound();
                }
            }
            prePinStatus = new bool[10];

            //速度制限投球(ボールが早すぎるとピンを貫通するため)
             if(vector.x < -2000f){
                 vector.x = -2000f;
             }
        }
        else if(gameMode == GameMode.Pins105)
        {
            spawn = new Vector3(9.5f, 0.5f, stageInfoSPArr[1].lanePositionZ + cameraZ);//Z軸はlanePsitionZを参照
            // Pinのサウンドは一回しかならないようにしているため、それをリセットする
            for (int i = 0; i < Pins105List.Count; i++)
            {
                if (Pins105List[i] != null)
                {
                    Pins105List[i].GetComponent<Pin>().ResetPinSound();
                }
            }
            prePinStatus = new bool[100];
        }
        pinStatus.CopyTo(prePinStatus, 0);

        ball = Instantiate(ballPrefab, spawn, Quaternion.identity);
        ballRb = ball.GetComponent<Rigidbody>();
        ballRb.AddForce(vector);

        soundManager.PlayThrowBallSE();
        soundManager.PlayBallRollSE(ball.GetComponent<AudioSource>());

        isBallRolling = true;

        if (!isReplaying)
        {
            ballCount--;//
            uiController.ballcountUI(ballCount);
            throwBallCountInCurrentStage++;
        }
    }

    // Invoke()などではEnterキー押下したときに中断できないため、これを利用する
    // DelayedCall()
    private Coroutine _DelayedCall;
    private IEnumerator DelayedCall(float delay, UnityAction callback)
    {
        yield return new WaitForSeconds(delay);
        _DelayedCall = null;

        callback();
    }
}
