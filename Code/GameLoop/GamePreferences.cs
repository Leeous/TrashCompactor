/// <summary>
/// The local user's preferences in Trash Compactor
/// </summary>
public static class GamePreferences
{
	/// <summary>
	/// Enables automatic switching to better weapons on item pickup
	/// </summary>
	[ConVar( "sb.autoswitch", ConVarFlags.UserInfo | ConVarFlags.Saved )]
	public static bool AutoSwitch { get; set; } = true;
}