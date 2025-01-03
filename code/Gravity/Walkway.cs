namespace Neverspace;

[Group( "Neverspace - Gravity" )]
[Title( "Walkway" )]
[Icon( "route" )]

public sealed class Walkway : GravityAttractor
{
	[Property] bool GravOnly { get; set; } = false;
	public Vector3 Gravity { get => GravityStrength * WorldTransform.Down; }

	protected override void OnStart()
	{
		base.OnStart();
		var col = GetComponent<Collider>();
		var trigger = GravOnly ? col : col.CreateDuplicate() as Collider;
		trigger.IsTrigger = true;
		trigger.OnTriggerEnter += OnGravTriggerEntered;
		trigger.OnTriggerExit += OnGravTriggerExit;
		if ( trigger is BoxCollider b )
		{
			var downscale = b.Scale.z * 0.5f;
			b.Scale = new( b.Scale.x - downscale, b.Scale.y - downscale, b.Scale.z );
			b.Center = b.Center.WithZ( b.Center.z + b.Scale.z );
		}
		Tags.Add( "walkway" );
	}

	protected override void ForceAddGravPawn( GravityPawn gp )
	{
		gp.ActiveWalkway = this;
	}

	protected override void ForceRemoveGravPawn( GravityPawn gp )
	{
		gp.ActiveWalkway = null;
	}

	public override bool HasGravPawn( GravityPawn gp )
	{
		return gp.ActiveWalkway == this;
	}

	protected override Vector3 GetWorldGravity( GravityPawn _ )
	{
		return Gravity;
	}
}
