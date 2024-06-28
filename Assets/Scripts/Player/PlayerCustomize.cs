using UnityEngine;
using Unity.Netcode;

public class PlayerCustomize : NetworkBehaviour
{
	[SerializeField] Material[] defaultPlayerMaterials;
	[SerializeField] GameObject body;
	
	public override void OnNetworkSpawn()
	{
		body.GetComponent<Renderer>().material = defaultPlayerMaterials[OwnerClientId];
	}
}