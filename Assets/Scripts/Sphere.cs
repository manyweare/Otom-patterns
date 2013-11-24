using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Holoville.HOTween;

public class Sphere : MonoBehaviour
{
	public GameObject CirclePrefab, HighlightPrefab, LockedPrefab;
	[HideInInspector]
	public Color32 CurrentColor = Color.black;
	[HideInInspector]
	public Color WRONG_COLOR = Color.red;
	[HideInInspector]
	public GameObject HighlightGameObject, LockedGameObject;
	private GameObject[] powerUpAnimPool = new GameObject[36];
	private GameObject myMesh, myShadow;
	private List<GameObject> coloredObjects = new List<GameObject>();
	private Transform _myTransform, _lockedTransform;
	private bool sameColor = true;
	private Tweener loopTween;
	private Sequence pulseSequence;
	private int numTurnsUnlockable = 0;
	[HideInInspector]
	public bool isLocked = false;

	// This property controls weather or not a circle is lockable.
	private bool _isLockable = true;
	public bool isLockable
	{
		get
		{
			if ((Color)CurrentColor == (Color)ArtManager.Instance.WHITEDOT_COLOR || isLocked)
			{
				_isLockable = false;
				return _isLockable;
			}
			else if (numTurnsUnlockable <= 0)
				_isLockable = true;
			else if (numTurnsUnlockable > 0)
				_isLockable = false;
			return _isLockable;
		}
		set
		{
			if ((Color)CurrentColor == (Color)ArtManager.Instance.WHITEDOT_COLOR)
			{
				_isLockable = false;
				return;
			}
			else if (value)
			{
				numTurnsUnlockable = 0;
				_isLockable = true;
			}
			else if (!value)
			{
				numTurnsUnlockable = 2;
				_isLockable = false;
			}
		}
	}

	void NextTurn()
	{
		if (numTurnsUnlockable > 0)
			--numTurnsUnlockable;
	}

	void Awake()
	{
		GameEventManager.NextTurn += NextTurn;

		_myTransform = transform;

		// Sequence with pulse that warns player that game over is imminent.
		pulseSequence = new Sequence(new SequenceParms().Loops(-1, LoopType.Restart));
		var firstPulseTween = HOTween.To(_myTransform, 0.4f, new TweenParms().Prop("localScale", _myTransform.localScale * 1.15f).Ease(EaseType.EaseInExpo));
		var backPulseTween = HOTween.To(_myTransform, 0.2f, new TweenParms().Prop("localScale", ArtManager.Instance.ORIGINAL_SCALE).Ease(EaseType.EaseOutExpo));
		var secondPulseTween = HOTween.To(_myTransform, 0.2f, new TweenParms().Prop("localScale", _myTransform.localScale * 1.1f).Ease(EaseType.EaseInExpo));
		pulseSequence.Append(firstPulseTween);
		pulseSequence.Append(backPulseTween);
		pulseSequence.Append(secondPulseTween);
		pulseSequence.Append(backPulseTween);
		pulseSequence.AppendInterval(1f);
		pulseSequence.Pause();
	}

	void Start()
	{
		InstantiateMeshes();

		// Change main Dot prefab collider radius based on screen ratio.
//		_myTransform.parent.transform.GetComponent<BoxCollider2D>().size *= ArtManager.Instance.SCREEN_RATIO;
	}

	#region CONSTRUCTOR METHODS

	// TODO: FIX COLOR ASSIGNMENT LOGIC.

	public void InstantiateMeshes()
	{
		myMesh = Instantiate(CirclePrefab, _myTransform.parent.position, Quaternion.identity) as GameObject;
		myMesh.transform.localScale = ArtManager.Instance.ORIGINAL_SCALE;
		myMesh.renderer.material = ArtManager.Instance.PatternMaterial;
		myMesh.name = "MyMesh";
		myMesh.transform.parent = _myTransform;
		coloredObjects.Add(myMesh);

		InstantiateHighlightMesh(CirclePrefab);
		InstantiateLockedMesh(LockedPrefab);
		InstantiateShadowMesh(CirclePrefab);

		AssignNewRandomColor(ArtManager.Instance.ColorList, coloredObjects);

		InstantiatePowerUpAnimPool(CirclePrefab);
	}
	
