using System;

namespace Neverspace;

public static class Extensions
{
	public static bool IsInCameraBounds( this ModelRenderer m, CameraComponent c )
	{
		return c.GetFrustum( new Rect( 0, 0, Screen.Width, Screen.Height ) ).IsInside( m.Bounds, true );
	}

	public static T GetFirstComponent<T>( this GameObject go ) where T : class
	{
		return go.Components.FirstOrDefault( c => c is T ) as T;
	}

	public static T GetFirstGoComponent<T>( this SceneTraceResult tr ) where T : class
	{
		return tr.GameObject?.GetFirstComponent<T>();
	}

	public static int DotSign( this Vector3 a, Vector3 b )
	{
		return Math.Sign( a.Dot( b ) );
	}
}