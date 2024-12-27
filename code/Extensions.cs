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
		go ??= c.GameObject;
		var dupeComp = go.Components.Create( TypeLibrary.GetType( c.GetType() ) );
		dupeComp.DeserializeImmediately( c.Serialize( duplicateSerializeOptions ).AsObject() );
		return dupeComp;
	}

	public static bool IsInCameraBounds( this ModelRenderer m, CameraComponent c )
	{
		return c.GetFrustum( new Rect( 0, 0, Screen.Width, Screen.Height ) ).IsInside( m.Bounds, true );
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