using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class Pipe : MonoBehaviour
{
	public Transform targetTrans;
	public Transform spawnTrans;

	public float spawnScale;
	public float rotateSpeed;
	public float leanAngle;
	public Placable currPlacableWaiting;
	public TextMeshPro leftPiecesText;

	public static Pipe main;

	private void Awake()
	{
		main = this;
	}

	void Start()
	{

	}

	void Update()
	{
		targetTrans.Rotate(0, rotateSpeed * Time.deltaTime, 0);
	}

	public void UpdateLine(List<Placable> line)
	{
		for (int i = 0; i < line.Count; i++)
		{
			line[i].transform.localScale = Vector3.one * spawnScale;

			line[i].transform.parent = targetTrans;

			line[i].transform.position = spawnTrans.position;

			line[i].transform.eulerAngles = Vector3.right * leanAngle;

			if (i == 0)
			{
				line[i].transform.DOMove(targetTrans.position, 0.5f).SetEase(Ease.OutBack);
			}
		}
		if (GameManager.main.isEndlessMode)
			leftPiecesText.text = "owo";
		else
			leftPiecesText.text = line.Count.ToString() + " left";
	}
}
