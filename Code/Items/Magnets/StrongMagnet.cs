using Sandbox;

public sealed partial class StrongMagnet : Item
{
    public float Multiplier = 100f;

    public override void Pickup()
    {
        Player.PickupRadius *= Multiplier;
    }

    public override void Drop()
    {
        Player.PickupRadius /= Multiplier;
    }
}