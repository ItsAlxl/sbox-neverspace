namespace Neverspace;

public static class Extensions
{
	public static bool IsInCameraBounds( this ModelRenderer m, CameraComponent c )
	{
		return c.GetFrustum( new Rect( 0, 0, Screen.Width, Screen.Height ) ).IsInside( m.Bounds, true );
	}

	public static T GetFirstGoComponent<T>( this SceneTraceResult tr ) where T : class
	{
		return tr.GameObject.Components.FirstOrDefault( c => c is T ) as T;
	}
}