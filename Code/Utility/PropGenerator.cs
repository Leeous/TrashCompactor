using System;
using System.Collections.Generic;
using System.Timers;
using Sandbox;

public sealed class PropGenerator : Component
{
	[Property] public List<Model> TrashPrefabs { get; set; } = new();
	[Property] public float SpawnInterval { get; set; } = 2.0f;
	[Property] public float PropImpactDamageSpeed { get; set; } = 100;

	private TimeSince _lastSpawn;
	
	protected override void OnFixedUpdate()
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
		
		var newTrashGameObject = new GameObject();
		
		// Add Prop component
		newTrashGameObject.AddComponent<Prop>();
		
		// Set prop position to component's position
		newTrashGameObject.GetComponent<Prop>().LocalPosition = this.LocalPosition;
		
		// Set model on Prop Component
		newTrashGameObject.GetComponent<Prop>().Model = prefab;
		
		// Decrease min speed needed for impact damage
		newTrashGameObject.GetComponent<Rigidbody>().MinImpactDamageSpeed = PropImpactDamageSpeed;
		
		// Add Prop tag (so PlayerPropGrab will interact with it)
		newTrashGameObject.Tags.Add(  "prop" );
		
		if ( newTrashGameObject.Components.TryGet<Rigidbody>( out var rb ) )
		{
			// rb.Velocity = Vector3.Random * 50f; We don't need this right now
		}
	}
}
