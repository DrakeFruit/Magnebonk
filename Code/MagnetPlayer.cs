using System;
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
    private WorldPanel _activeTooltip;

    protected override void OnFixedUpdate()
    {
        if ( Collider.IsValid() )
            Collider.Radius = PickupRadius;

        UpdatePressing();
        EnumerateCoins();
    }

    public void UpdatePressing()
    {
        var closestPressable = Scene.GetAllComponents<Component.IPressable>()
            .OfType<Component>()
            .Where( x => (x.WorldPosition - WorldPosition).Length < InteractionDistance )
            .OrderBy( x => x.WorldPosition.DistanceSquared( WorldPosition ) )
            .FirstOrDefault();

        if ( (_lastClosestComponent.IsValid() && (_lastClosestComponent.WorldPosition - WorldPosition).Length > InteractionDistance) || 
             (closestPressable.IsValid() && closestPressable != _lastClosestComponent) )
        {
            ClearCurrent();

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

        if ( !closestPressable.IsValid() ) return;

        if ( closestPressable is Component.IPressable pressable && Input.Pressed( "use" ) )
        {
            pressable.Press( new IPressable.Event( closestPressable ) );
        }

        _activeTooltip = closestPressable.Components.Get<WorldPanel>( FindMode.EverythingInSelfAndDescendants );
        if ( _activeTooltip.IsValid() )
        {
            _activeTooltip.Enabled = true;
        }
    }

    private void ClearCurrent()
    {
        if ( _activeHighlight.IsValid() )
            _activeHighlight.Enabled = false;
        
        _activeHighlight = null;

        if ( _activeTooltip.IsValid() )
            _activeTooltip.Enabled = false;
        
        _activeTooltip = null;
    }

    protected override void OnUpdate()
    {
        Gizmo.Draw.ScreenText( $"Coins: {Coins}", new Vector2( 25, 25 ), "sans-serif", 45 );
    }

    public void EnumerateCoins()
    {
        for ( int i = CoinsList.Count - 1; i >= 0; i-- )
        {
            var coin = CoinsList[i];

            if ( !coin.IsValid() )
            {
                CoinsList.RemoveAt( i );
                continue;
            }

            float lerpSpeed = 10f; 
            coin.WorldPosition = Vector3.Lerp( coin.WorldPosition, WorldPosition, Time.Delta * lerpSpeed );

            if ( (coin.WorldPosition - WorldPosition).Length < 35f )
            {
                Coins += (int)CoinMultiplier;
                coin.GameObject.Destroy();
                CoinsList.RemoveAt( i );
            }
        }
    }

    public void OnTriggerEnter( GameObject other )
    {
        var coin = other.GetComponent<Coin>();
        if ( coin.IsValid() && !CoinsList.Contains( coin ) )
        {
            CoinsList.Add( coin );
        }
    }

    public HighlightOutline GetHighlight( GameObject obj )
    {
        var interactable = obj.GetComponent<Component.IPressable>();
        if ( interactable != null && interactable is Component comp )
        {
            var highlight = comp.Components.GetInDescendantsOrSelf<HighlightOutline>( true );
            if ( highlight.IsValid() ) return highlight;

            var renderer = comp.Components.GetInDescendantsOrSelf<ModelRenderer>( true ) ?? 
                           comp.Components.GetInDescendantsOrSelf<SkinnedModelRenderer>( true );

            return renderer.IsValid() 
                ? renderer.Components.GetOrCreate<HighlightOutline>( FindMode.EverythingInSelf ) 
                : null;
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