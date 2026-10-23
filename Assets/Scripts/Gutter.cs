using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gutter : MonoBehaviour
{
    [SerializeField] private SoundManager soundManager;

    private void OnCollisionEnter(Collision collision)
    {
        soundManager.FadeOutBallRollSE();
    }
}
