using System;
using System.Collections.Generic;
using UnityEngine;

// Exists on All Clients
public class AnimationManager : MonoBehaviour
{
	public static AnimationManager Instance { get; private set;}
	
	void Awake()
	{
		Instance = this;
	}
	
	public const float CraftAnimationTime = .4f;
	public const float BounceHeight = .5f;
	
	public List<CraftAnimation> craftAnimations = new List<CraftAnimation>();

	void Update()
	{
		for (int i = craftAnimations.Count - 1; i >= 0; i--)
		{
			CraftAnimation craftAnimation = craftAnimations[i];
			bool keep = craftAnimation.ProgressBy(Time.deltaTime);
			
			if (keep)
			{
				craftAnimations[i] = craftAnimation;
			}
			else
			{
				craftAnimation.LockItems(false);
				craftAnimations.RemoveAt(i);
			}
		}
	}
	
	public void AnimateCraft(Transform[] itemTransforms, Vector3 craftPosition)
	{
		craftAnimations.Add(new CraftAnimation(itemTransforms, craftPosition));
	}
}

[Serializable]
public struct CraftAnimation
{
	Transform[] itemTransforms;
	Vector3[] initialPositions;
	Vector3 craftPosition;
	float timer;
	
	public CraftAnimation(Transform[] itemTransforms, Vector3 craftPosition)
	{
		this.itemTransforms = itemTransforms;
		initialPositions = new Vector3[itemTransforms.Length];
		for (int i = 0; i < itemTransforms.Length; i++)
		{
			initialPositions[i] = itemTransforms[i].position;
		}
		this.craftPosition = craftPosition;
		timer = 0;
		LockItems(true);
	}

	public bool ProgressBy(float deltaTime)
	{
		timer += deltaTime;
		if (timer > AnimationManager.CraftAnimationTime) { return false; }
		
		for (int i = 0; i < itemTransforms.Length; i++)
		{
			itemTransforms[i].position = 
				Vector3.Lerp(initialPositions[i], craftPosition, timer / AnimationManager.CraftAnimationTime)
				+ new Vector3(0, HeightAtTime(timer / AnimationManager.CraftAnimationTime), 0);
		}
		return true;
	}
	
	public void LockItems(bool boolean)
	{
		foreach (Transform item in itemTransforms)
		{
			item.GetComponent<Item>().LockItem(boolean);
		}
	}

	static float HeightAtTime(float t)
	{
		// t expected as 0 to 1
		return (-4 * AnimationManager.BounceHeight * Mathf.Pow(t, 2)) + (4 * AnimationManager.BounceHeight * t);
	}
}