namespace Neverspace;

[Group( "Neverspace - Gravity" )]
[Title( "Attractor" )]
[Icon( "play_for_work" )]

public abstract class GravityAttractor : Component
{
	[Property] public float GravityStrength { get; set; } = 800.0f;

	public abstract void AddGravPawn( GravityPawn gp );
	public abstract void RemoveGravPawn( GravityPawn gp );
	public abstract bool HasGravPawn( GravityPawn gp );
	protected abstract Vector3 GetWorldGravity( GravityPawn gp );

	public Vector3 GetGravityOn( GravityPawn gp )
	{
		return HasGravPawn( gp ) ? GetWorldGravity( gp ) : Vector3.Zero;
	}

	protected void OnGravTriggerEntered( Collider c )
	{
		var gravPawn = c.GetComponent<GravityPawn>();
		if ( gravPawn != null && gravPawn.IsValidGravTrigger( c ) && !HasGravPawn( gravPawn ) )
		{
			Log.Info( $"{GameObject} gravs {gravPawn.GameObject}" );
			AddGravPawn( gravPawn );
		}
	}

	protected void OnGravTriggerExit( Collider c )
	{
		var gravPawn = c.GetComponent<GravityPawn>();
		if ( gravPawn != null && gravPawn.IsValidGravTrigger( c ) && HasGravPawn( gravPawn ) )
		{
			Log.Info( $"{GameObject} ungravs {gravPawn.GameObject}" );
			RemoveGravPawn( gravPawn );
		}
	}
}
