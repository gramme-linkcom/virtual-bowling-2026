using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ボウリングのピンの挙動を制御するクラス
public class Pin : MonoBehaviour
{
    private SoundManager soundManager;      // サウンド管理用
    private GameContoller GameContoller;    // ゲーム全体の制御用
    private bool hasPlayedSound;            // 衝突音を再生済みかどうか
    bool ones = false;                      // ピンが倒れた判定を1回だけ行うためのフラグ

    // コンポーネント初期化
    private void OnEnable()
    {
        soundManager = Camera.main.gameObject.GetComponent<SoundManager>();
        GameContoller = GameObject.Find("GameController").GetComponent<GameContoller>();
        hasPlayedSound = true;
    }

    // 毎フレーム実行
    private void Update()
    {
        if(!ones)
        {
            // ピンの回転角度を取得
            float x = this.transform.rotation.x;
            float z = this.transform.rotation.z;
            float knockoutLevel = 0.2f;  // ピンが倒れたと判定する角度のしきい値

            // ピンが一定以上傾いたら倒れたと判定
            if (x > knockoutLevel || -knockoutLevel > x || z > knockoutLevel || -knockoutLevel > z)
            {
                GameContoller.Pins105AllCount += 1;  // 倒れたピンの総数をカウント
                ones = true;  // 1回だけカウントするためにフラグを立てる
            }
        }
    }

    // 他のオブジェクトと衝突した時の処理
    private void OnCollisionEnter(Collision collision)
    {
        // ピンまたはボールと衝突した場合
        if (collision.gameObject.CompareTag("Pin") || collision.gameObject.CompareTag("Ball"))
        {
            // 衝突音を1回だけ再生
            if (!hasPlayedSound)
            {
                soundManager.PlayHitPinSE();
                hasPlayedSound = true;
            }
        }
    }

    // ピンの音声状態をリセット
    internal void ResetPinSound()
    {
        hasPlayedSound = false;
        ones = false;
    }
}
