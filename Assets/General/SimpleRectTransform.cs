using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(RectTransform))]
public class SimpleRectTransform : MonoBehaviour
{
	RectTransform attached;

	// Backing Fields
	[SerializeField] Vector2 positionScale;
	[SerializeField] Vector2 sizeScale;
	[Space]
	[SerializeField] Vector2 positionOffset;
	[SerializeField] Vector2 sizeOffset;
	[Space]
	[Space]
	[SerializeField] Vector2 anchor;
	[SerializeField] float rotation;

	#region Properties
	public Vector2 PositionScale
	{
		get { return positionScale; }
		set
		{
			positionScale = value;
			SetRectValues();
		}
	}
	public Vector2 SizeScale
	{
		get { return sizeScale; }
		set
		{
			sizeScale = value;
			SetRectValues();
		}
	}
	public Vector2 PositionOffset
	{
		get { return positionOffset; }
		set
		{
			positionOffset = value;
			SetRectValues();
		}
	}
	public Vector2 SizeOffset
	{
		get { return sizeOffset; }
		set
		{
			sizeOffset = value;
			SetRectValues();
		}
	}
	public Vector2 Anchor
	{
		get { return anchor; }
		set
		{
			anchor = value;
			SetRectValues();
		}
	}
	public float Rotation
	{
		get { return rotation; }
		set
		{
			rotation = value;
			SetRectValues();
		}
	}
	#endregion

	public void SetRectValues()
	{
		if (GetAttached())
		{
			float left = positionScale.x - (sizeScale.x * anchor.x);
			float right = positionScale.x + (sizeScale.x * (1 - anchor.x));
			float top = positionScale.y - (sizeScale.y * anchor.y);
			float bottom = positionScale.y + (sizeScale.y * (1 - anchor.y));

			attached.anchorMin = new Vector2(left, top);
			attached.anchorMax = new Vector2(right, bottom);

			float leftOffset = positionOffset.x - (sizeOffset.x * anchor.x);
			float rightOffset = positionOffset.x + (sizeOffset.x * (1 - anchor.x));
			float topOffset = positionOffset.y - (sizeOffset.y * anchor.y);
			float bottomOffset = positionOffset.y + (sizeOffset.y * (1 - anchor.y));

			attached.offsetMin = new Vector2(leftOffset, topOffset);
			attached.offsetMax = new Vector2(rightOffset, bottomOffset);

			attached.eulerAngles = new Vector3(0, 0, rotation);
		}
	}

	bool GetAttached()
	{
		if (TryGetComponent(out RectTransform component))
		{
			attached = component;
			return true;
		}
		return false;
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(SimpleRectTransform))]
public class SimpleRectTransformEditor : Editor
{
	public override void OnInspectorGUI()
	{
		SimpleRectTransform simpleRect = (SimpleRectTransform)target;

		if (DrawDefaultInspector())
		{
			simpleRect.SetRectValues();
		}
	}
}
#endif