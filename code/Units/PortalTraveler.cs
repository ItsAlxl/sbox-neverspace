using System;

namespace Neverspace;

[Group( "Neverspace - Units" )]
[Title( "Portal Traveler - Base" )]
[Icon( "login" )]

public class PortalTraveler : Component
{
	[Property] public bool IsCameraViewer = false;

	public Transform TravelerTransform { get => WorldTransform; set => WorldTransform = value; }
	public bool IsInPassage { get => passageSource != null; }

	private readonly Dictionary<GameObject, GameObject> goToProxy = new();
	private Portal passageSource;
	private Portal passageTarget;
	private int passageSide;
	private bool passageSwapped;

	private Portal PassagePortalLegit { get => passageSwapped ? passageTarget : passageSource; }
	private Portal PassagePortalProxy { get => passageSwapped ? passageSource : passageTarget; }

	private IEnumerable<ModelRenderer> VisualComponentsLegit { get => GetGoVisualComponents( GameObject ); }
	private IEnumerable<ModelRenderer> VisualComponentsProxy { get => goToProxy.SelectMany( kv => GetGoVisualComponents( kv.Value ) ); }

	protected override void OnAwake()
	{
		base.OnAwake();
		foreach ( var m in VisualComponentsLegit )
		{
			if ( m.MaterialOverride.ResourceName == "neverspace-generic" )
			{
				m.MaterialOverride = m.MaterialOverride.CreateCopy();
			}
		}
	}

	public virtual void TeleportTo( Transform destinationTransform )
	{
		TravelerTransform = destinationTransform;
		Transform.ClearInterpolation();
	}

	public virtual void OnMovement() { }

	static private IEnumerable<ModelRenderer> GetGoVisualComponents( GameObject go )
	{
		return go.Components.GetAll<ModelRenderer>( FindMode.EverythingInSelfAndDescendants );
	}

	public void BeginPassage( Portal from, Portal to, int side )
	{
		if ( IsInPassage )
			return;

		passageSource = from;
		passageTarget = to;
		passageSide = side;
		passageSwapped = false;

		CreateProxy();
		ConfigurePassageSlices();
	}

	private void ConfigurePassageSlices()
	{
		var swapSide = passageSwapped ? -passageSide : passageSide;
		BeginSlice( PassagePortalLegit.WorldPlane, VisualComponentsLegit, swapSide );
		BeginSlice( PassagePortalProxy.WorldPlane, VisualComponentsProxy, -swapSide );
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
			EndSlice( VisualComponentsLegit );
		}
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

		foreach ( var m in VisualComponentsLegit )
		{
			goToProxy.TryGetValue( m.GameObject, out GameObject proxy );

			if ( proxy is null )
			{
				proxy = new GameObject();
				proxy.Tags.Add( "tp-proxy" );
				proxy.Tags.Add( m.Tags );
				proxy.Tags.Remove( "player" );

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
				kv.Value.WorldTransform = PassagePortalLegit.GetPortalTransform( PassagePortalProxy, kv.Key.WorldTransform );
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
