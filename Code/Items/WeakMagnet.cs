using Sandbox;

public sealed class WeakMagnet : Item
{
    public float Multiplier = 1.1f;
    public override void Pickup()
    {
        Player.PickupRadius *= Multiplier;
    }

    public override void Drop()
    {
        Player.PickupRadius /= Multiplier;
    }
}