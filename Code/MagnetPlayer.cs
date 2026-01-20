using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using Sandbox;

public sealed class MagnetPlayer : Component, Component.ITriggerListener
{
	[Property] public int CoinMultiplier { get; set; } = 1;
	[Property] public int PickupRadius { get; set; } = 30;
	[RequireComponent] HullCollider Collider { get; set; }
	public List<Coin> CoinsList { get; set; } = new();
	public int Coins { get; set; }

	public static MagnetPlayer Local => Game.ActiveScene.GetAll<MagnetPlayer>().FirstOrDefault( x => !x.IsProxy );
	public static PlayerController LocalController => Local.GetComponent<PlayerController>();
	[Sync] public NetDictionary<ItemDefinition, int> Items { get; set; } = new();
	
	protected override void OnFixedUpdate()
	{
		Collider.Radius = PickupRadius;
		EnumerateCoins();
	}

	protected override void OnUpdate()
	{
		Gizmo.Draw.ScreenText( Coins.ToString(), new Vector2(25, 25), "sans-serif", 45 );
	}

	public void EnumerateCoins()
	{
		foreach ( var i in CoinsList )
		{
			if ( !i.IsValid() )
			{
				CoinsList.Remove(i);
				return;
			}
			else
			{
				i.WorldPosition = Vector3.Lerp( i.WorldPosition, WorldPosition, 0.05f );
				if ( (i.WorldPosition - WorldPosition).Length < 30 )
				{
					i.GameObject.Destroy();
					Coins += CoinMultiplier;
				}
			}
		}
	}

	public void OnTriggerEnter( GameObject other )
	{
		var coin = other.GetComponent<Coin>();
		if ( coin.IsValid() )
		CoinsList.Add(coin);
	}
}
