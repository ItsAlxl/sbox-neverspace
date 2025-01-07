namespace Neverspace;

[Group( "Neverspace - Rooms" )]
[Title( "Room Controller" )]
[Icon( "maps_home_work" )]

public sealed class RoomController : Component
{
	[Property] Room ActiveRoom { get; set; }
	[Property] CameraComponent PlayerCamera { get; set; }
	[Property] PortalTravelerPlayer PlayerTraveler { get; set; }

	private readonly Dictionary<Portal, Room> portalToEgressRoom = new();
	private List<Room> allRooms;

	protected override void OnStart()
	{
		base.OnStart();
		PlayerCamera ??= Scene.Camera;
		PlayerTraveler ??= Scene.GetAllComponents<PortalTravelerPlayer>().ElementAt( 0 );

		allRooms = new( Scene.GetAllComponents<Room>() );
		Dictionary<Portal, Room> portalToRoom = new();
		foreach ( var room in allRooms )
		{
			var roomPortals = room.GetPortals();
			foreach ( var portal in roomPortals )
			{
				portalToRoom.Add( portal, room );
			}
		}

		foreach ( var kv in portalToRoom )
		{
			var egress = kv.Key.EgressPortal;
			if ( egress != null && portalToRoom[egress] != kv.Value )
			{
				portalToEgressRoom[kv.Key] = portalToRoom[egress];
			}
		}

		PlayerTraveler.OnTeleport += OnPlayerTeleport;
		ActivateRoom( ActiveRoom );
		PlayerTraveler.WorldPosition = ActiveRoom.WorldPosition.WithZ(ActiveRoom.WorldPosition.z + 32.0f);
		PlayerTraveler.WorldScale = ActiveRoom.WorldScale;
	}

	private void ActivateRoom( Room r )
	{
		var activate = new HashSet<Room> { r };
		foreach ( var gate in r.GetGateways() )
		{
			if ( portalToEgressRoom.ContainsKey( gate ) )
			{
				activate.Add( portalToEgressRoom[gate] );
			}
		}

		foreach ( var room in allRooms )
		{
			room.GatewaysActive = activate.Contains( room );
		}
		r.ApplyCameraProperties( PlayerCamera );
		ActiveRoom = r;
	}

	private void OnPlayerTeleport( Portal ingress )
	{
		portalToEgressRoom.TryGetValue( ingress, out Room r );
		if ( r != null )
			ActivateRoom( r );
	}
}
