using UnityEngine;
using UnityEngine.InputSystem;

public class ClientInput : MonoBehaviour 
{
	InputMaster inputMaster = null;
	
	public InputAction[] hotbarActions = new InputAction[10]
	{
		new InputAction("Hotbar0", InputActionType.Button, "<Keyboard>/0"),
		new InputAction("Hotbar1", InputActionType.Button, "<Keyboard>/1"),
		new InputAction("Hotbar2", InputActionType.Button, "<Keyboard>/2"),
		new InputAction("Hotbar3", InputActionType.Button, "<Keyboard>/3"),
		new InputAction("Hotbar4", InputActionType.Button, "<Keyboard>/4"),
		new InputAction("Hotbar5", InputActionType.Button, "<Keyboard>/5"),
		new InputAction("Hotbar6", InputActionType.Button, "<Keyboard>/6"),
		new InputAction("Hotbar7", InputActionType.Button, "<Keyboard>/7"),
		new InputAction("Hotbar8", InputActionType.Button, "<Keyboard>/8"),
		new InputAction("Hotbar9", InputActionType.Button, "<Keyboard>/9")
	};
	
	public InputMaster.PlayerActions Primary
	{
		get
		{
			if (inputMaster == null)
			{
				inputMaster = new InputMaster();
				inputMaster.Enable();
				Cursor.lockState = CursorLockMode.Locked;
			}
			return inputMaster.Player;
		}
	}

	private void OnEnable() { if (inputMaster != null) { inputMaster.Enable(); } }
	private void OnDisable() { if (inputMaster != null) { inputMaster.Disable(); } }
}