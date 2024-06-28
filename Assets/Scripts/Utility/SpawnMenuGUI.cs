using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using TMPro;

public class SpawnMenuGUI : MonoBehaviour
{
	public static SpawnMenuGUI Instance;
	
	[SerializeField] TMP_Dropdown itemType;
	[SerializeField] TMP_InputField amount;
	
	void Awake()
	{
		Instance = this;
	}
	
	void Start()
	{
		var optionsList = new List<TMP_Dropdown.OptionData>();

		for (int i = 0; i < ItemInteractionManager.ItemDescriptions.Length; i++)
		{
			optionsList.Add(new(ItemInteractionManager.ItemDescriptions[i].itemType.ToString()));
		}

		itemType.options = optionsList;
	}
	
	public void Spawn()
	{
		if (NetworkManager.Singleton.IsServer)
		{
			for (int i = 0; i < int.Parse(amount.text); i++)
			{
				ItemInteractionManager.SpawnNew((ItemType)itemType.value, new Vector3(0, 2, 0));
			}
		}
	}
	
	public void ToggleMouse()
	{
		bool locked = Cursor.lockState == CursorLockMode.Locked;
		Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
	}
}