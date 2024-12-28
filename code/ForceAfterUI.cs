namespace Neverspace;

public class MyGameSystem : GameObjectSystem
{
	public MyGameSystem( Scene scene ) : base( scene )
	{
		Listen( Stage.StartUpdate, -1, ForceAfterUI, "ForceAfterUI" );
	}

	void ForceAfterUI()
	{
		foreach ( var m in Scene.GetAllComponents<ModelRenderer>() )
		{
			// shitty hack to circumvent the s&box bug that makes unlit materials' colors wrong
			m.RenderOptions.AfterUI = true;
		}
	}
}
