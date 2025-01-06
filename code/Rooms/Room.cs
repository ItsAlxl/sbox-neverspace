namespace Neverspace;

[Group( "Neverspace - Rooms" )]
[Title( "Room" )]
[Icon( "living" )]

public sealed class Room : Component
{
	[Property] Color CamColor { get; set; } = Color.Black;
	[Property] float CamNear { get; set; } = 0.25f;
	[Property] float CamFar { get; set; } = 5000.0f;

	public bool GatewaysActive
	{
		set
		{
			foreach ( var g in GetGateways() )
			{
				if ( !g.Tags.HasAny( "quantum", "unlockable-door" ) )
					g.GameObject.Enabled = value;
			}
		}
	}

	public void ApplyCameraProperties( CameraComponent cam )
	{
		cam.BackgroundColor = CamColor;
		cam.ZNear = CamNear;
		cam.ZFar = CamFar;
	}

	public IEnumerable<Portal> GetPortals()
	{
		return GameObject.Components.GetAll<Portal>( FindMode.EverythingInDescendants );
	}

	public IEnumerable<Gateway> GetGateways()
	{
		return GameObject.Components.GetAll<Gateway>( FindMode.EverythingInDescendants );
	}
}
