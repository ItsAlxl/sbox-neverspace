namespace Neverspace;

[Group( "Neverspace - Gravity" )]
[Title( "Gravity Pawn" )]
[Icon( "fitness_center" )]

public abstract class GravityPawn : Component
{
	[Property] Vector3 BaseGravity { get; set; } = new( 0.0f, 0.0f, -800.0f );

	public Walkway ActiveWalkway { get => Walkways.Count == 0 ? null : Walkways.Last(); }
	protected List<Walkway> Walkways { get; set; } = new();
	protected List<Planetoid> Planetoids { get; set; } = new();

	public Vector3 CurrentGravity
	{
		get =>
			ActiveWalkway == null ?
				(
					Planetoids.Count == 0 ? BaseGravity : PlanetoidGravity
				) :
				ActiveWalkway.Gravity;
	}

	public Vector3 PlanetoidGravity
	{
		get
		{
			var g = Vector3.Zero;
			foreach ( var p in Planetoids )
				g += p.GetGravityOn( this );
			return g;
		}
	}

	public bool IsGravAffected { get => ActiveWalkway != null || Planetoids.Count > 0; }

	public virtual bool IsValidGravTrigger( Collider c )
	{
		return !c.Tags.Has( "grav-ignore" );
	}

	public void Clear()
	{
		var walkways = new List<Walkway>( Walkways );
		foreach ( var w in walkways )
			w.RemoveGravPawn( this );

		var planetoids = new List<Planetoid>( Planetoids );
		foreach ( var p in planetoids )
			p.RemoveGravPawn( this );
	}

	protected virtual void AddWalkway( Walkway w )
	{
		var idx = Walkways.IndexOf( w );
		if ( idx != -1 )
			Walkways.RemoveAt( idx );
		Walkways.Add( w );
	}

	protected virtual void RemoveWalkway( Walkway w )
	{
		Walkways.Remove( w );
	}

	protected virtual void AddPlanetoid( Planetoid p )
	{
		Planetoids.Add( p );
	}

	protected virtual void RemovePlanetoid( Planetoid p )
	{
		Planetoids.Remove( p );
	}

	public void AddGrav( GravityAttractor grav )
	{
		if ( grav is Planetoid p )
			AddPlanetoid( p );
		if ( grav is Walkway w )
			AddWalkway( w );
	}

	public void RemoveGrav( GravityAttractor grav )
	{
		if ( grav is Planetoid p )
			RemovePlanetoid( p );
		if ( grav is Walkway w )
			RemoveWalkway( w );
	}

	public bool IsAffectedBy( GravityAttractor grav )
	{
		return (grav is Planetoid && Planetoids.Contains( grav )) ||
			(grav is Walkway && Walkways.Contains( grav ));
	}
}