	public void InstantiateShadowMesh(GameObject prefab)
	{
	    myShadow = Instantiate(prefab, _myTransform.parent.position, Quaternion.identity) as GameObject;
	    myShadow.name = "Shadow";
		myShadow.transform.localScale = ArtManager.Instance.ORIGINAL_SCALE;
		myShadow.transform.localPosition += new Vector3(0.005f, -0.005f, 0.2f);
		myShadow.renderer.material = ArtManager.Instance.SimpleMaterial;
		myShadow.renderer.material.color = Color.black - new Color(0f, 0f, 0f, 0.6f);
	    myShadow.transform.parent = _myTransform;
	}
	
	public void InstantiateHighlightMesh(GameObject prefab)
	{
		// This is the prefab that activates when player taps on dot.
		HighlightGameObject = Instantiate(prefab, _myTransform.position, Quaternion.identity) as GameObject;
		HighlightGameObject.name = "Highlight";
		HighlightGameObject.transform.localPosition += new Vector3(0f, 0f, 0.1f);
		HighlightGameObject.renderer.material = ArtManager.Instance.SimpleMaterial;
		HighlightGameObject.transform.parent = _myTransform;
		coloredObjects.Add(HighlightGameObject);
		HighlightGameObject.SetActive(false);
	}
	
	public void InstantiateLockedMesh(GameObject prefab)
	{
	    LockedGameObject = Instantiate(prefab, _myTransform.position, Quaternion.identity) as GameObject;
		LockedGameObject.name = "Locked";
		LockedGameObject.renderer.material.SetTexture("_Detail", myMesh.renderer.material.GetTexture("_Detail"));
		LockedGameObject.renderer.material.SetFloat("_DetailTiling", myMesh.renderer.material.GetFloat("_DetailTiling"));
		_lockedTransform = LockedGameObject.transform;
	    _lockedTransform.localScale = Vector3.zero;
	    _lockedTransform.parent = _myTransform;

		loopTween = HOTween.To(_lockedTransform, 3f, ArtManager.Instance.lockLoopParms);
		loopTween.Pause();

		coloredObjects.Add(LockedGameObject);
		LockedGameObject.SetActive(false);
	}
	
	void InstantiatePowerUpAnimPool(GameObject prefab)
	{
		for (int i = 0; i < powerUpAnimPool.Length; i++)
		{
			powerUpAnimPool[i] = Instantiate(prefab, _myTransform.position + new Vector3(0f, 0f, -2f), Quaternion.identity) as GameObject;
			powerUpAnimPool[i].name = "PowerUp" + i.ToString();
			powerUpAnimPool[i].renderer.material.SetTexture("_Detail", myMesh.renderer.material.GetTexture("_Detail"));
			powerUpAnimPool[i].renderer.material.SetFloat("_DetailTiling", myMesh.renderer.material.GetFloat("_DetailTiling"));
			powerUpAnimPool[i].transform.localScale = myMesh.transform.localScale * 0.75f;
			powerUpAnimPool[i].transform.parent = _myTransform;
			powerUpAnimPool[i].SetActive(false);
		}
	}
	
	#endregion

	public void AssignNewRandomColor(List<Color32> colorList, List<GameObject> objs)
	{
		// Random index for material and color lists.
		int k = (int)UnityEngine.Random.Range(0, colorList.Count);
		CurrentColor = colorList[k];

		for (int i = 0; i < objs.Count; i++)
		{
			// Assign new material.
			if (ArtManager.Instance.Patterns[k] != null)
			{
				objs[i].renderer.material.SetTexture("_Detail", ArtManager.Instance.Patterns[k]);
				objs[i].renderer.material.SetFloat("_DetailTiling", 15f * ArtManager.Instance.SCREEN_RATIO);
			}
			// Assign new color.
			objs[i].renderer.material.color = CurrentColor;
		}
	}

