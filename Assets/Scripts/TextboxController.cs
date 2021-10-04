using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class TextboxController : MonoBehaviour
{
	public GameObject pressSpace;
	public TextMeshProUGUI textboxText;
	public CanvasGroup canvasGroup;
	public string text;
	public bool isTalking = false;
	public bool isOpen = false;

	[Space]

	public float charSpeed;
	public float commaSpeed;

	bool skipDialogue;
	string bossFaceNormal;

	void Start()
	{
		bossFaceNormal = "";
		Disable();
		// Talk("Omg this is, brow, fox that great, great, great thing.");
	}

	void Update()
	{
		bool skipKey = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Mouse0);

		if (isTalking && skipKey)
			skipDialogue = true;
		else if (!isTalking && isOpen && skipKey)
		{
			ClosePopup();
		}
	}

	void Disable()
	{
		transform.DOKill();
		transform.localScale = Vector3.zero;
		canvasGroup.alpha = 0;
	}

	public void Talk(string _text)
	{
		pressSpace.SetActive(false);
		StopCoroutine(talkEnum());
		Disable();
		transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
		canvasGroup.DOFade(1, 0.5f);
		text = _text;
		StartCoroutine(talkEnum());
	}


	public void Talk(string _text, string talkFace, string normalFace)
	{
		GameManager.main.bossController.faceText.text = talkFace;
		bossFaceNormal = normalFace;
		Talk(_text);
	}

	public void ClosePopup()
	{
		StopCoroutine(talkEnum());
		transform.DOScale(Vector3.zero, 0.5f);
		canvasGroup.DOFade(0, 0.5f);
		isOpen = false;
	}

	IEnumerator talkEnum()
	{
		textboxText.text = "";
		isOpen = true;
		isTalking = true;
		skipDialogue = false;

		yield return new WaitForSeconds(0.5f);

		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];

			textboxText.text = textboxText.text + c;

			if (!skipDialogue)
			{
				//TODO: Add talking sound here
				yield return new WaitForSecondsRealtime(c == ',' ? commaSpeed : charSpeed);
			}
		}
		if (bossFaceNormal != "")
			GameManager.main.bossController.faceText.text = bossFaceNormal;
		bossFaceNormal = "";
		isTalking = false;
		skipDialogue = false;


		yield return new WaitForSeconds(4f);

		pressSpace.SetActive(true);
	}
}
