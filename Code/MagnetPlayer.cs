using Sandbox;

public sealed class MagnetPlayer : Component
{
	[RequireComponent] HullCollider Collider { get; set; }
	
	protected override void OnFixedUpdate()
	{

	}
}
