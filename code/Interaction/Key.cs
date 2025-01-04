namespace Neverspace;

[Group( "Neverspace - Interaction" )]
[Title( "Key" )]
[Icon( "vpn_key" )]

public sealed class Key : Carriable
{
	const float LOCKIN_ROTATE_SPEED = 1.5f;
	const float LOCKIN_MOVE_SPEED = 2.5f;
	const float LOCKIN_RESCALE_SPEED = 2.0f;

	[Property] Vector3 LockLocalScale { get; set; } = Vector3.One * 0.5f;
	private Vector3 lockedTargetPos;

	public void LockIn( Vector3 WorldPos )
	{
		canBeCarried = false;
		lockedTargetPos = WorldPos;
		carrier?.StopCarrying();
		foreach ( var b in GameObject.Components.GetAll<Rigidbody>() )
		{
			b.Enabled = false;
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if ( !canBeCarried )
		{
			LocalRotation = Rotation.Lerp( LocalRotation, Rotation.Identity, LOCKIN_ROTATE_SPEED * Time.Delta );
			LocalScale = LocalScale.LerpTo( LockLocalScale, LOCKIN_RESCALE_SPEED * Time.Delta );
			WorldPosition = WorldPosition.LerpTo( lockedTargetPos, LOCKIN_MOVE_SPEED * WorldScale.x * Time.Delta );
		}
	}
}
