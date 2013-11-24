using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class ChainLine : MonoBehaviour
{
	private LineRenderer lineRenderer;

	void Awake()
	{
		lineRenderer = gameObject.GetComponent<LineRenderer>() as LineRenderer;
		lineRenderer.SetWidth(0.05f, 0.05f);
		lineRenderer.SetVertexCount(4);
		lineRenderer.material = new Material(Shader.Find("Custom/Simple Transparent"));
		lineRenderer.SetColors(ArtManager.Instance.CHAIN_COLOR, ArtManager.Instance.CHAIN_COLOR);
	}

	void OnDestroy()
	{
        DestroyImmediate(renderer.material);
    }

	public void DrawLine(Vector3 startPosition, Vector3 endPosition)
	{
		// Offsets Z to make sure lines render in front of spheres.
		startPosition += new Vector3(0f, 0f, 0.6f);
		endPosition += new Vector3(0f, 0f, 0.6f);
		lineRenderer.SetPosition(0, startPosition);
		lineRenderer.SetPosition(2, startPosition);
		lineRenderer.SetPosition(1, endPosition);
		lineRenderer.SetPosition(3, endPosition);
	}

	public void FadeLine()
	{
		HOTween.To(lineRenderer.material, 0.1f, "color", lineRenderer.material.color - new Color(0f, 0f, 0f, 1f));
	}
}