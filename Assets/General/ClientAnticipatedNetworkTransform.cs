using Unity.Netcode.Components;

public class ClientAnticipatedNetworkTransform : AnticipatedNetworkTransform
{
	protected override bool OnIsServerAuthoritative()
	{
		return false;
	}
}