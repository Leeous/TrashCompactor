using System;

public sealed partial class PlayerData : Component
{
    [Property] public Guid PlayerId { get; set; }
    [Property] public long SteamId { get; set; }
    [Property] public string DisplayName { get; set; }

    [Sync] public int Kills { get; set; }
    [Sync] public int Deaths { get; set; }

    public Connection Connection => Connection.Find( PlayerId );

    /// <summary>
    /// Is this me (local player)?  
    /// </summary>
    public bool IsMe => PlayerId == Connection.Local.Id;

    public float Ping => Connection?.Ping ?? 0;

    /// <summary>
    /// Data for all players  
    /// </summary>
    public static IEnumerable<PlayerData> All => Game.ActiveScene.GetAll<PlayerData>();

    public static PlayerData For( Connection connection ) => connection == null ? default : For( connection.Id );

    /// <summary>
    /// Get player data for a player's Id
    /// </summary>
    /// <param name="playerId"></param>
    /// <returns></returns>
    public static PlayerData For(Guid playerId )
    {
        return All.FirstOrDefault( x => x.PlayerId == playerId );
    } 

    // Host-side respawn tracking. No sync required.
    private bool _needsRespawn;
    private RealTimeSince _timeSinceDied;

	protected override void OnUpdate()
	{
		if ( !Networking.IsHost ) return;
        if ( !_needsRespawn ) return;
        if ( _timeSinceDied < 4f ) return;

        //RequestRespawn(); 
	}

    [Rpc.Broadcast]
	private void RpcAddStat( string identifier, int amount = 1 )
	{
		Sandbox.Services.Stats.Increment( identifier, amount );
	}

    	/// <summary>
	/// Called on the host, calls a RPC on the player and adds a stat
	/// </summary>
	/// <param name="identifier"></param>
	/// <param name="amount"></param>
	public void AddStat( string identifier, int amount = 1 )
	{
		if ( Application.CheatsEnabled ) return;

		Assert.True( Networking.IsHost, "PlayerData.AddStat is host-only!" );

		using ( Rpc.FilterInclude( Connection ) )
		{
			RpcAddStat( identifier, amount );
		}
	}

    /// <summary>
	/// Called on the host when the player dies. Starts the respawn countdown so that
	/// PlayerData can trigger a respawn if the PlayerObserver is destroyed (e.g. by cleanup)
	/// before it fires.
	/// </summary>
	public void MarkForRespawn()
	{
		_needsRespawn = true;
		_timeSinceDied = 0;
	}
}