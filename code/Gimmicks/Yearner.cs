namespace Neverspace;

[Group( "Neverspace - Gimmicks" )]
[Title( "Yearner" )]
[Icon( "shopping_cart" )]

public sealed class Yearner : Component, Component.ICollisionListener
{
	[Property] GameObject DesiredGo { get; set; }
	[Property] ModelRenderer AffectModel { get; set; }

	[Property] Material HappyMaterial { get; set; }
	[Property] Color HappyTint { get; set; } = Color.White;
	[Property] float HappyDist { get; set; } = 32.0f;

	[Property] GameObject EnableOnRetrieval { get; set; }
	[Property] GameObject DisableOnRetrieval { get; set; }

	[Property] Rigidbody Projectile { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
		AffectModel ??= GetComponent<ModelRenderer>();
		HappyDist *= WorldScale.x;
		HappyDist *= HappyDist;
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
		if ( Vector3.DistanceBetweenSquared( WorldPosition, DesiredGo.WorldPosition ) <= HappyDist )
		{
			AffectModel.MaterialOverride = HappyMaterial;
			AffectModel.Tint = HappyTint;

			if ( EnableOnRetrieval != null )
				EnableOnRetrieval.Enabled = true;
			if ( DisableOnRetrieval != null )
				DisableOnRetrieval.Enabled = false;

			if ( Projectile != null )
			{
				var shootDirection = (Projectile.WorldPosition - WorldPosition).Normal;
				Projectile.ApplyImpulse( shootDirection * Projectile.Mass * 1500.0f );
				Projectile.AngularVelocity = Vector3.One;
			}

			Enabled = false;
		}
	}
}
