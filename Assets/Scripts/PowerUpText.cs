using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class PowerUpText : MonoBehaviour
{
	public Color MyColor;
	private Transform _myTransform;
	private Vector3 _originalPosition, _hidePosition;
	private GameObject myShadow;

	void Awake()
	{
		_myTransform = transform;
		_originalPosition = _myTransform.position;
		_hidePosition = _originalPosition - new Vector3(0f, 1f, 0f);
		guiText.color = MyColor;

		InstantiateShadow();
	}

	void InstantiateShadow()
	{
		myShadow = new GameObject("Text Shadow");
		myShadow.AddComponent<GUIText>();
		myShadow.guiText.text = guiText.text;
		myShadow.guiText.font = guiText.font;
		myShadow.guiText.anchor = guiText.anchor;
		myShadow.guiText.alignment = guiText.alignment;
		myShadow.guiText.fontSize = guiText.fontSize;
		myShadow.guiText.color = Color.black;
		myShadow.transform.position = _myTransform.position + new Vector3(0.015f, -0.003f, -0.01f);
		myShadow.transform.parent = _myTransform;
	}

	IEnumerator HideSelf()
	{
		HOTween.To(_myTransform, 0.2f, new TweenParms().Prop("position", _hidePosition).Ease(EaseType.EaseInExpo));
		HOTween.To(guiText, 0.2f, "color", Color.black);
		yield return new WaitForSeconds(0.2f);
	}

	IEnumerator ShowSelf()
	{
		HOTween.To(_myTransform, 0.2f, new TweenParms().Prop("position", _originalPosition).Ease(EaseType.EaseOutExpo));
		HOTween.To(guiText, 0.2f, "color", MyColor);
		yield return new WaitForSeconds(0.2f);
	}

	public void UpdateCount(int i)
	{
		StartCoroutine(CoUpdateCount(i));
	}

	IEnumerator CoUpdateCount(int count)
	{
		yield return StartCoroutine("HideSelf");
		guiText.text = count.ToString();
		myShadow.guiText.text = guiText.text;
		yield return StartCoroutine("ShowSelf");
	}
}
