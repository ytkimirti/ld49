using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeaController : MonoBehaviour
{
	void Start()
	{

	}

	void Update()
	{

	}

	private void OnCollisionEnter2D(Collision2D other)
	{
		GameObject go = other.gameObject;

		print("Touched");
		if (!go)
			return;
		Placable p = go.GetComponent<Placable>();

		//TODO: Splash effect
		if (!p)
			return;

		GameManager.main.OnPlacableDropped();
		p.Die();
	}
}
