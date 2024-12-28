namespace Neverspace;

[Group( "Neverspace - Gravity" )]
[Title( "Attractor" )]
[Icon( "play_for_work" )]

public abstract class GravityAttractor : Component
{
	[Property] public float GravityStrength { get; set; } = 800.0f;

	protected abstract void ForceAddGravPawn( GravityPawn gp );
	protected abstract void ForceRemoveGravPawn( GravityPawn gp );
	public abstract bool HasGravPawn( GravityPawn gp );
	protected abstract Vector3 GetWorldGravity( GravityPawn gp );

	public Vector3 GetGravityOn( GravityPawn gp )
	{
		return HasGravPawn( gp ) ? GetWorldGravity( gp ) : Vector3.Zero;
	}

	public virtual bool CanAddGravPawn( GravityPawn gp )
	{
		return !HasGravPawn( gp );
	}

	public void AddGravPawn( GravityPawn gp )
	{
		if ( CanAddGravPawn( gp ) )
			ForceAddGravPawn( gp );
	}


	public virtual bool CanRemoveGravPawn( GravityPawn gp )
	{
		return HasGravPawn( gp );
	}

	public void RemoveGravPawn( GravityPawn gp )
	{
		if ( CanRemoveGravPawn( gp ) )
			ForceRemoveGravPawn( gp );
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
