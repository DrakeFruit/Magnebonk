using Sandbox;
using Sandbox.UI;
using System;

[AssetType( Category = "Magnebonk", Extension = "item", Name = "Item Definition", Flags = AssetTypeFlags.IncludeThumbnails )]
public class ItemDefinition : GameResource
{
	public string Name { get; set; }
	public string Description { get; set; }
	[TargetType(typeof(Item))] public Type ItemComponent { get; set; }
	public ItemRarity Rarity { get; set; } = ItemRarity.Common;
	public GameObject Prefab { get; set; }
	[Description( "Tilts the item 15 degrees on the ground and in previews" )] public bool Tilted { get; set; } = true;
	public Texture Icon { get; set; }

	public enum ItemRarity
	{
		Common,
		Uncommon,
		Rare,
		Epic,
		Legendary,
		Mythic
	}

	public static Color RarityToColor( ItemRarity rarity )
	{
		return rarity switch
		{
			ItemRarity.Common => Color.White,
			ItemRarity.Uncommon => Color.Green,
			ItemRarity.Rare => Color.Blue,
			ItemRarity.Epic => Color.Magenta,
			ItemRarity.Legendary => Color.Yellow,
			ItemRarity.Mythic => Color.Red,
			_ => Color.White,
		};
	}
}
