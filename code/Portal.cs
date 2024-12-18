using Sandbox;

namespace Neverspace;

[Group( "Neverspace - Portals" )]
[Title( "Portal" )]
[Icon( "join_left" )]

public sealed class Portal : Component
{
	[Property] Portal EgressPortal { get; set; }

	[Property] ModelRenderer ViewScreen { get; set; }
	[Property] CameraComponent GhostCamera { get; set; }
	[Property] CameraComponent PlayerCamera { get; set; }

	private Texture renderTarget;

	protected override void OnAwake()
	{
		ViewScreen ??= GameObject.Components.GetInChildren<ModelRenderer>();
		GhostCamera ??= GameObject.Components.GetInChildren<CameraComponent>();
		PlayerCamera ??= Scene.Camera;
	}

	protected override void OnStart()
	{
		ViewScreen.MaterialOverride = Material.FromShader( "shaders/portal" ).CreateCopy();
	}

	protected override void OnUpdate()
	{
		if ( EgressPortal == null )
		{
			ViewScreen.Enabled = false;
		}
		else
		{
			ViewScreen.Enabled = true;
			if ( renderTarget == null || !renderTarget.Size.AlmostEqual( Screen.Size, 0.5f ) )
			{
				renderTarget?.Dispose();
				renderTarget = Texture.CreateRenderTarget()
					.WithSize( Screen.Size )
					.Create();
				GhostCamera.RenderTarget = renderTarget;
				ViewScreen.MaterialOverride.Set( "viewtex", renderTarget );

			}

			GhostCamera.FieldOfView = PlayerCamera.FieldOfView;
			GhostCamera.Transform.World = EgressPortal.Transform.World.ToWorld( Transform.World.ToLocal( PlayerCamera.Transform.World ) );
		}
	}
}
