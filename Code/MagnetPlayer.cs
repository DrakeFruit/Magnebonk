using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using Sandbox;

public sealed class MagnetPlayer : Component, Component.ITriggerListener
{
	[Property] public float CoinMultiplier { get; set; } = 1;
	[Property] public float PickupRadius { get; set; } = 75;
	[Property] public int InteractionDistance { get; set; } = 100;
	[RequireComponent] HullCollider Collider { get; set; }
	public List<Coin> CoinsList { get; set; } = new();
	public int Coins { get; set; }

	public static MagnetPlayer Local => Game.ActiveScene.GetAll<MagnetPlayer>().FirstOrDefault( x => !x.IsProxy );
	public static PlayerController LocalController => Local.GetComponent<PlayerController>();
	[Sync] public NetDictionary<ItemDefinition, int> Items { get; set; } = new();

	private Component _lastClosestComponent;
	private HighlightOutline _activeHighlight;

	protected override void OnFixedUpdate()
	{
	    Collider.Radius = PickupRadius;
	    EnumerateCoins();

	    float distSq = InteractionDistance * InteractionDistance;
	
	    var closestPressable = Scene.GetAllComponents<Component.IPressable>()
	        .OfType<Component>()
	        .Where( x => x.WorldPosition.DistanceSquared( WorldPosition ) < distSq )
	        .OrderBy( x => x.WorldPosition.DistanceSquared( WorldPosition ) )
	        .FirstOrDefault();

	    if ( closestPressable != _lastClosestComponent )
	    {
	        ClearCurrentHighlight();

	        if ( closestPressable.IsValid() )
	        {
	            _activeHighlight = GetHighlight( closestPressable.GameObject );
	            if ( _activeHighlight.IsValid() )
	            {
	                _activeHighlight.Enabled = true;
	            }
	        }
	
	        _lastClosestComponent = closestPressable;
	    }

	    if ( closestPressable is Component.IPressable pressable && Input.Pressed( "use" ) )
	    {
	        pressable.Press( new IPressable.Event( closestPressable ) );
	    }
	}

	private void ClearCurrentHighlight()
	{
	    if ( _activeHighlight.IsValid() )
	    {
	        _activeHighlight.Enabled = false;
	    }
	    _activeHighlight = null;
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
					Coins += (int)CoinMultiplier;
				}
			}
		}
	}

	public void OnTriggerEnter( GameObject other )
	{
		var coin = other.GetComponent<Coin>();
		if ( coin.IsValid() )
		{
			CoinsList.Add(coin);
			return;
		}
	}

	public HighlightOutline GetHighlight( GameObject obj )
	{
		var interactable = obj.GetComponent<Component.IPressable>();
		if ( interactable != null )
		{
			if ( interactable is Component comp )
			{
				var highlight = comp.Components.GetInDescendantsOrSelf<HighlightOutline>( true );
				if ( highlight.IsValid() ) return highlight;

				var renderer = comp.Components.GetInDescendantsOrSelf<ModelRenderer>( true ) ?? 
				               comp.Components.GetInDescendantsOrSelf<SkinnedModelRenderer>( true );

				return renderer.IsValid() 
				    ? renderer.Components.GetOrCreate<HighlightOutline>( FindMode.EverythingInSelf ) 
				    : null;
			}
		}
		return null;
	}

	public void GiveItem( ItemDefinition definition )
	{
		var type = TypeLibrary.GetType( definition.ItemComponent );
		var comp = MagnetPlayer.Local.Components.Create( type );
		if ( comp is Item item )
		{
			item.Definition = definition;
		}
	}
}
