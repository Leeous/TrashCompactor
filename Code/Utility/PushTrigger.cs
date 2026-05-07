using Sandbox;
using System.Collections.Generic;

namespace Sandbox;

public sealed class PushTrigger : Component, Component.ITriggerListener
{
	[Property] public Vector3 PushDirection { get; set; } = Vector3.Forward;
	[Property] public float Force { get; set; } = 1500f;

	// Track everything currently inside the trigger
	private List<GameObject> _activeObjects = new();

	protected override void OnFixedUpdate()
	{
		// Calculate the direction once per frame
		var worldDir = WorldRotation * PushDirection.Normal;

		// Clean up the list in case any objects were deleted/destroyed while inside
		_activeObjects.RemoveAll( x => !x.IsValid );

		foreach ( var target in _activeObjects )
		{
			// 1. Handle Props (looks in self and parents for Rigidbody)
			if ( target.Components.TryGet<Rigidbody>( out var rb, FindMode.EverythingInSelfAndAncestors ) )
			{
				rb.PhysicsBody.Velocity = worldDir * Force;
				rb.PhysicsBody.Sleeping = false; // Keep it awake
			}

			// 2. Handle Players
			if ( target.Components.TryGet<CharacterController>( out var controller, FindMode.EverythingInSelfAndAncestors ) )
			{
				controller.Velocity += worldDir * Force * Time.Delta;
			}
		}
	}

	public void OnTriggerEnter( Collider other )
	{
		if ( !_activeObjects.Contains( other.GameObject ) )
		{
			_activeObjects.Add( other.GameObject );
			// Log.Info( $"Compactor grabbed: {other.GameObject.Name}" );
		}
	}

	public void OnTriggerExit( Collider other )
	{
		if ( _activeObjects.Contains( other.GameObject ) )
		{
			_activeObjects.Remove( other.GameObject );
			// Log.Info( $"Compactor released: {other.GameObject.Name}" );
		}
	}
}
