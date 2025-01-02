using System;

namespace Neverspace;

[Group( "Neverspace - Portals" )]
[Title( "Portal Traveler" )]
[Icon( "login" )]

public abstract class PortalTraveler : Component
{
	public const float MIN_WORLD_SCALE = 0.1f;

	[Property] public bool IsCameraViewer = false;

	public Transform TravelerTransform { get => WorldTransform; set => WorldTransform = value; }
	public bool IsInPassage { get => passageSource != null; }

	private readonly Dictionary<GameObject, GameObject> goToProxy = new();
	private Gateway passageSource;
	private Gateway passageTarget;
	private int passageSide;
	private bool passageSwapped;

	private Gateway PassageGatewayLegit { get => passageSwapped ? passageTarget : passageSource; }
	private Gateway PassageGatewayProxy { get => passageSwapped ? passageSource : passageTarget; }

	private IEnumerable<ModelRenderer> VisualComponentsLegit { get => GetGoVisualComponents( GameObject ); }
	private IEnumerable<ModelRenderer> VisualComponentsProxy { get => goToProxy.SelectMany( kv => GetGoVisualComponents( kv.Value ) ); }

	public event Action<Portal> OnTeleport;

	public virtual void TeleportThrough( Portal portal )
	{
		TravelerTransform = portal.GetEgressTransform( TravelerTransform );

		var gravPawn = GetComponent<GravityPawn>();
		if ( gravPawn != null )
		{
			gravPawn.ActiveWalkway?.RemoveGravPawn( gravPawn );
			portal.EgressPortal.InstantWalkway?.AddGravPawn( gravPawn );
		}
		Transform.ClearInterpolation();
		OnTeleport?.Invoke( portal );
		if ( WorldScale.z < MIN_WORLD_SCALE )
		{
			WorldScale = new( MIN_WORLD_SCALE, MIN_WORLD_SCALE, MIN_WORLD_SCALE );
		}
	}

	protected Vector3 GetTransformedVector3( Vector3 worldVect, Transform destinationTransform, bool rescale = true )
	{
		return destinationTransform.NormalToWorld( WorldTransform.NormalToLocal( worldVect ) ) * worldVect.Length * (rescale ? (destinationTransform.Scale / WorldScale) : 1.0f);
	}

	public virtual void OnMovement() { }

	static private IEnumerable<ModelRenderer> GetGoVisualComponents( GameObject go )
	{
		return go.Components.GetAll<ModelRenderer>( FindMode.EverythingInSelfAndDescendants );
	}

	public void BeginPassage( Gateway from, Gateway to, int side )
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
		BeginSlice( PassageGatewayLegit.WorldPlane, VisualComponentsLegit, swapSide );
		BeginSlice( PassageGatewayProxy.WorldPlane, VisualComponentsProxy, -swapSide );
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

			m.CreateDuplicate( proxy );
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
				kv.Value.WorldTransform = PassageGatewayLegit.GetPortalTransform( PassageGatewayProxy, kv.Key.WorldTransform );
			}
		}
	}

	protected override void OnPreRender()
	{
		base.OnPreRender();
		DriveProxy();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		DestroyProxy();
	}
}
