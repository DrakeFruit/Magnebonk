using System;
using Sandbox;

[Category( "Items" )]
public class ItemPickup : Component, Component.ITriggerListener
{
	[Property] public ItemDefinition Definition { get; set; }
	[RequireComponent] SphereCollider Collider { get; set; }
	[RequireComponent] HighlightOutline Highlight { get; set; }
	public PlayerController Controller { get; set; }
	public MagnetPlayer Player { get; set; }
	protected override void OnStart()
	{
		LocalPosition += Vector3.Up * 5f;
		if ( Definition.Tilted ) LocalRotation = LocalRotation.Angles().WithPitch( -15 );

		var radius = 25;
		Collider.IsTrigger = true;
		Collider.Radius = radius;
		Collider.Center = new Vector3(0, 0, radius / 2 );

		Highlight.Color = ItemDefinition.RarityToColor( Definition.Rarity );
	}

	protected override void OnFixedUpdate()
	{
		LocalRotation = LocalRotation.Angles().WithYaw( LocalRotation.Yaw() + 1 );
	}

	public virtual void OnTriggerEnter( GameObject other )
	{
		Controller = other.GetComponent<PlayerController>();
		Player = other.GetComponent<MagnetPlayer>();
		if ( !Controller.IsValid() || !Player.IsValid() ) return;

		var type = TypeLibrary.GetType( Definition.ItemComponent );
		var comp = Player.Components.Create( type );
		if ( comp is Item item )
		{
			item.Definition = Definition;
		}
		GameObject.Destroy();
	}
}
