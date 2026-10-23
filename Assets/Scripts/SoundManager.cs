using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    private AudioSource audioSourceWithBall;

    [SerializeField] private AudioClip hitPinSE;
    [SerializeField] private AudioClip throwBallSE;
    [SerializeField] private AudioClip cheersSE;
    [SerializeField] private AudioClip throw_1_SE;
    [SerializeField] private AudioClip throw_2_SE;
    [SerializeField] private AudioClip kansei_SE;
    [SerializeField] private AudioClip yeah_SE;
    [SerializeField] private AudioClip hakusyu_SE;
    [SerializeField] private AudioClip morning_BGM;


    private bool isPlayingBallRollSE;

    internal void PlayHitPinSE()
    {
        audioSource.PlayOneShot(hitPinSE);
    }

    internal void PlayThrowBallSE()
    {
        audioSource.PlayOneShot(throwBallSE);
    }

    internal void PlayCheersSE()
    {
        audioSource.PlayOneShot(cheersSE);
    }

    internal void PlayThrow_1_SE()
    {
        audioSource.PlayOneShot(throw_1_SE);
    }

    internal void PlayThrow_2_SE()
    {
        audioSource.PlayOneShot(throw_2_SE);
    }
    
    internal void Hakusyu_SE()
    {
        audioSource.PlayOneShot(hakusyu_SE);
    }

    internal void Kansei_SE()
    {
        audioSource.PlayOneShot(kansei_SE);
    }

    internal void Morning_BGM()
    {
        audioSource.PlayOneShot(morning_BGM);
    }

    internal void Yeah_SE()
    {
        audioSource.PlayOneShot(yeah_SE);
    }

    internal void PlayBallRollSE(AudioSource audioSource)
    {
        isPlayingBallRollSE = true;

        audioSourceWithBall = audioSource;
        audioSourceWithBall.Play();
    }

    internal void FadeOutBallRollSE()
    {
        if (!isPlayingBallRollSE) return;

        _FadeOutBallRollSECoroutine = StartCoroutine(FadeOutBallRollSECoroutine());
        isPlayingBallRollSE = false;
    }

    internal void StopFadeOutBallRollSECoroutine()
    {
        if (_FadeOutBallRollSECoroutine != null)
        {
            StopCoroutine(_FadeOutBallRollSECoroutine);
            _FadeOutBallRollSECoroutine = null;
        }
    }

    private Coroutine _FadeOutBallRollSECoroutine;
    private IEnumerator FadeOutBallRollSECoroutine()
    {
        var wait = new WaitForSeconds(0.05f);
        var volume = 1f;
        while (volume > 0f)
        {
            volume -= 0.1f;
            audioSourceWithBall.volume = volume;

            yield return wait;
        }
        audioSourceWithBall.volume = 1f;
        audioSourceWithBall.Stop();

        _FadeOutBallRollSECoroutine = null;
    }
}
