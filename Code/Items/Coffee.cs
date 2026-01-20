using Sandbox;

public sealed class Coffee : Item
{
    public float SpeedPercentage = 1.05f;
    public override void Pickup()
    {
        Controller.RunSpeed *= SpeedPercentage;
        Controller.WalkSpeed *= SpeedPercentage;
    }

    public override void Drop()
    {
        Controller.RunSpeed /= SpeedPercentage;
        Controller.WalkSpeed /= SpeedPercentage;
    }
}