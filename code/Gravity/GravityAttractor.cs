namespace Neverspace;

[Group( "Neverspace - Gravity" )]
[Title( "Attractor" )]
[Icon( "play_for_work" )]

public abstract class GravityAttractor : Component
{
	[Property] public float GravityStrength { get; set; } = 800.0f;

	protected abstract Vector3 GetWorldGravity( GravityPawn gp );

	public bool AffectsPawn( GravityPawn gp )
	{
		return gp.IsAffectedBy( this );
	}

	public Vector3 GetGravityOn( GravityPawn gp )
	{
		return AffectsPawn( gp ) ? GetWorldGravity( gp ) : Vector3.Zero;
	}

	public virtual bool CanAddGravPawn( GravityPawn gp )
	{
		return !AffectsPawn( gp );
	}

	public void AddGravPawn( GravityPawn gp )
	{
		if ( CanAddGravPawn( gp ) )
			gp.AddGrav( this );
	}

	public virtual bool CanRemoveGravPawn( GravityPawn gp )
	{
		return AffectsPawn( gp );
	}

	public void RemoveGravPawn( GravityPawn gp )
	{
		if ( CanRemoveGravPawn( gp ) )
			gp.RemoveGrav( this );
	}

	protected void OnGravTriggerEntered( Collider c )
	{
		var gravPawn = c.GetComponent<GravityPawn>();
		if ( gravPawn != null && gravPawn.IsValidGravTrigger( c ) )
		{
			AddGravPawn( gravPawn );
		}
	}

	protected void OnGravTriggerExit( Collider c )
	{
		var gravPawn = c.GetComponent<GravityPawn>();
		if ( gravPawn != null && gravPawn.IsValidGravTrigger( c ) )
		{
			RemoveGravPawn( gravPawn );
		}
	}
}
