//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright ?2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// All children added to the game object with this script will be arranged into a table
/// with rows and columns automatically adjusting their size to fit their content
/// (think "table" tag in HTML).
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/Table")]
public class UITable : MonoBehaviour
{
	public int columns = 0;
	public Vector2 padding = Vector2.zero;
	public bool sorted = false;
	public bool hideInactive = true;
	public bool repositionNow = false;
	public bool keepWithinPanel = false;

	UIPanel mPanel;

	/// <summary>
	/// Function that sorts items by name.
	/// </summary>

	static public int SortByName (Transform a, Transform b) { return string.Compare(a.name, b.name); }

	/// <summary>
	/// Positions the grid items, taking their own size into consideration.
	/// </summary>

	void RepositionVariableSize (List<Transform> children)
	{
		float xOffset = 0;
		float yOffset = 0;

		int cols = columns > 0 ? children.Count / columns + 1 : 1;
		int rows = columns > 0 ? columns : children.Count;

		Bounds[,] bounds = new Bounds[cols, rows];
		Bounds[] boundsRows = new Bounds[rows];
		Bounds[] boundsCols = new Bounds[cols];

		int x = 0;
		int y = 0;

		foreach (Transform t in children)
		{
			Bounds b = NGUIMath.CalculateRelativeWidgetBounds(t);
			Vector3 scale = t.localScale;
			b.min = Vector3.Scale(b.min, scale);
			b.max = Vector3.Scale(b.max, scale);
			bounds[y, x] = b;

			boundsRows[x].Encapsulate(b);
			boundsCols[y].Encapsulate(b);

			if (++x >= columns && columns > 0)
			{
				x = 0;
				++y;
			}
		}

		x = 0;
		y = 0;

		foreach (Transform t in children)
		{
			Bounds b = bounds[y, x];
			Bounds br = boundsRows[x];
			Bounds bc = boundsCols[y];

			Vector3 pos = t.localPosition;
			pos.x =   xOffset + b.extents.x - b.center.x;
			pos.y = -(yOffset + b.extents.y + b.center.y);

			pos.x += b.min.x - br.min.x + padding.x;
			pos.y += (b.max.y - b.min.y - bc.max.y + bc.min.y) * 0.5f - padding.y;

			xOffset += br.max.x - br.min.x + padding.x * 2f;

			t.localPosition = pos;

			if (++x >= columns && columns > 0)
			{
				x = 0;
				++y;

				xOffset = 0f;
				yOffset += bc.size.y + padding.y * 2f;
			}
		}
	}

	/// <summary>
	/// Reposition all elements in the grid.
	/// </summary>

	public void Reposition ()
	{
		Transform myTrans = transform;
		List<Transform> children = new List<Transform>();

		for (int i = 0; i < myTrans.childCount; ++i)
		{
			Transform child = myTrans.GetChild(i);

			if (!hideInactive || child.gameObject.activeSelf)
			{
				children.Add(child);
			}
		}
		if (sorted) children.Sort(SortByName);
		if (children.Count > 0) RepositionVariableSize(children);
		if (mPanel != null) mPanel.ConstrainTargetToBounds(myTrans, true);
	}

	/// <summary>
	/// Position the grid's contents when the script starts.
	/// </summary>

	void Start ()
	{
		if (keepWithinPanel) mPanel = NGUITools.FindInParents<UIPanel>(gameObject);
		Reposition();
	}

	/// <summary>
	/// Is it time to reposition? Do so now.
	/// </summary>

	void LateUpdate ()
	{
		if (repositionNow)
		{
			repositionNow = false;
			Reposition();
		}
	}
}