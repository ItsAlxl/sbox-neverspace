namespace Neverspace;

[Group( "Neverspace - Gravity" )]
[Title( "Planetoid" )]
[Icon( "public" )]

public sealed class Planetoid : GravityAttractor
{
	[Property] public float InfluenceStart { get; set; } = 32.0f;
	[Property] public float InfluenceEnd { get; set; } = 128.0f;
	[Property] public Curve InfluenceCurve { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
		var col = GetComponent<Collider>();
		var trigger = col.CreateDuplicate() as Collider;
		trigger.IsTrigger = true;
		trigger.Static = false;
		trigger.OnTriggerEnter += OnGravTriggerEntered;
		trigger.OnTriggerExit += OnGravTriggerExit;
		if ( trigger is SphereCollider s )
		{
			s.Radius = InfluenceEnd;
			InfluenceEnd *= WorldScale.x;
			InfluenceStart *= WorldScale.x;
		}
		Tags.Add( "planetoid" );
	}

	protected override Vector3 GetWorldGravity( GravityPawn gp )
	{
		var pawnToPlanet = WorldPosition - gp.WorldPosition;
		return pawnToPlanet.Normal * GravityStrength * InfluenceCurve.Evaluate( (pawnToPlanet.Length - InfluenceStart) / (InfluenceEnd - InfluenceStart) );
	}

	protected override void DrawGizmos()
	{
		base.DrawGizmos();
		Gizmo.Draw.Color = Color.Blue;
		Gizmo.Draw.LineSphere( Vector3.Zero, InfluenceEnd, 16 );
	}
}