	public void AnimateBirth()
	{
		isLockable = true;
		pulseSequence.Pause();

		myMesh.SetActive(true);
		myShadow.SetActive(true);
		HighlightGameObject.SetActive(false);
		LockedGameObject.SetActive(false);

		AssignNewRandomColor(ArtManager.Instance.ColorList, coloredObjects);

		_myTransform.localScale = Vector3.one;
		
		// Move this sphere to the top of the board and drop it.
		_myTransform.parent.position += new Vector3(0f, 1.5f, 0f);
		_myTransform.parent.collider2D.enabled = true;
		_myTransform.parent.rigidbody2D.gravityScale = 1f;
		_myTransform.collider.enabled = true;
	}
	
	public void AnimateDeath()
	{
		StartCoroutine("CoAnimateDeath");
	}

	public IEnumerator CoAnimateDeath()
	{
		DeactivateHighlight();
		HOTween.To(_myTransform, 0.1f, ArtManager.Instance.deathParms);
		yield return new WaitForSeconds(0.1f);

		_myTransform.parent.rigidbody2D.gravityScale = 0f;
		_myTransform.parent.collider2D.enabled = false;
		_myTransform.collider.enabled = false;
	}

	public void ActivateHighlight()
	{
		if (HighlightGameObject.activeSelf)
			return;

		HighlightGameObject.SetActive(true);
		HighlightGameObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
		HOTween.To(HighlightGameObject.transform, 0.3f, ArtManager.Instance.highlightParms);
		if (sameColor)
			HighlightGameObject.renderer.material.color = ArtManager.Instance.CHAIN_COLOR;
		else
			HighlightGameObject.renderer.material.color = WRONG_COLOR;
	}
    
	public void DeactivateHighlight()
	{
		StartCoroutine("CoDeactivateHighlight");
	}

	IEnumerator CoDeactivateHighlight()
	{
		if (!HighlightGameObject.activeSelf)
			yield break;

		HOTween.To(HighlightGameObject.transform, 0.2f, "localScale", Vector3.zero);
		yield return new WaitForSeconds(0.2f);
		HighlightGameObject.SetActive(false);
	}

	public void WrongSphere()
	{
		HighlightGameObject.renderer.material.color = WRONG_COLOR;
	}
 
	public void SquareSphere()
	{
		HOTween.To(HighlightGameObject.transform, 0.2f, "localScale", new Vector3(1.7f, 1.7f, 1.7f));
	}

	public void LockSelf()
	{
		StartCoroutine("CoLockSelf");
	}
 
	IEnumerator CoLockSelf()
	{
		if (LockedGameObject.activeSelf)
			yield break;

		myMesh.SetActive(false);
		myShadow.SetActive(false);
		LockedGameObject.SetActive(true);
		_lockedTransform.localScale = Vector3.zero;
		HOTween.To(_lockedTransform, 0.4f, ArtManager.Instance.lockParms);
		yield return new WaitForSeconds(0.4f);
		loopTween.Restart();
	}
 
	public void UnlockSelf()
	{
		StartCoroutine("CoUnlockSelf");
	}
 
	IEnumerator CoUnlockSelf()
	{
		if (!LockedGameObject.activeSelf)
			yield break;

		loopTween.Pause();
		LockedGameObject.SetActive(true);
		HOTween.To(_lockedTransform, 0.4f, ArtManager.Instance.unlockParms);
		yield return new WaitForSeconds(0.4f);
		LockedGameObject.SetActive(false);
		myMesh.SetActive(true);
		myShadow.SetActive(true);
	}
	
	public void HighlightLock()
	{
		StartCoroutine("CoHighlightLock");
	}
	
	IEnumerator CoHighlightLock()
	{
		loopTween.Pause();
		HOTween.To(_lockedTransform, 0.2f, ArtManager.Instance.highlightLockParms);
		yield return null;
	}

	public void ResetLock()
	{
		StartCoroutine("CoResetLock");
	}
	
