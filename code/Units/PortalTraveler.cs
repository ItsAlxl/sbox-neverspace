using System;

namespace Neverspace;

[Group( "Neverspace - Portals" )]
[Title( "Traveler" )]
[Icon( "login" )]

public sealed class PortalTraveler : Component
{
	public Transform TravelerTransform { get => WorldTransform; set => WorldTransform = value; }

	public Action<Action<Transform>, Transform> TeleportHook = null;
	public bool IsCameraViewer = false;

	private Dictionary<GameObject, GameObject> goToProxy = new();

	private Portal sourcePortal;
	private Portal targetPortal;
	private int approachSide;

	private void baseTeleport( Transform destinationTransform )
	{
		TravelerTransform = destinationTransform;
		Transform.ClearInterpolation();
		DriveProxy();
	}

	public void TeleportTo( Transform destinationTransform )
	{
		if ( TeleportHook == null )
		{
			baseTeleport( destinationTransform );
		}
		else
		{
			TeleportHook( baseTeleport, destinationTransform );
		}
	}

	private IEnumerable<ModelRenderer> GetGoVisualComponents( GameObject go )
	{
		return go.Components.GetAll<ModelRenderer>( FindMode.EverythingInSelfAndDescendants );
	}

	private IEnumerable<ModelRenderer> GetVisualComponents()
	{
		return GetGoVisualComponents( GameObject );
	}

	private IEnumerable<ModelRenderer> GetProxyVisualComponents()
	{
		return goToProxy.SelectMany( kv => GetGoVisualComponents( kv.Value ) );
	}

	public void BeginTeleportTransition( Portal from, Portal to, int side )
	{
		sourcePortal = from;
		targetPortal = to;
		approachSide = side;
		CreateProxy();
		BeginSlice( from.GetWorldPlane(), GetVisualComponents(), side );
		BeginSlice( to.GetWorldPlane(), GetProxyVisualComponents(), -side );
	}

	public void EndTeleportTransition( Portal from, Portal to )
	{
		if ( sourcePortal == from && targetPortal == to )
		{
			sourcePortal = null;
			targetPortal = null;
			approachSide = 0;
			DestroyProxy();
			EndSlice( GetVisualComponents() );
		}
	}

	private void BeginSlice( Plane p, IEnumerable<ModelRenderer> models, int side )
	{
		foreach ( var m in models )
		{
			m.SceneObject.Attributes.Set( "ClipOgn", p.Position );
			m.SceneObject.Attributes.Set( "ClipNormal", p.Normal * side );
			m.SceneObject.Attributes.Set( "ClipEnabled", true );
		}
	}

	private void EndSlice( IEnumerable<ModelRenderer> models )
	{
		foreach ( var m in models )
		{
			m.SceneObject.Attributes.Set( "ClipEnabled", false );
		}
	}

	public void CreateProxy()
	{
		if ( goToProxy.Count > 0 )
			return;

		GameObject.SerializeOptions serializeOptions = new()
		{
			Cloning = true
		};

		foreach ( var m in GetVisualComponents() )
		{
			goToProxy.TryGetValue( m.GameObject, out GameObject proxy );

			if ( proxy is null )
			{
				proxy = new GameObject();
				proxy.Tags.Add( "tp-proxy" );
				proxy.Tags.Add( m.Tags );

				proxy.Name = m.GameObject.Name + "_TPPROXY";
				proxy.MakeNameUnique();
				GameObject.AddSibling( proxy, true );

				goToProxy.Add( m.GameObject, proxy );
			}

			var proxyComponent = proxy.Components.Create( TypeLibrary.GetType( m.GetType() ) );
			proxyComponent.DeserializeImmediately( m.Serialize( serializeOptions ).AsObject() );
		}
	}

	private void DestroyProxy()
	{
		foreach ( var p in goToProxy.Values )
		{
			p.Destroy();
		}
		goToProxy.Clear();
	}

	private void DriveProxy()
	{
		if ( sourcePortal != null && targetPortal != null )
		{
			foreach ( var kv in goToProxy )
			{
				kv.Value.WorldTransform = sourcePortal.GetPortalTransform( targetPortal, kv.Key.WorldTransform );
			}
		}
	}

	protected override void OnUpdate()
	{
		DriveProxy();
	}

	protected override void OnDestroy()
	{
		DestroyProxy();
	}
}
