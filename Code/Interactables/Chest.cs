using System.Linq;
using Sandbox;

public sealed class Chest : Component, Component.IPressable
{
	ChestUI ChestUI { get; set; }
    bool IPressable.Press(IPressable.Event e)
	{
		var chestUI = Scene.GetAllComponents<ChestUI>().FirstOrDefault();
		if ( chestUI.IsValid() ) return false;

		var screenPanel = Scene.GetAllComponents<ScreenPanel>().FirstOrDefault();
		if ( !screenPanel.IsValid() ) return false;

		ChestUI = screenPanel.GameObject.AddComponent<ChestUI>();
		var itemList = ResourceLibrary.GetAll<ItemDefinition>().Where( x => x.Rarity == ItemDefinition.ItemRarity.Common ).Shuffle().Take(5);
		foreach( var i in itemList )
		{
			Log.Info(i);
			ChestUI.PotentialItems.Add(i);
		}
		return true;
	}
}
