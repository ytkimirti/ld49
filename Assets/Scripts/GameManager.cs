using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;
using EZCameraShake;

public class GameManager : MonoBehaviour
{
	public bool isGameOver;
	public bool isEndlessMode;
	public bool startEndlessSpawning;
	public int endlessScore;
	public static bool isShadowsClosed;
	public Light directionalLight;

	[Space]

	public string currStage;
	public static int levelIndex;
	public bool controlLock = false;
	public Level currLevel;
	public Level debugLevel;
	public bool isDebug = false;
	public Level[] levels;

	[Header("Raycasting")]
	public LayerMask placableLayer;

	[Header("References")]
	public GameObject island2;
	public TextMeshProUGUI levelEndText;
	public TextMeshProUGUI creditsText;
	public TextMeshProUGUI restarterText;
	public Transform minHeightBar;

	public BossController bossController;

	public Material unplacablePositionMaterial;
	public Material[] skinMats;

	public PlacableManager placableManager;

	public TextboxController textboxController;
	public AudioSource musicSource;

	bool memSpeed;

	public static GameManager main;

	private void Awake()
	{
		main = this;
	}

	public void LoadLevel(Level level)
	{
		currLevel = level;
		if (isDebug)
			currLevel = debugLevel;

		StartCoroutine(GameLoopEnum());
	}

	public float FindMaxHeight()
	{
		float currHeight = 100;

		while (currHeight > 0)
		{
			if (Physics2D.Raycast(new Vector2(-15f, currHeight), Vector2.right, 50, placableLayer))
				return currHeight;
			currHeight -= 0.5f;
		}
		return 0;
	}

	public void StageUpdate(string s)
	{
		if (s == "endless")
		{
			bossController.Die();
		}
		if (s == "endless")
		{
			isEndlessMode = true;
			island2.SetActive(true);
		}
	}

	public void OnCursedEvent()
	{
		bossController.isAngry = true;
		bossController.isTalking = true;
	}

	public void OnPlacableDropped()
	{
		if (currStage == "suzanne")
			return;

		CameraShaker.Instance.ShakeOnce(2, 5, 0, 1);
		//TODO: Shaking etc.
		Invoke("AngryRestart", 0.3f);
	}

	void Start()
	{
		directionalLight.shadows = isShadowsClosed ? LightShadows.None : LightShadows.Soft;
		memSpeed = false;
		musicSource.Play();
		minHeightBar.transform.position = new Vector2(minHeightBar.position.x, 30);
		LoadLevel(levels[levelIndex]);
	}

	public void EnableMinHeight(float height)
	{
		minHeightBar.transform.DOMoveY(height, 1.5f);
	}

	public void DisableMinHeight()
	{
		minHeightBar.transform.DOMoveY(30, 3);
	}

