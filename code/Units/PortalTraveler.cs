using System;

namespace Neverspace;

[Group( "Neverspace - Portals" )]
[Title( "Traveler" )]
[Icon( "login" )]

public sealed class PortalTraveler : Component
{
	[Property] public bool IsCameraViewer = false;

	public Action<Transform> TeleportHook = null;
	public Action MovtHook = null;

	public Transform TravelerTransform { get => WorldTransform; set => WorldTransform = value; }

	private readonly Dictionary<GameObject, GameObject> goToProxy = new();
	private Portal passageSource;
	private Portal passageTarget;
	private int passageSide;
	private bool passageSwapped;

	public void BaseTeleport( Transform destinationTransform )
	{
		TravelerTransform = destinationTransform;
		Transform.ClearInterpolation();
	}

	public void TeleportTo( Transform destinationTransform )
	{
		if ( TeleportHook == null )
		{
			BaseTeleport( destinationTransform );
		}
		else
		{
			TeleportHook( destinationTransform );
		}
	}

	public void OnMovement()
	{
		MovtHook?.Invoke();
	}

	static private IEnumerable<ModelRenderer> GetGoVisualComponents( GameObject go )
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

	public void BeginPassage( Portal from, Portal to, int side )
	{
		if ( IsInPassage() )
			return;

		passageSource = from;
		passageTarget = to;
		passageSide = side;
		passageSwapped = false;

		CreateProxy();
		ConfigurePassageSlices();
	}

	public Portal GetRealPassagePortal()
	{
		return passageSwapped ? passageTarget : passageSource;
	}

	public Portal GetProxyPassagePortal()
	{
		return passageSwapped ? passageSource : passageTarget;
	}

	private void ConfigurePassageSlices()
	{
		var swapSide = passageSwapped ? -passageSide : passageSide;
		BeginSlice( GetRealPassagePortal().GetWorldPlane(), GetVisualComponents(), swapSide );
		BeginSlice( GetProxyPassagePortal().GetWorldPlane(), GetProxyVisualComponents(), -swapSide );
	}

	public void SwapPassage()
	{
		passageSwapped = !passageSwapped;
		ConfigurePassageSlices();
	}

	public void EndPassage( Portal from, Portal to )
	{
		if ( !passageSwapped && passageSource == from && passageTarget == to ||
			passageSwapped && passageSource == to && passageTarget == from )
		{
			passageSource = null;
			passageTarget = null;
			passageSide = 0;
			DestroyProxy();
			EndSlice( GetVisualComponents() );
		}
	}

	public bool IsInPassage()
	{
		return passageSource != null;
	}

	private static void BeginSlice( Plane p, IEnumerable<ModelRenderer> models, int side )
	{
		foreach ( var m in models )
		{
			m.SceneObject.Attributes.Set( "ClipOgn", p.Position );
			m.SceneObject.Attributes.Set( "ClipNormal", p.Normal * side );
			m.SceneObject.Attributes.Set( "ClipEnabled", true );
		}
	}

	private static void EndSlice( IEnumerable<ModelRenderer> models )
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
		if ( passageSource != null && passageTarget != null )
		{
			foreach ( var kv in goToProxy )
			{
				kv.Value.WorldTransform = GetRealPassagePortal().GetPortalTransform( GetProxyPassagePortal(), kv.Key.WorldTransform );
			}
		}
	}

	protected override void OnPreRender()
	{
		DriveProxy();
	}

	protected override void OnDestroy()
	{
		DestroyProxy();
	}
}
