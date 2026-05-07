using Sandbox;

public sealed class PlayerPropGrab : Component
{
	[Property] public float GrabDistance { get; set; } = 100f;
	[Property] public float ThrowForce { get; set; } = 800f;

	private GameObject _heldObject;
	private Rigidbody _heldBody;

	protected override void OnUpdate()
	{
		if ( Input.Pressed( "Attack1" ))
		{
			
			PropGrab();
		}

		if ( _heldObject.IsValid() )
		{
			UpdateHeldPosition();
		}
	}

	void PropGrab()
	{
		if ( _heldObject.IsValid() )
		{
			ThrowProp();
			return;
		}

		var ray = Scene.Camera.ScreenNormalToRay( 0.5f );
		var tr = Scene.Trace.Ray( ray, GrabDistance )
			.IgnoreGameObjectHierarchy( GameObject )
			.WithTag( "prop" )
			.Run();
		
		if ( tr.Hit && tr.GameObject.Components.TryGet<Rigidbody>( out var rb, FindMode.EverythingInSelfAndAncestors ) )
		{
			_heldObject = rb.GameObject;
			_heldBody = rb;
			
			// Disable motion
			_heldBody.PhysicsBody.MotionEnabled = false;
			Log.Info( $"Grabbed: ${_heldObject.Name}" );
		}
	}

	void UpdateHeldPosition()
	{
		var targetPos = Scene.Camera.WorldPosition + (Scene.Camera.WorldRotation.Forward * 80f);
		
		_heldObject.WorldPosition = Vector3.Lerp( _heldObject.WorldPosition, targetPos, Time.Delta * 25f );
		_heldObject.WorldRotation = Rotation.Lerp( _heldObject.WorldRotation, Scene.Camera.WorldRotation, Time.Delta * 10f  );	
	}

	void ThrowProp()
	{
		_heldBody.PhysicsBody.MotionEnabled = true;

		var forceVector = Scene.Camera.WorldRotation.Forward * ThrowForce * _heldBody.PhysicsBody.Mass;
		_heldBody.PhysicsBody.ApplyImpulse( forceVector );
		
		Log.Info( $"Threw: ${_heldObject.Name}" );

		_heldBody = null;
		_heldObject = null;
	}
}
