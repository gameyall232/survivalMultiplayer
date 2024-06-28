using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.Netcode.Transports.UTP;

public class NetworkManagerUI : MonoBehaviour
{
	[SerializeField] GameObject connectionPanel;
	[SerializeField] Button hostBtn;
	[SerializeField] Button clientBtn;
	[SerializeField] Button serverBtn;
	
	[SerializeField] TMPro.TMP_InputField ipField;
	[SerializeField] TMPro.TMP_InputField portField;
	
	void Awake()
	{
		hostBtn.onClick.AddListener(() =>
		{
			NetworkManager.Singleton.StartHost();
			connectionPanel.SetActive(false);
		});
		clientBtn.onClick.AddListener(() =>
		{
			NetworkManager.Singleton.StartClient();
			connectionPanel.SetActive(false);
		});
		serverBtn.onClick.AddListener(() =>
		{
			NetworkManager.Singleton.StartServer();
			connectionPanel.SetActive(false);
		});
	}
	
	public void SetData(int mode)
	{
		string ip = "127.0.0.1";
		ushort port = 25565;
		
		switch (mode)
		{
			case 0:
				ip = ipField.text;
				port = ushort.Parse(portField.text);
				break;
			case 1:
				ip = "192.168.7.242";
				port = 25565;
				break;
			case 2:
				// localhost, default values
				break;
		}
		
		NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(
			ip, port, "0.0.0.0"
		);
	}
}