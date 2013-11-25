using UnityEngine;
using System;
using System.Collections;
using Holoville.HOTween;

public class MainMenu : MonoBehaviour
{
	public GameObject SoundButton;
	public GUIText VersionText;
	public string VersionNumber;
	private Transform _myTransform;
	private Vector3 hiddenPos;
	private bool isHidden = true;
	private bool soundIsOn = true;
	private AudioListener _listener;

	// SINGLETON
	public static MainMenu Instance {
		get {
			if (instance == null)
				instance = new MainMenu();
			return instance;
		}
	}

	private static MainMenu instance = null;

	void Awake()
	{
		if (instance)
			DestroyImmediate(gameObject);
		else
			instance = this;

		_myTransform = transform;
	}

	void Start()
	{
		_myTransform.position -= new Vector3(1f, 0f, 0f);
		hiddenPos = _myTransform.position;

		// Makes Sound button display the right text.
		var soundStatus = AudioListener.volume > 0 ? "On" : "Off";
		SoundButton.guiText.text = "Sound " + soundStatus;

		VersionText.text = "VERSION " + VersionNumber;

		StartCoroutine("ShowSelf");
	}

	void Update()
	{
		if (!isHidden)
			HandleInput();
	}

	void HandleInput()
	{
		if (Input.touchCount == 1)
		{
			if (Input.GetTouch(0).phase == TouchPhase.Began)
			{
				var ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
				RaycastHit hit;
				LayerMask menuLayer = 1 << LayerMask.NameToLayer("MainMenu");
				if (Physics.Raycast(ray, out hit, 10f, menuLayer))
					MainMenuActions(hit.collider.transform.parent.gameObject);
			}
		}

		if (Application.platform == RuntimePlatform.OSXEditor)
		{
			if (Input.GetMouseButtonDown(0))
			{
				var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				LayerMask menuLayer = 1 << LayerMask.NameToLayer("MainMenu");
				if (Physics.Raycast(ray, out hit, 10f, menuLayer))
					MainMenuActions(hit.collider.transform.parent.gameObject);
			}
		}
	}

	void MainMenuActions(GameObject button)
	{
		switch (button.name)
		{
		case "New Game":
			StartCoroutine("LoadNewGame");
			break;
		case "Sound":
			ToggleSound();
			break;
		case "About":
			StartCoroutine("LoadAbout");
			break;
		}
	}

	IEnumerator LoadNewGame()
	{
		HideSelf();
		yield return new WaitForSeconds(0.3f);
		Application.LoadLevel("Gameboard");
	}

	IEnumerator LoadAbout()
	{
		HideSelf();
		yield return new WaitForSeconds(0.3f);
		Application.LoadLevel("About");
	}

	void ToggleSound()
	{
		soundIsOn = !soundIsOn;
		AudioListener.volume = soundIsOn == true ? 100 : 0;
		var soundStatus = soundIsOn == true ? "On" : "Off";
		SoundButton.guiText.text = "Sound " + soundStatus;
	}

	// Animation Methods.

	public IEnumerator ShowSelf()
	{
		HOTween.To(_myTransform, 0.3f, new TweenParms().Prop("position", Vector3.zero).Ease(EaseType.EaseOutExpo));
		yield return new WaitForSeconds(0.3f);
		isHidden = false;
	}

	void HideSelf()
	{
		isHidden = true;
		HOTween.To(_myTransform, 0.3f, new TweenParms().Prop("position", hiddenPos).Ease(EaseType.EaseInExpo));
	}
}
