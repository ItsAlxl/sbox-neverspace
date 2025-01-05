namespace Neverspace;

[Group( "Neverspace - Gravity" )]
[Title( "Walkway" )]
[Icon( "route" )]

public sealed class Walkway : GravityAttractor
{
	[Property] bool GravOnly { get; set; } = false;

	[Property] Walkway Twin { get; set; }
	[Property] Gateway TwinEgress { get; set; }

	public Vector3 Gravity { get => GravityStrength * WorldTransform.Down; }

	protected override void OnAwake()
	{
		base.OnAwake();
		var col = GetComponent<Collider>();
		var trigger = GravOnly ? col : col.CreateDuplicate() as Collider;
		trigger.IsTrigger = true;
		trigger.OnTriggerEnter += OnGravTriggerEntered;
		trigger.OnTriggerExit += OnGravTriggerExit;
		if ( trigger is BoxCollider b )
		{
			var downscale = b.Scale.z * 0.75f;
			b.Scale = new( b.Scale.x - downscale / LocalScale.x, b.Scale.y - downscale / LocalScale.y, b.Scale.z );
			b.Center = b.Center.WithZ( b.Center.z + b.Scale.z );
		}
		Tags.Add( "walkway" );
	}

	protected override Vector3 GetWorldGravity( GravityPawn _ )
	{
		return Gravity;
	}

	public void AffectAfterTeleport( GravityPawn p, Gateway g )
	{
		if ( TwinEgress == g )
			Twin?.AddGravPawn( p );
	}
}
