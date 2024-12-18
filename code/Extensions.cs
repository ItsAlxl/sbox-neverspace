namespace Neverspace;

public static class Extensions
{
	public static bool IsInCameraBounds( this ModelRenderer m, CameraComponent c )
	{
		return c.GetFrustum(new Rect(0, 0, Screen.Width, Screen.Height)).IsInside(m.Bounds, true);
	}
}