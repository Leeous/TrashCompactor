using System.Collections.Generic;
using System.Timers;
using Sandbox;

public sealed class PropGenerator : Component
{
	[Property] public List<GameObject> TrashPrefabs { get; set; } = new();
	[Property] public float SpawnInterval { get; set; } = 2.0f;

	private TimeSince _lastSpawn;
	
	protected override void OnUpdate()
	{
		if ( _lastSpawn > SpawnInterval )
		{
			SpawnRandomTrash();
			_lastSpawn = 0;
		}
	}

	void SpawnRandomTrash()
	{
		if ( TrashPrefabs.Count == 0 ) return;
		
		Log.Info( "Spawning trash..." );
		
		var randomIndex = Game.Random.Int( 0, TrashPrefabs.Count - 1 );
		var prefab = TrashPrefabs[randomIndex];

		if ( prefab != null )
		{
			var newTrash = prefab.Clone( WorldPosition, WorldRotation );
			if ( newTrash.Components.TryGet <Rigidbody>( out var rb ) )
			{
				rb.Velocity = Vector3.Random * 50f;
			}
		}
	}
}
