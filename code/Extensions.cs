using System;

namespace Neverspace;

public static class Extensions
{
	private static readonly GameObject.SerializeOptions duplicateSerializeOptions = new()
	{
		Cloning = true
	};

	public static Component CreateDuplicate( this Component c, GameObject go = null )
	{
		var dupeComp = (go ?? c.GameObject).Components.Create( TypeLibrary.GetType( c.GetType() ) );
		var serial = c.Serialize( duplicateSerializeOptions ).AsObject();
		serial["__guid"] = Guid.NewGuid();
		dupeComp.DeserializeImmediately( serial );
		return dupeComp;
	}

	public static BBox AggregateGoChildBounds( this List<GameObject> gos )
	{
		Queue<GameObject> scanQueue = new();
		foreach ( var g in gos )
			scanQueue.Enqueue( g );

		var bounds = new BBox();
		while ( scanQueue.Count > 0 )
		{
			var g = scanQueue.Dequeue();
			bounds = bounds.Size.IsNearlyZero( 0.001f ) ? g.GetBounds() : bounds.AddBBox( g.GetBounds() );
			foreach ( var c in g.Children )
				scanQueue.Enqueue( c );
		}
		return bounds;
	}

	public static BBox BBoxToLocal( this Transform t, BBox b )
	{
		return new( t.PointToLocal( b.Mins ), t.PointToLocal( b.Maxs ) );
	}

	public static BBox BBoxToWorld( this Transform t, BBox b )
	{
		return new( t.PointToWorld( b.Mins ), t.PointToWorld( b.Maxs ) );
	}

	public static float MinValue( this Vector3 v )
	{
		return MathF.Min( MathF.Min( v.x, v.y ), v.z );
	}

	public static float MaxValue( this Vector3 v )
	{
		return MathF.Max( MathF.Max( v.x, v.y ), v.z );
	}

	// slabs; im not gonna pretend to understand this
	public static bool RayIntersects( this BBox b, Ray r )
	{
		var invR = Vector3.One / r.Forward;
		var t0 = (b.Mins - r.Position) * invR;
		var t1 = (b.Maxs - r.Position) * invR;
		return Vector3.Min( t0, t1 ).MaxValue() <= Vector3.Max( t0, t1 ).MinValue();
	}

	public static float DistanceSquaredToPoint( this BBox bounds, Vector3 p )
	{
		return bounds.ClosestPoint( p ).DistanceSquared( p );
	}

	public static Frustum GetScreenFrustum( this CameraComponent c )
	{
		return c.GetFrustum( new Rect( 0, 0, Screen.Width, Screen.Height ) );
	}

	public static bool IsInCameraBounds( this BBox bounds, CameraComponent c )
	{
		return c.GetScreenFrustum().IsInside( bounds, true );
	}

	public static bool IsInCameraBounds( this ModelRenderer m, CameraComponent c )
	{
		return m.Bounds.IsInCameraBounds( c );
	}

	public static T GetGoComponent<T>( this SceneTraceResult tr ) where T : class
	{
		return tr.GameObject?.Components.Get<T>();
	}

	public static int DotSign( this Vector3 a, Vector3 b )
	{
		return Math.Sign( a.Dot( b ) );
	}

	private static bool RotationComponentsAlmostEqual( Rotation a, Rotation b, float delta = 0.001f )
	{
		return a.x.AlmostEqual( b.x, delta ) && a.y.AlmostEqual( b.y, delta ) && a.z.AlmostEqual( b.z, delta ) && a.w.AlmostEqual( b.w, delta );
	}

	public static bool AlmostEqual( this Rotation a, Rotation b, float delta = 0.001f )
	{
		return RotationComponentsAlmostEqual( a, b, delta ) || RotationComponentsAlmostEqual( a, b, delta );
	}

	public static bool AlmostEqual( this Transform a, Transform b, float delta = 0.001f )
	{
		return a.Position.AlmostEqual( b.Position, delta ) && a.Scale.AlmostEqual( b.Scale, delta ) && a.Rotation.AlmostEqual( b.Rotation, delta );
	}
}