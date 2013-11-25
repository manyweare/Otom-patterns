using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Holoville.HOTween;

public class ArtManager : MonoBehaviour
{
	public GameObject CircleMesh;
	[HideInInspector]
	public List<GameObject> DataMeshList = new List<GameObject>();
	[HideInInspector]
	public Vector3 ORIGINAL_SCALE = Vector3.one;
	[HideInInspector]
	public List<Color32> ColorList = new List<Color32>();
	[HideInInspector]
	public List<Color32> OriginalColors = new List<Color32>();
	[HideInInspector]
	public List<Color32> ExtraColors = new List<Color32>();
	public Color32 Color01 = new Color32(255, 0, 125, 255); // Pink
	public Color32 Color02 = new Color32(125, 0, 125, 255); // Purple
	public Color32 Color03 = new Color32(0, 255, 150, 255); // Teal
	public Color32 Color04 = new Color32(0, 125, 255, 255); // Blue
	public Color32 Color05 = new Color32(255, 230, 0, 255); // Yellow
	public Color32 GREYDOT_COLOR = new Color32(90, 90, 90, 255); // Grey
	public Color32 WHITEDOT_COLOR = new Color32(255, 255, 255, 255); // White
	public Color32 CHAIN_COLOR = new Color32(255, 255, 255, 255);
	public Material SimpleMaterial, PatternMaterial;
	public float DETAIL_TILING = 10f;
	public Texture[] Patterns = new Texture[7];
	public TweenParms highlightParms = new TweenParms();
	public TweenParms deathParms = new TweenParms();
	public TweenParms lockParms = new TweenParms();
	public TweenParms unlockParms = new TweenParms();
	public TweenParms highlightLockParms = new TweenParms();
	public TweenParms resetLockParms = new TweenParms();
	public TweenParms lockLoopParms = new TweenParms();
	public TweenParms hideParms = new TweenParms();
	public TweenParms showParms = new TweenParms();
	public TweenParms explosionParms = new TweenParms();
	public Color LOCKED_COLOR = Color.white;
	[HideInInspector]
	public float SCREEN_RATIO = 1f;
	private Vector3 LOCKED_SCALE = Vector3.one * 0.7f;

	public static ArtManager Instance
	{
		get
		{
			if (instance == null)
				instance = new ArtManager();
			return instance;
		}
	}
	
	private static ArtManager instance = null;
	
	void Awake()
	{
		if (instance)
			DestroyImmediate(gameObject);
		else
			instance = this;

		SCREEN_RATIO = MainCamera.Instance.screenRatio;
		ORIGINAL_SCALE = Vector3.one * SCREEN_RATIO;
		DETAIL_TILING *= SCREEN_RATIO;
		
		DataMeshList.Add(CircleMesh);
		
		ColorList.Add(Color01);
		ColorList.Add(Color02);
		ColorList.Add(Color03);
		ColorList.Add(Color04);
		ColorList.Add(Color05);
		ColorList.Shuffle();
		
		OriginalColors.AddRange(ColorList);
		
		ExtraColors.Add(GREYDOT_COLOR);
		ExtraColors.Add(WHITEDOT_COLOR);
		
		highlightParms.Prop("localScale", ORIGINAL_SCALE * 1.3f);
		highlightParms.Ease(EaseType.EaseOutBack);
		
		deathParms.Prop("localScale", Vector3.zero);
		deathParms.Ease(EaseType.EaseInBack);
		
		lockParms.Prop("localScale", LOCKED_SCALE);
		lockParms.Ease(EaseType.EaseOutBack);
		
		resetLockParms.Prop("localScale", LOCKED_SCALE * 0.3f);
		resetLockParms.Ease(EaseType.EaseOutExpo);
		
		unlockParms.Prop("localScale", LOCKED_SCALE * 3f);
		unlockParms.Ease(EaseType.EaseInBack);
		
		highlightLockParms.Prop("localScale", LOCKED_SCALE * 1.1f);
		highlightLockParms.Ease(EaseType.EaseOutBack);
		
		lockLoopParms.Prop("localScale", LOCKED_SCALE * 1.1f);
		lockLoopParms.Loops(-1, LoopType.Yoyo);
		lockLoopParms.Ease(EaseType.EaseInOutCubic);
		
		hideParms.Prop("localScale", Vector3.zero);
		hideParms.Ease(EaseType.EaseInExpo);
		
		showParms.Prop("localScale", ORIGINAL_SCALE);
		showParms.Ease(EaseType.EaseOutExpo);
	}
	
	public void AddColors(int i)
	{
		if (i > ExtraColors.Count)
			return;
		var j = i;
		while (j > 0)
		{
			ColorList.Add(ExtraColors[ExtraColors.Count - 1]);
			ExtraColors.Remove(ExtraColors[ExtraColors.Count - 1]);
			--j;
		}
	}
	
	public void RemoveColors(int i)
	{
		if (i > ColorList.Count)
			return;
		var j = i;
		while (j > 0)
		{
			ExtraColors.Add(ColorList[ColorList.Count - 1]);
			ColorList.Remove(ColorList[ColorList.Count - 1]);
			--j;
		}
	}
	
	public void AddGreyDot()
	{
		if (!ColorList.Contains(GREYDOT_COLOR))
		{
			ColorList.Add(GREYDOT_COLOR);
			ExtraColors.Remove(GREYDOT_COLOR);
			Debug.Log("*** ADDED Grey Dot. ***");
		} else
			Debug.LogWarning("*** WARNING: Trying to add GREY Dot but it's already active. ***");
	}
	
	public void RemoveGreyDot()
	{
		if (ColorList.Contains(GREYDOT_COLOR))
		{
			ExtraColors.Add(GREYDOT_COLOR);
			ColorList.Remove(GREYDOT_COLOR);
			Debug.Log("*** REMOVED Grey Dot. ***");
		} else
			Debug.LogWarning("*** WARNING: Trying to REMOVE GREY Dot but it's already been removed. ***");
	}
	
	public void AddWhiteDot()
	{
		if (!ColorList.Contains(WHITEDOT_COLOR))
		{
			ColorList.Add(WHITEDOT_COLOR);
			ExtraColors.Remove(WHITEDOT_COLOR);
			Debug.Log("*** ADDED White Dot. ***");
		} else
			Debug.LogWarning("*** WARNING: Trying to ADD WHITE Dot but it's already active. ***");
	}
	
	public void RemoveWhiteDot()
	{
		if (ColorList.Contains(WHITEDOT_COLOR))
		{
			ExtraColors.Add(WHITEDOT_COLOR);
			ColorList.Remove(WHITEDOT_COLOR);
			Debug.Log("*** REMOVED White Dot. ***");
		} else
			Debug.LogWarning("*** WARNING: Trying to REMOVE WHITE Dot but it's already been removed. ***");
	}
}