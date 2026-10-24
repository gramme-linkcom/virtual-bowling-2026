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

    // BGM は効果音と別の AudioSource で鳴らし、ループ再生する (2026 追加)。
    // 以前は効果音と同じ AudioSource の PlayOneShot だったので、曲が終わると無音になっていた。
    [SerializeField] private bool loopBGM = true;
    [SerializeField, Range(0f, 1f)] private float bgmVolume = 1f;
    private AudioSource bgmSource;

    internal void Morning_BGM()
    {
        PlayBGM(morning_BGM);
    }

    internal void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            bgmSource.spatialBlend = 0f;
            if (audioSource != null) bgmSource.outputAudioMixerGroup = audioSource.outputAudioMixerGroup;
        }
        bgmSource.loop = loopBGM;
        bgmSource.volume = bgmVolume * (audioSource != null ? audioSource.volume : 1f);
        // 同じ曲が流れているときは最初からにしない
        if (bgmSource.isPlaying && bgmSource.clip == clip) return;
        bgmSource.clip = clip;
        bgmSource.Play();
    }

    internal void StopBGM()
    {
        if (bgmSource != null) bgmSource.Stop();
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
