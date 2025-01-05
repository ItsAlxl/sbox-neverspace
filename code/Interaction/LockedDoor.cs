namespace Neverspace;

[Group( "Neverspace - Interaction" )]
[Title( "Locked Door" )]
[Icon( "balcony" )]

public sealed class LockedDoor : Component
{
	const float PROXIMITY_THRESHOLD = 4096.0f;

	[Property] List<Keyhole> Keyholes { get; set; } = new();
	[Property] GameObject LockedGo { get; set; }
	[Property] GameObject UnlockedGo { get; set; }

	private bool HasAllKeys { get => Keyholes.All( ( k ) => k.HasKey ); }
	private bool IsUnlocked { get; set; } = false;

	protected override void OnAwake()
	{
		base.OnAwake();
		foreach ( var k in Keyholes )
		{
			k.OnKeyInserted += OnKeyChanged;
		}
		OnKeyChanged();
	}

	private void OnKeyChanged()
	{
		var unlock = HasAllKeys;
		if ( IsUnlocked != unlock )
		{
			if ( UnlockedGo != null )
				UnlockedGo.Enabled = unlock;
			if ( LockedGo != null )
				LockedGo.Enabled = !unlock;
			IsUnlocked = unlock;
		}
	}
}
