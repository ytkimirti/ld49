using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using EZCameraShake;

public class Placable : MonoBehaviour
{
	public bool isPlaced = false;
	public bool isEmptyTrigger = false;

	[Space]

	public Collider2D col;
	public Rigidbody2D rb;
	public MeshRenderer meshRenderer;
	Material defMaterial;

	[Header("Shake Stuff")]
	public bool isShaking;
	public Vector2 verticalShakeForce;
	public float horizontalShakeForce;
	public float shakeTorque;
	public Vector2 shakeTime;
	float shakeTimer;
	[Space]
	public bool isGenerator;
	public float propellerSpeed;
	public Transform propeller;

	[Header("Hangable Stuff")]
	public bool isHangable;
	public Transform emptyTrans;
	public Transform fullTrans;
	public float hangingForce;
	public Placable parentHost;
	public GameObject arrowGO;
	public SpriteRenderer arrowEffectSprite;

	[Space]

	public LayerMask blocksLayer;
	public LayerMask hangableLayer;

	[Header("Person Stuff")]
	public GameObject richElephantLayer;
	public bool isPerson;
	public bool isElephant;
	public MeshRenderer[] skinMeshes;
	public float comHeight;
	Material skinMat;


	private void Awake()
	{
		defMaterial = meshRenderer.material;
	}

	void Start()
	{
		if (isElephant && GameManager.main.currStage == "rich_elephant")
			richElephantLayer.SetActive(true);

		if (isPerson && !isElephant)
		{
			skinMat = GameManager.main.skinMats[Random.Range(0, GameManager.main.skinMats.Length)];
			foreach (MeshRenderer m in skinMeshes)
				m.material = skinMat;
			rb.centerOfMass = Vector3.up * comHeight;
		}
	}

	void Update()
	{
		if (isShaking)
		{
			UpdateShaking();
		}
	}

	public virtual void BecomeWaiter()
	{
		col.enabled = false;
		rb.simulated = false;
	}

	public void Die()
	{
		Destroy(gameObject);
	}

	public virtual void BecomeTrigger()
	{
		isPlaced = false;
		col.enabled = true;
		rb.simulated = true;
		col.isTrigger = true;
		rb.isKinematic = true;
	}

	public virtual void BecomeBlock()
	{
		if (isHangable)
		{
			CameraShaker.Instance.ShakeOnce(3, 2, 0, 1);
			AudioManager.main.Play("window_install");
		}

		isPlaced = true;
		col.enabled = true;
		rb.simulated = true;
		col.isTrigger = false;

		if (!isHangable)
			rb.isKinematic = false;

		if (isHangable)
			GetHanged();
	}

	public void GetHanged()
	{
		Collider2D col2D = Physics2D.OverlapPoint(fullTrans.position, blocksLayer);

		GameObject placableGO = col2D.transform.parent.gameObject;

		Placable p = placableGO.GetComponent<Placable>();

		Debug.Assert(p, "P is null for some reason, check layers");

		parentHost = p;

		transform.parent = placableGO.transform;
		rb.isKinematic = true;

		//Adding force
		if (p)
		{
			p.rb.AddForce(-transform.right * hangingForce, ForceMode2D.Impulse);
			arrowEffectSprite.gameObject.SetActive(true);

			arrowEffectSprite.transform.parent = null;
			arrowEffectSprite.DOFade(0, 1.2f);

			Vector3 targetPos = arrowEffectSprite.transform.position + arrowEffectSprite.transform.right * 3.5f;
			arrowEffectSprite.transform.DOMove(targetPos, 0.7f).SetEase(Ease.OutQuart);
			Destroy(arrowEffectSprite.gameObject, 1.2f);
		}

		arrowGO.SetActive(false);
	}

	public void UpdateShaking()
	{
		if (!isPlaced)
			return;

		if (isGenerator)
		{
			propeller.Rotate(0, Time.deltaTime * propellerSpeed, 0, Space.Self);
		}

		if (isShaking)
		{
			shakeTimer -= Time.deltaTime;

			if (shakeTimer <= 0)
			{
				shakeTimer = Random.Range(shakeTime.x, shakeTime.y);

				float randHorizontalForce = Random.Range(-horizontalShakeForce, horizontalShakeForce);

				Vector2 force = new Vector2(randHorizontalForce, Random.Range(verticalShakeForce.x, verticalShakeForce.y));

				rb.AddForce(force, ForceMode2D.Impulse);

				rb.AddTorque((randHorizontalForce / horizontalShakeForce) * shakeTorque);

				if (isElephant)
					CameraShaker.Instance.ShakeOnce(2, 2, 0, 1);
			}
		}
	}

	void FixedUpdate()
	{
		meshRenderer.material = isEmptyTrigger ? defMaterial : GameManager.main.unplacablePositionMaterial;

		isEmptyTrigger = true;
		if (!isPlaced && isHangable && isEmptyTrigger)
		{
			if (!Physics2D.OverlapPoint(fullTrans.position, blocksLayer))
				isEmptyTrigger = false;
			// else if (Physics2D.OverlapPoint(emptyTrans.position, blocksLayer))
			// 	isEmptyTrigger = false;
			else if (Physics2D.Raycast(emptyTrans.position, transform.right, 20, blocksLayer))
				isEmptyTrigger = false;
		}
	}

	private void OnCollisionEnter2D(Collision2D other)
	{
		if (other.gameObject.tag == "Sea")
		{
			AudioManager.main.Play("splash");
			GameManager.main.OnPlacableDropped();
		}
		if (isGenerator)
		{
			AudioManager.main.Play("generator_impact");
		}
		else
			AudioManager.main.Play("wood_place");
	}

	void OnTriggerStay2D(Collider2D other)
	{
		if (!isPlaced)
			isEmptyTrigger = false;
	}
}
