using System;
using Sandbox;

[Category( "Items" )]
public class ItemPickup : Component, Component.ITriggerListener
{
	[Property] public ItemDefinition Definition { get; set; }
	[RequireComponent] SphereCollider Collider { get; set; }
	[RequireComponent] HighlightOutline Highlight { get; set; }
	public PlayerController Controller { get; set; }
	protected override void OnStart()
	{
		LocalPosition += Vector3.Up * 5f;
		if ( Definition.Tilted ) LocalRotation = LocalRotation.Angles().WithPitch( -15 );

		Collider.IsTrigger = true;
		Collider.Radius = MagnetPlayer.Local.PickupRadius;
		Collider.Center = new Vector3(0, 0, MagnetPlayer.Local.PickupRadius / 2 );

		Highlight.Color = ItemDefinition.RarityToColor( Definition.Rarity );
		Highlight.ObscuredColor = Color.Transparent;
	}

	protected override void OnFixedUpdate()
	{
		LocalRotation = LocalRotation.Angles().WithYaw( LocalRotation.Yaw() + 1 );
	}

	public virtual void OnTriggerEnter( GameObject other )
	{
		if ( !other.Components.TryGet<MagnetPlayer>( out var player ) ) return;

		MagnetPlayer.Local.GiveItem( Definition );
		GameObject.Destroy();
	}
}
