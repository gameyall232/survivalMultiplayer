using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsDisplay : MonoBehaviour
{
	[SerializeField] Image healthWheel;
	[SerializeField] Image hungerWheel;
	[SerializeField] Image thirstWheel;
	[SerializeField] Image encumbranceWheel;
	[SerializeField] Image durabilityWheel;
	PlayerInventory inventory;
	
	public void Initialize()
	{
		inventory = GUIManager.PlayerInventory;
		
		inventory.InventoryChanged += activeSlot =>
		{
			RefreshDisplay(activeSlot);
		};
	}
	
	void RefreshDisplay(ItemSlot activeSlot)
	{
		encumbranceWheel.fillAmount = (float)inventory.weight / PlayerInventory.MAX_WEIGHT;
		
		if (activeSlot.DataKey == DataKeyType.Durability)
		{
			if (activeSlot.description.dataKey == DataKeyType.Durability)
			{
				durabilityWheel.fillAmount = activeSlot.dataValue / activeSlot.description.dataValue;
			}
			else { Debug.LogWarning("Discrepancy between saved and instanced custom data!"); }
		}
		else
		{
			durabilityWheel.fillAmount = 0;
		}
	}
}