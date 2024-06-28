using UnityEngine;

public class GUIManager : MonoBehaviour
{
	public static GUIManager Instance { get; private set;}
	public static PlayerInteract PlayerInteract => Instance.player;
	public static PlayerInventory PlayerInventory => Instance.playerInventory;
	
	PlayerInteract player;
	PlayerInventory playerInventory;
	[SerializeField] HotbarUI hotbar;
	[SerializeField] PlayerStatsDisplay playerStatsDisplay;
	
	void Awake()
	{
		Instance = this;
	}
	
	public void Initialize(PlayerInteract player)
	{
		this.player = player;
		playerInventory = player.GetComponent<PlayerInventory>();
		transform.GetChild(4).gameObject.SetActive(true);
		transform.GetChild(5).gameObject.SetActive(true);
		hotbar.Initialize();	
		playerStatsDisplay.Initialize();
	}
}