using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FinishTrriger : MonoBehaviour
{
	[SerializeField] private GameContoller gameController;
	[SerializeField] private UIController uiController;
	[SerializeField] private SoundManager soundManager;

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Ball"))
		{
			if (gameController.cameraMode != GameContoller.CameraMode.Replay)
            {
				gameController.ChangeCameraMode(GameContoller.CameraMode.LaneEnd);
				gameController.OnLaneTriggerEnter(5f);
			}

			soundManager.FadeOutBallRollSE();

			if (gameController.cameraMode == GameContoller.CameraMode.Replay)
            {
				uiController.ShowMessage("<size=80><color=#FF5126>S</color></size>TRIKE!");
				DOVirtual.DelayedCall(3f, () => gameController.FinishReplay());
            }
		}
	}
}
