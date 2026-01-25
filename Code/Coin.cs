using Sandbox;
using System;

public sealed class Coin : Component
{
	TimeSince TimeSince = 0;
	float speed = 1f;
	float amount = 3;
	float startingHeight = 3.5f;
	protected override void OnFixedUpdate()
	{
		WorldRotation = WorldRotation.Angles().WithYaw(WorldRotation.Yaw() + 2);
		WorldPosition = WorldPosition.WithZ((float)Math.Sin(TimeSince * speed) * amount + startingHeight);
	}
}
