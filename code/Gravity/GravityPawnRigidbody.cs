namespace Neverspace;

[Group( "Neverspace - Gravity" )]
[Title( "Gravity Pawn - Rigidbody" )]
[Icon( "fitness_center" )]

public class GravityPawnRigidbody : GravityPawn
{
	private IEnumerable<Rigidbody> bodies;
	private float StartScaleZ;

	protected override void OnAwake()
	{
		base.OnAwake();
		bodies = GameObject.Components.GetAll<Rigidbody>();

		foreach ( var b in bodies )
		{
			b.Gravity = false;
		}
		StartScaleZ = WorldScale.z;
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
		foreach ( var b in bodies )
		{
			b.ApplyForce( b.Mass * CurrentGravity * WorldScale / LocalScale * WorldScale.z / StartScaleZ );
		}
	}
}
