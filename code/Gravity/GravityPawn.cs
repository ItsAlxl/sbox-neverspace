namespace Neverspace;

[Group( "Neverspace - Gravity" )]
[Title( "Gravity Pawn" )]
[Icon( "fitness_center" )]

public class GravityPawn : Component
{
	[Property] Vector3 BaseGravity { get; set; } = new( 0.0f, 0.0f, -800.0f );

	public Walkway ActiveWalkway { get; set; }
	public List<Planetoid> Planetoids { get; set; } = new();
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

	public virtual bool IsValidGravTrigger( Collider c )
	{
		return !c.Tags.Has( "grav-ignore" );
	}
}
