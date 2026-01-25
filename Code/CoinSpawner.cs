using Sandbox;
using System.Collections.Generic;
using System.Linq;

public sealed class CoinSpawner : Component
{
	[Property] GameObject Coin { get; set; }

	int coinCount => Scene.Components.GetAll<Coin>().Count();
	int maxCoins = 1000;
	protected override void OnFixedUpdate()
	{
		if( coinCount < maxCoins )
		{
			for( var i = 0; i < 3; i++ )
			{
				var position = Scene.NavMesh.GetRandomPoint();
				var coin = Coin.Clone();
				if ( position != null )
				coin.WorldPosition = position.Value;
			}
		}
	}
}