	IEnumerator CoResetLock()
	{
		HOTween.To(_lockedTransform, 0.2f, ArtManager.Instance.resetLockParms);
		yield return new WaitForSeconds(0.2f);
		loopTween.Play();
	}

	public void HideSelf()
	{
		DeactivateHighlight();
		pulseSequence.Pause();
		HOTween.To(_myTransform, 0.2f, ArtManager.Instance.hideParms);
	}

	public void ShowSelf()
	{
		AssignNewRandomColor(ArtManager.Instance.ColorList, coloredObjects);
		HOTween.To(_myTransform, 0.2f, ArtManager.Instance.showParms);
	}

	// Method to animate the death of the chained dot and send a copy to the corresponding power up color.
	// This helps inform the player that the dots she chained are added to power up score.
	public IEnumerator AnimateToPowerUp()
	{
		if (myMesh.renderer.material.color == ArtManager.Instance.GREYDOT_COLOR)
		{
			AnimateDeath();
			yield break;
		}
		
		// Find position of power up button of the same color as chain to know where to send dot.
		Vector3 powerUpButtonPos = Vector3.zero;
		int colorIndex = ArtManager.Instance.OriginalColors.IndexOf(CurrentColor);
		if (colorIndex >= 0)
			powerUpButtonPos = PowerUpsManager.Instance.TextList[colorIndex].transform.position;
		
		Vector3[] positions = new Vector3[5];
		int numAnims = 1;
		
		// If dot is white, we're gonna animate 5 dots going towards all power up colors.
		if (myMesh.renderer.material.color == ArtManager.Instance.WHITEDOT_COLOR)
		{
			numAnims = 5;
			for (int j = 0; j < numAnims; j++)
			{
				// Make an array with the positions of all power up buttons.
				var targetPos = Vector3.zero;
				targetPos = PowerUpsManager.Instance.TextList[j].transform.position;
				positions[j] = Camera.main.ViewportToWorldPoint(targetPos);
			}
		}
		else
			positions[0] = Camera.main.ViewportToWorldPoint(powerUpButtonPos);
		
		// Animate power up circles.
		for (int i = 0; i < numAnims; i++)
		{
			// Activate a circle from the pool. This guy will animate towards the power up button.
			powerUpAnimPool[i].SetActive(true);
			powerUpAnimPool[i].transform.position = _myTransform.position;
			powerUpAnimPool[i].transform.localScale = myMesh.transform.localScale * 0.75f;
			powerUpAnimPool[i].renderer.material.color = CurrentColor;
//			Destroy(powerUpAnimPool[i], 0.5f);
			
			HOTween.To(powerUpAnimPool[i].transform, 0.5f, 
			           new TweenParms().Prop("position", positions[i]).Ease(EaseType.EaseInOutExpo));
			HOTween.To(powerUpAnimPool[i].transform, 0.5f,
			           new TweenParms().Prop("localScale", Vector3.zero).Ease(EaseType.EaseInExpo));
//			HOTween.To(powerUpAnimPool[i].renderer.material, 0.4f,
//			           new TweenParms().Prop("color", powerUpAnimPool[i].renderer.material.color - new Color(0f, 0f, 0f, 1f)).Ease(EaseType.EaseInExpo));
			
			yield return new WaitForSeconds(0.1f);
		}
		
//		// Replenish pool.
//		for (int i = 0; i < numAnims; i++)
//		{
//			GameObject temp = Instantiate(CirclePrefab, _myTransform.position, Quaternion.identity) as GameObject;
//			temp.renderer.material.color = CurrentColor;
//			temp.transform.parent = _myTransform;
//			powerUpAnimPool[i] = temp;
//			powerUpAnimPool[i].SetActive(false);
//		}
		
		// Hide current circle.
		AnimateDeath();
	}
	
	public void Pulsate()
	{
		pulseSequence.Restart();
	}
	
	public void StopPulse()
	{
		pulseSequence.Rewind();
		pulseSequence.Pause();
	}
}