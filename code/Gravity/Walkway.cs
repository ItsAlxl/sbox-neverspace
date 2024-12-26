namespace Neverspace;

[Group( "Neverspace - Gravity" )]
[Title( "Walkway" )]
[Icon( "route" )]

public sealed class Walkway : Component
{
	public float GravityStrength { get; set; } = 800.0f;
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
			b.Center = b.Center.WithZ( b.Center.z + 32.0f );
		}
	}

	private void OnTriggerEntered( Collider c )
	{
		var gravPawn = c.GetComponent<GravityPawn>();
		if ( gravPawn != null )
		{
			gravPawn.ActiveWalkway = this;
			Log.Info( $"{GameObject.Name} gravs {c}" );
		}
	}

	private void OnTriggerExit( Collider c )
	{
		var gravPawn = c.GetComponent<GravityPawn>();
		if ( gravPawn != null && gravPawn.ActiveWalkway == this )
		{
			gravPawn.ActiveWalkway = null;
			Log.Info( $"{GameObject.Name} ungravs {c}" );
		}
	}
}
