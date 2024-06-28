using System;
using Unity.Netcode;
using UnityEngine;

public class Item : NetworkBehaviour, ISelectable
{
	[field: SerializeField] public ItemDescription itemDescription { get; private set; }
	public NetworkVariable<DataTag> dataTag = new 
		NetworkVariable<DataTag>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
	public DataTag initialDataTag;
	
	public NetworkVariable<bool> CanPickup =
		new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
	bool canPickup = true;

	[SerializeField] Material[] modelMaterials;
	[SerializeField] Renderer[] renderers;

	public override void OnNetworkSpawn()
	{
		dataTag.Value = initialDataTag;
		CanPickup.OnValueChanged += (bool _, bool newBool) => { canPickup = newBool; };
	}

	public virtual void Damage()
	{
		ServerDestroy();
	}

	public virtual void Animate(string animationName)
	{
		//TODO: implement this
	}

	public void Highlight(bool on)
	{
		for (int i = 0; i < renderers.Length; i++)
		{
			if (renderers[i] != null)
			{
				Material[] newMats = renderers[i].materials;

				if (newMats.Length > 1)
				{
					newMats[1] = on ? ItemInteractionManager.Instance.outlineMaterial : modelMaterials[i];
					renderers[i].materials = newMats;
				}
			}
		}
	}

	public virtual void ServerDestroy()
	{
		GetComponent<NetworkObject>().Despawn(true);
	}

	public override void OnNetworkObjectParentChanged(NetworkObject parentNetworkObject)
	{
		if (parentNetworkObject != null)
		{
			if (parentNetworkObject.transform.TryGetComponent(out PlayerInteract player))
			{
				transform.localPosition = PlayerInteract.handOffset;
				transform.localEulerAngles = Vector3.zero;
				LockItem(true);
			}
		}
		else
		{
			LockItem(false);
		}
	}

	public void LockItem(bool boolean)
	{
		if (IsServer) { CanPickup.Value = !boolean; }
		canPickup = !boolean;
		GetComponent<Rigidbody>().isKinematic = boolean;
	}
}

public struct DataTag : INetworkSerializable
{
	DataKeyType key;
	public DataKeyType Key { get { return key; } }
	public float data;

	public static bool operator ==(DataTag left, DataTag right)
	{
		return left.key == right.key && left.data == right.data;
	}
	public static bool operator !=(DataTag left, DataTag right)
	{
		return !(left == right);
	}
	
	public DataTag(DataKeyType key, float value)
	{
		this.key = key;
		data = value;
	}

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		Debug.Log(key.ToString());
		serializer.SerializeValue(ref key);
		serializer.SerializeValue(ref data);
	}

	public override bool Equals(object obj)
	{
		if (obj is DataTag)
		{
			DataTag other = (DataTag)obj;
			return this == other;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(key, data);
	}
}

public enum DataKeyType
{
	None,
	Durability,
	WorkbenchId
}