	IEnumerator GameLoopEnum()
	{
		for (int stageIndex = 0; stageIndex < currLevel.stages.Length; stageIndex++)
		{
			// Start of the stage
			LevelStage stage = currLevel.stages[stageIndex];
			currStage = stage.name;

			StageUpdate(stage.name);

			if (isEndlessMode)
			{
				yield return new WaitForSeconds(2f);

				textboxController.Talk("Your boss died from a heart attack. You are free.");
				while (textboxController.isOpen)
					yield return new WaitForEndOfFrame();

				yield return new WaitForSeconds(1f);

				textboxController.Talk("Put as much stuff as you can. Post your highscore!");
				while (textboxController.isOpen)
					yield return new WaitForEndOfFrame();

				yield return new WaitForSeconds(1f);

				startEndlessSpawning = true;
				ScaleAndFadeIn(levelEndText);
				yield break;
			}

			// Boss talking
			if (stage.bossText != "")
			{
				bossController.isOnCamera = true;

				yield return new WaitForSeconds(0.5f);

				if (stage.bossNormalFace == "" || stage.bossTalkFace == "")
					textboxController.Talk(stage.bossText, "-o-", "-_-");
				else
					textboxController.Talk(stage.bossText, stage.bossTalkFace, stage.bossNormalFace);

				while (textboxController.isOpen)
				{
					bossController.isTalking = textboxController.isTalking;
					yield return new WaitForEndOfFrame();
				}

				bossController.isTalking = false;

				yield return new WaitForSeconds(0.5f);

				bossController.isOnCamera = false;
			}

			yield return new WaitForSeconds(0.7f);

			if (stage.enableMinHeight)
				EnableMinHeight(stage.minHeight);

			placableManager.SpawnAllPlacables(stage.placables);

			//Waiting for the player to finish his tower
			while (placableManager.placablesInLine.Count != 0 || placableManager.currPlaccableInHand)
				yield return new WaitForEndOfFrame();

			if (currStage == "suzanne")
			{
				yield return new WaitForSeconds(1f);
				OnCursedEvent();

				yield return new WaitForSeconds(7f);
			}

			yield return new WaitForSeconds(1f);

			CameraShaker.Instance.ShakeOnce(2, 2, 0, 1);

			//Tower is smaller than the min height, loose the game
			if (stage.enableMinHeight && FindMaxHeight() < stage.minHeight)
			{
				bossController.isOnCamera = true;

				yield return new WaitForSeconds(2f);

				textboxController.Talk("This tower is, too short!");

				bossController.isTalking = true;

				while (textboxController.isOpen)
					yield return new WaitForEndOfFrame();

				Restart();

				//Filler
				yield return new WaitForSeconds(2f);
			}

			DisableMinHeight();
		}

		musicSource.Stop();
		AudioManager.main.Play("win");

		yield return new WaitForSeconds(2.9f);


		CameraShaker.Instance.ShakeOnce(3, 2, 0, 1);
		//Won the game

		levelEndText.text = currLevel.levelEndText;

		if (currStage == "suzanne")
		{
			bossController.Die();
			levelEndText.text = "Success???";
		}

		ScaleAndFadeIn(levelEndText);

		levelIndex++;

		yield return new WaitForSeconds(3.5f);

		if (levelIndex < levels.Length)
			Restart();
	}

	void ScaleAndFadeIn(TextMeshProUGUI t)
	{
		t.gameObject.SetActive(true);
		t.transform.localScale = Vector3.one * 0.5f;
		t.transform.DOScale(Vector3.one, 0.7f).SetEase(Ease.OutBack);
		t.color = new Color(t.color.r, t.color.g, t.color.b, 0);
		t.DOFade(1, 0.5f);
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.H))
		{
			isShadowsClosed = !isShadowsClosed;
			directionalLight.shadows = isShadowsClosed ? LightShadows.None : LightShadows.Soft;
		}
		if (isEndlessMode)
		{
			levelEndText.text = endlessScore.ToString();
		}
		// // if (Input.GetKeyDown(KeyCode.B))
		// // 	memSpeed = !memSpeed;
		// // Time.timeScale = memSpeed ? 5f : 1f;
		// Time.fixedDeltaTime = Time.deltaTime * 0.02f;

		controlLock = textboxController.isOpen;

		if (Input.GetKeyDown(KeyCode.X))
		{
			Restart();
		}
	}

	public void AngryRestart()
	{
		musicSource.Stop();
		bossController.isAngry = true;
		bossController.isOnCamera = true;
		bossController.faceText.text = ">o<";
		bossController.isAngry = true;

		Invoke("Restart", 2f);
	}

	public void Restart()
	{
		if (isGameOver)
			return;
		if (!isEndlessMode)
			musicSource.Stop();
		isGameOver = true;

		StartCoroutine(restartEnum());
	}

	IEnumerator restartEnum()
	{
		if (isEndlessMode)
		{
			CameraController.main.zoomOut = true;

			yield return new WaitForSeconds(3f);

			ScaleAndFadeIn(creditsText);

			yield return new WaitForSeconds(3f);

			ScaleAndFadeIn(restarterText);

			while (!Input.GetKey(KeyCode.R))
				yield return new WaitForEndOfFrame();

			//continues here and restarts the level
		}

		Fader.main.FadeIn();

		yield return new WaitForSecondsRealtime(Fader.main.fadeSpeed);

		SceneManager.LoadScene(0);
	}

	public static void LerpFully(Transform myTrans, Transform target, float pt, float rt, float st)
	{
		LerpFully(myTrans, target.position, target.rotation, target.localScale, pt, rt, st);
	}

	public static void LerpFully(Transform t, Vector3 p, Quaternion r, Vector3 s, float pt, float rt, float st)
	{
		t.position = Vector3.Lerp(t.position, p, Time.deltaTime * pt);
		t.localScale = Vector3.Lerp(t.localScale, s, Time.deltaTime * st);
		t.rotation = Quaternion.Lerp(t.rotation, r, Time.deltaTime * rt);
	}
}
