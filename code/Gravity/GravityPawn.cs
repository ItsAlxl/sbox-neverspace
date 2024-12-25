namespace Neverspace;

[Group( "Neverspace - Gravity" )]
[Title( "Gravity Pawn" )]
[Icon( "play_for_work" )]

public sealed class GravityPawn : Component
{
	[Property] Vector3 BaseGravity { get; set; } = new( 0.0f, 0.0f, -800.0f );

	public Vector3 WalkwayGravity { get; set; }
	public Vector3 CurrentGravity { get => WalkwayGravity.IsNearZeroLength ? BaseGravity : WalkwayGravity; }

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
