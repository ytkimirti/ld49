using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

public class PlacableManager : MonoBehaviour
{
	public string blocksToSpawn;
	public List<Placable> placablesInLine;

	[Space]

	public Placable currPlaccableInHand;

	[Header("Following")]

	public float followMoveSpeed;
	public float followRotateSpeed;
	public float followScaleSpeed;

	[Header("References")]

	public Transform targetTrans;
	public GameObject[] placablePrefabs;
	public GameObject[] randomPlacables;

	public static PlacableManager main;

	private void Awake()
	{
		main = this;
	}

	void Start()
	{
		// For testing
		// SpawnAllPlacables(blocksToSpawn);
	}

	public void SpawnAllPlacables(string spawnString)
	{
		for (int i = 0; i < spawnString.Length; i++)
		{
			if (spawnString[i] == ' ')
				continue;

			GameObject prefab = findPrefabFromChar(spawnString[i]);

			Debug.Assert(prefab, "There is no prefab attached to this char!");

			GameObject placableGO = Instantiate(prefab, Pipe.main.spawnTrans.position, Quaternion.identity);

			Placable placable = placableGO.GetComponent<Placable>();

			placable.BecomeWaiter();

			placablesInLine.Add(placable);
		}
	}

	public void SpawnRandomPlacable()
	{
		GameObject prefab = randomPlacables[Random.Range(0, randomPlacables.Length)];

		GameObject placableGO = Instantiate(prefab, Pipe.main.spawnTrans.position, Quaternion.identity);

		Placable placable = placableGO.GetComponent<Placable>();

		placable.BecomeWaiter();

		placablesInLine.Add(placable);
	}

	public GameObject findPrefabFromChar(char c)
	{
		for (int i = 0; i < placablePrefabs.Length; i++)
		{
			if (c == placablePrefabs[i].name[0])
				return placablePrefabs[i];
		}
		return null;
	}

	void UpdatePlacableLine()
	{
		if (!currPlaccableInHand && placablesInLine.Count > 0)
		{
			currPlaccableInHand = placablesInLine[0];

			OnHoldStart();

			placablesInLine.RemoveAt(0);

			Pipe.main.UpdateLine(placablesInLine);
		}
	}

	void Update()
	{
		if (GameManager.main.startEndlessSpawning && GameManager.main.isEndlessMode && placablesInLine.Count < 2)
		{
			SpawnRandomPlacable();
		}

		UpdatePlacableLine();

		if (!GameManager.main.controlLock && Input.GetKeyDown(KeyCode.Space))
			RotateTargetTrans();
		if (currPlaccableInHand)
		{
			targetTrans.position = CameraController.main.mousePos;
			//Update the following of the cursor
			GameManager.LerpFully(currPlaccableInHand.transform, targetTrans.position, targetTrans.rotation, targetTrans.localScale,
			 		followMoveSpeed, followRotateSpeed, followScaleSpeed);

			if (!GameManager.main.controlLock && Input.GetKeyDown(KeyCode.Mouse0) && currPlaccableInHand.isEmptyTrigger)
			{
				Place();
			}
		}
	}

	public void OnHoldStart()
	{
		currPlaccableInHand.transform.parent = transform;

		currPlaccableInHand.BecomeTrigger();
	}

	public void RotateTargetTrans()
	{
		targetTrans.Rotate(0, 0, 90);
	}

	public void Place()
	{
		if (GameManager.main.isEndlessMode)
			GameManager.main.endlessScore++;

		CameraShaker.Instance.ShakeOnce(0.4f, 5, 0, 0.4f);

		AudioManager.main.Play("piece_place");

		StartCoroutine(PlaceEnum());
	}

	//Move it to the target position, wait one physics update
	//If it cant place there, dont place it there.
	IEnumerator PlaceEnum()
	{
		// currPlaccableInHand.transform.position = targetTrans.position;
		// currPlaccableInHand.transform.rotation = targetTrans.rotation;
		currPlaccableInHand.transform.localScale = targetTrans.localScale;

		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();

		if (currPlaccableInHand && currPlaccableInHand.isEmptyTrigger)
		{
			currPlaccableInHand.BecomeBlock();

			currPlaccableInHand = null;

			targetTrans.rotation = Quaternion.identity;
		}
	}

}