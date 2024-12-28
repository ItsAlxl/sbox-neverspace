namespace Neverspace;

[Group( "Neverspace - Gravity" )]
[Title( "Gravity Pawn - Rigidbody" )]
[Icon( "fitness_center" )]

public class GravityPawnRigidbody : GravityPawn
{
	private IEnumerable<Rigidbody> bodies;

	protected override void OnAwake()
	{
		base.OnAwake();
		bodies = GameObject.Components.GetAll<Rigidbody>();

		foreach ( var b in bodies )
		{
			b.Gravity = false;
		}
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
		foreach ( var b in bodies )
		{
			b.ApplyForce( CurrentGravity * b.Mass * 2.5f ); // idk why the x2.5 is there either
		}
	}
}
