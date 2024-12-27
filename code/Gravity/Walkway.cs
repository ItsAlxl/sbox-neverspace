namespace Neverspace;

[Group( "Neverspace - Gravity" )]
[Title( "Walkway" )]
[Icon( "route" )]

public sealed class Walkway : Component
{
	[Property] public float GravityStrength { get; set; } = 800.0f;
	public Vector3 Gravity { get => GravityStrength * WorldTransform.Down; }
	private Collider col;

	protected override void OnAwake()
	{
		base.OnAwake();
		col = GetComponent<Collider>();
		var trigger = col.CreateDuplicate() as Collider;
		trigger.IsTrigger = true;
		trigger.OnTriggerEnter += OnTriggerEntered;
		trigger.OnTriggerExit += OnTriggerExit;
		if ( trigger is BoxCollider b )
		{
			var downscale = b.Scale.z * 0.5f;
			b.Scale = new( b.Scale.x - downscale, b.Scale.y - downscale, b.Scale.z );
			b.Center = b.Center.WithZ( b.Center.z + b.Scale.z );
		}
		Tags.Add( "walkway" );
	}

	public void AddGravPawn( GravityPawn gp )
	{
		gp.ActiveWalkway = this;
	}

	public void RemoveGravPawn( GravityPawn gp )
	{
		if ( gp.ActiveWalkway == this )
		{
			gp.ActiveWalkway = null;
		}
	}

	private void OnTriggerEntered( Collider c )
	{
		var gravPawn = c.GetComponent<GravityPawn>();
		if ( gravPawn != null && gravPawn.IsValidGravTrigger( c ) )
		{
			AddGravPawn( gravPawn );
		}
	}

	private void OnTriggerExit( Collider c )
	{
		var gravPawn = c.GetComponent<GravityPawn>();
		if ( gravPawn != null && gravPawn.IsValidGravTrigger( c ) )
		{
			RemoveGravPawn( gravPawn );
		}
	}
}
