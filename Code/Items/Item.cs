using System.Collections.Generic;

namespace Sandbox;

[Category( "Items" )]
public class Item : Component
{
	public ItemDefinition Definition { get; set; }
	public MagnetPlayer Player => MagnetPlayer.Local;
	public PlayerController Controller => MagnetPlayer.LocalController;

	protected override void OnStart()
	{
		if ( Game.IsClosing || !Player.IsValid() || !Controller.IsValid() ) return;
		if ( !Player.Items.TryAdd(Definition, 1) )
		{
			Player.Items[Definition]++;
		}
		Pickup();
	}

	protected override void OnDestroy()
	{
		if ( Game.IsClosing || !Player.IsValid() || !Controller.IsValid() ) return;
		if ( Player.Items.TryGetValue( Definition, out var count ) )
		{
			if ( count <= 1 )
			{
				Player.Items.Remove( Definition );
			}
			else
			{
				Player.Items[ Definition ]--;
			}
		}
		Drop();

	}

	public virtual void Pickup()
	{
		
	}

	public virtual void Drop()
	{
		
	}

	public void Render()
	{
		
	}
}
