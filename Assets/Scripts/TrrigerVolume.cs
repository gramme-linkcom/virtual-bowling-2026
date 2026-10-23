using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrrigerVolume : MonoBehaviour
{
	public GameContoller controller;

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Ball"))
		{
			if (controller.cameraMode != GameContoller.CameraMode.Replay)
				controller.ChangeCameraMode(GameContoller.CameraMode.Follow);
		}
	}

}
