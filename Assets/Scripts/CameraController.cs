using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	public Transform debugTrans;
	public Vector2 mousePos;
	public LayerMask rayWallLayerMask;
	public bool zoomOut;

	[Space]

	public float scrollSpeed;
	public float scrollLerpSpeed;
	Vector2 targetPos;

	[Space]

	public float zoomLerpSpeed;
	public float zoomSpeed;

	[Space]

	public Transform holderTrans;
	public Camera cam;

	public static CameraController main;

	private void Awake()
	{
		main = this;
	}

	void Start()
	{
		targetPos = transform.position;
	}

	void Update()
	{
		if (zoomOut)
		{
			float val = Mathf.Lerp(holderTrans.position.z, -30, Time.deltaTime * 3);
			holderTrans.position = new Vector3(holderTrans.position.x, holderTrans.position.y, val);
		}

		UpdateMovement();
		UpdateRaycasting();
	}

	void UpdateRaycasting()
	{
		RaycastHit hit;
		Ray ray = cam.ScreenPointToRay(Input.mousePosition);

		if (Physics.Raycast(ray, out hit, 9999, rayWallLayerMask))
		{
			mousePos = hit.point;
		}

		debugTrans.position = mousePos;
	}

	void UpdateMovement()
	{
		Vector2 inputDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

		targetPos += inputDir * scrollSpeed * Time.deltaTime;

		transform.position = Vector2.Lerp(transform.position, targetPos, scrollLerpSpeed * Time.deltaTime);
	}
}
