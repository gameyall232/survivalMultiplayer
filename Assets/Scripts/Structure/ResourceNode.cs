public abstract class ResourceNode : Structure
{
	public ResourceNodeType type;

	public abstract void Harvest();
}

public enum ResourceNodeType
{
	OreDeposit,
	Tree
}