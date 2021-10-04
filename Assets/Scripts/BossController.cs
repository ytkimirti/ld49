using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class BossController : MonoBehaviour
{
	public bool isDed;
	public bool isOnCamera;
	public bool isTalking;
	public bool isAngry;

	[Space]

	public float moveSpeed;
	public float rotateSpeed;

	[Space]

	public float idleSinSpeed;
	public float idleSinMagnitude;

	public float talkSinSpeed;
	public float talkSinMagnitude;

	public float angrySinSpeed;
	public float angrySinMagnitude;

	[Space]

	public Transform mainHolder;
	public Transform rotationHolder;
	public Transform headHolder;

	[Space]
	public Transform cameraTarget;
	public Transform normalTarget;
	public Transform dieTarget;

	[Space]
	public TextMeshPro faceText;
	bool memIsOnCamera;
	bool memIsTalking;

	public AudioSource talkSound;

	float targetHeadRot;

	void Start()
	{
		memIsOnCamera = isOnCamera;
	}

	public void Die()
	{
		talkSound.Stop();
		isDed = true;
	}

	void Update()
	{
		if (isDed)
		{
			faceText.text = "XoX";
			GameManager.LerpFully(transform, dieTarget, 3, 3, 3);
			return;
		}

		talkSound.pitch = !isAngry ? 1.65f : 2.14f;

		//Talking sound
		if (memIsTalking != (isTalking || isAngry))
		{
			if (isTalking || isAngry)
				talkSound.Play();
			else
				talkSound.Stop();
		}
		memIsTalking = (isTalking || isAngry);

		//On camera SFX
		if (memIsOnCamera != isOnCamera)
		{
			if (isOnCamera)
				AudioManager.main.Play("wind_come");
			else
				AudioManager.main.Play("wind_go");
		}
		memIsOnCamera = isOnCamera;

		rotationHolder.localEulerAngles = new Vector3(Mathf.Sin(Time.time * idleSinSpeed) * idleSinMagnitude, 0, 0);

		if (isTalking)
			targetHeadRot = Mathf.Sin(Time.time * talkSinSpeed) * talkSinMagnitude;

		float headTalkRot = Mathf.LerpAngle(headHolder.transform.localEulerAngles.x, targetHeadRot, 8 * Time.deltaTime);

		float headRotY = 0;

		if (isAngry)
			headRotY = Mathf.Sin(Time.time * angrySinSpeed) * angrySinMagnitude;

		headHolder.transform.localEulerAngles = new Vector3(headTalkRot, headRotY / 5f, headRotY);

		GameManager.LerpFully(transform, isOnCamera ? cameraTarget : normalTarget, 5, 5, 5);
	}
}
