namespace Neverspace;

[Group( "Neverspace - Gravity" )]
[Title( "Gravity Pawn" )]
[Icon( "fitness_center" )]

public sealed class GravityPawn : Component
{
	[Property] Vector3 BaseGravity { get; set; } = new( 0.0f, 0.0f, -800.0f );

	public Walkway ActiveWalkway { get; set; }
	public Vector3 CurrentGravity { get => ActiveWalkway == null ? BaseGravity : ActiveWalkway.Gravity; }

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
			b.Velocity += CurrentGravity * Time.Delta;
		}
	}
}
