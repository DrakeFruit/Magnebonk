using Sandbox;

public sealed class Coin : Component
{
	protected override void OnFixedUpdate()
	{
		WorldRotation = WorldRotation.Angles().WithYaw(WorldRotation.Yaw() + 2);
	}
}
