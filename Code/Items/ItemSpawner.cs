using Sandbox;

[Category( "Items" )]
public sealed class ItemSpawner : Component
{
	[Property] ItemDefinition ItemDefinition { get; set; }
	protected override void OnStart()
	{
		ItemDefinition.Prefab.Clone( WorldPosition );
	}
}
