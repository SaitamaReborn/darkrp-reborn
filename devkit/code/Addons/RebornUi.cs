using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sandbox;

namespace DarkRPReborn.Addons;

/// <summary>
/// Addon UI framework (contract v3): server-driven menus and HUD cards. The
/// addon DESCRIBES the interface; the shared renderer draws it on the target
/// player's screen with the game's design by default (navy panel, violet
/// accent) - override Accent/Background per menu to restyle. Button clicks
/// round-trip to host-side handlers. PUBLIC contract file, mirrored verbatim
/// in the community Dev Kit (where menus render for real over the simulator).
/// </summary>
public enum RebornButtonStyle
{
	Primary,
	Danger,
	Ghost,
}

/// <summary>Serialized widget (internal wire format - do not build directly,
/// use the RebornMenu fluent API).</summary>
public sealed class RebornUiWidget
{
	public string T { get; set; } = "";       // header | text | kv | divider | table | input | button
	public string A { get; set; } = "";       // main text / label
	public string B { get; set; } = "";       // secondary text / value / placeholder
	public string Id { get; set; } = "";      // button/input id
	public string Style { get; set; } = "";   // button style
	public List<string> Cols { get; set; }
	public List<List<string>> Rows { get; set; }
}

/// <summary>Wire format of a menu.</summary>
public sealed class RebornUiMenuDto
{
	public string Id { get; set; }
	public string Title { get; set; } = "";
	public string Icon { get; set; } = "widgets";
	public string Accent { get; set; } = "";
	public string Background { get; set; } = "";
	public int Width { get; set; } = 620;
	public List<RebornUiWidget> Widgets { get; set; } = new();
}

/// <summary>Wire format of a HUD card.</summary>
public sealed class RebornUiHudDto
{
	public string Id { get; set; }
	public string Title { get; set; } = "";
	public string Accent { get; set; } = "";
	public List<string> Lines { get; set; } = new();
}

/// <summary>
/// A window shown to one player. Defaults follow the game's design; set
/// Accent ("#8a38f5") or Background to restyle. Build with the fluent API:
/// new RebornMenu("Bourse").Header("Cours").KeyValue("ACME","12$")
///   .Input("qty","Quantité","10").Button("buy","Acheter", (p,inputs) => ...)
/// then Reborn.ShowMenu(player, menu).
/// </summary>
public sealed class RebornMenu
{
	public string Id { get; }
	public string Title { get; set; }
	/// <summary>Material icon name shown in the title bar.</summary>
	public string Icon { get; set; } = "widgets";
	/// <summary>Hex accent override ("" = game default violet).</summary>
	public string Accent { get; set; } = "";
	/// <summary>Hex background override ("" = game default navy).</summary>
	public string Background { get; set; } = "";
	public int Width { get; set; } = 620;

	internal readonly List<RebornUiWidget> Widgets = new();
	internal readonly Dictionary<string, Action<IRebornPlayer, Dictionary<string, string>>> Handlers = new( StringComparer.Ordinal );

	public RebornMenu( string title )
	{
		Id = Guid.NewGuid().ToString( "N" );
		Title = title ?? "";
	}

	public RebornMenu Header( string text ) { Widgets.Add( new RebornUiWidget { T = "header", A = text ?? "" } ); return this; }
	public RebornMenu Text( string text ) { Widgets.Add( new RebornUiWidget { T = "text", A = text ?? "" } ); return this; }
	public RebornMenu KeyValue( string label, string value ) { Widgets.Add( new RebornUiWidget { T = "kv", A = label ?? "", B = value ?? "" } ); return this; }
	public RebornMenu Divider() { Widgets.Add( new RebornUiWidget { T = "divider" } ); return this; }

	public RebornMenu Table( IEnumerable<string> columns, IEnumerable<IEnumerable<string>> rows )
	{
		Widgets.Add( new RebornUiWidget
		{
			T = "table",
			Cols = columns?.ToList() ?? new List<string>(),
			Rows = rows?.Select( r => r?.ToList() ?? new List<string>() ).ToList() ?? new List<List<string>>(),
		} );
		return this;
	}

	/// <summary>Text field whose value is delivered to every button handler
	/// (inputs dictionary, keyed by this id).</summary>
	public RebornMenu Input( string id, string label, string placeholder = "" )
	{
		Widgets.Add( new RebornUiWidget { T = "input", Id = id ?? "", A = label ?? "", B = placeholder ?? "" } );
		return this;
	}

	public RebornMenu Button( string id, string label, Action<IRebornPlayer, Dictionary<string, string>> onClick, RebornButtonStyle style = RebornButtonStyle.Primary )
	{
		if ( string.IsNullOrWhiteSpace( id ) ) return this;
		Widgets.Add( new RebornUiWidget { T = "button", Id = id, A = label ?? id, Style = style.ToString() } );
		if ( onClick != null ) Handlers[id] = onClick;
		return this;
	}

	internal RebornUiMenuDto ToDto() => new()
	{
		Id = Id,
		Title = Title,
		Icon = Icon,
		Accent = Accent,
		Background = Background,
		Width = Width,
		Widgets = Widgets,
	};
}

public static partial class Reborn
{
	// menuId -> (target steamid, menu with handlers). Host-side registry.
	static readonly Dictionary<string, (string SteamId, RebornMenu Menu)> _activeMenus = new( StringComparer.Ordinal );

	/// <summary>Show (or refresh) a menu on one player's screen.</summary>
	public static void ShowMenu( IRebornPlayer player, RebornMenu menu )
	{
		if ( Host == null || player == null || menu == null ) return;
		_activeMenus[menu.Id] = (player.SteamId, menu);
		Host.SendUi( player, "menu", Json.Serialize( menu.ToDto() ) );
	}

	/// <summary>Close a menu previously shown to this player.</summary>
	public static void CloseMenu( IRebornPlayer player, RebornMenu menu )
	{
		if ( Host == null || player == null || menu == null ) return;
		_activeMenus.Remove( menu.Id );
		Host.SendUi( player, "menu_close", menu.Id );
	}

	/// <summary>Small always-on card in the player's top-right HUD stack.
	/// Same id = refresh in place.</summary>
	public static void ShowHudCard( IRebornPlayer player, string id, string title, IEnumerable<string> lines, string accent = "" )
	{
		if ( Host == null || player == null || string.IsNullOrWhiteSpace( id ) ) return;
		var dto = new RebornUiHudDto { Id = id, Title = title ?? "", Accent = accent ?? "", Lines = lines?.ToList() ?? new List<string>() };
		Host.SendUi( player, "hud", Json.Serialize( dto ) );
	}

	public static void RemoveHudCard( IRebornPlayer player, string id )
	{
		if ( Host == null || player == null || string.IsNullOrWhiteSpace( id ) ) return;
		Host.SendUi( player, "hud_remove", id );
	}

	/// <summary>Click round-trip entry (called by the renderer's host RPC).
	/// Only the player the menu was shown to may trigger its handlers.</summary>
	internal static void HandleMenuClick( string callerSteamId, string menuId, string buttonId, Dictionary<string, string> inputs )
	{
		if ( !_activeMenus.TryGetValue( menuId ?? "", out var entry ) ) return;
		if ( entry.SteamId != callerSteamId ) return;

		if ( buttonId == "__close" )
		{
			_activeMenus.Remove( menuId );
			return;
		}
		if ( !entry.Menu.Handlers.TryGetValue( buttonId ?? "", out var handler ) ) return;

		var player = FindPlayer( callerSteamId );
		if ( player == null ) return;
		try { handler( player, inputs ?? new Dictionary<string, string>() ); }
		catch ( Exception e ) { RebornLogSink.Warn( $"[addons] menu '{entry.Menu.Title}' button '{buttonId}' threw: {e.Message}" ); }
	}

	// ---- HTTP (contract v3) ----------------------------------------------
	// Works for domains present in the game's HTTP allowlist: ask for your
	// addon's domain at review time. Returns null on any failure.

	public static async Task<string> HttpGetAsync( string url )
	{
		try { return await Http.RequestStringAsync( url, "GET" ); }
		catch ( Exception e ) { RebornLogSink.Warn( $"[addons] HttpGet {url}: {e.Message}" ); return null; }
	}

	public static async Task<string> HttpPostJsonAsync( string url, string json )
	{
		try
		{
			var content = new System.Net.Http.StringContent( json ?? "", System.Text.Encoding.UTF8, "application/json" );
			var response = await Http.RequestAsync( url, "POST", content: content );
			return response == null ? null : await response.Content.ReadAsStringAsync();
		}
		catch ( Exception e ) { RebornLogSink.Warn( $"[addons] HttpPost {url}: {e.Message}" ); return null; }
	}

	// ---- keyed per-addon storage (contract v3) ---------------------------
	// Your addon's own little database: one JSON blob per key, all sandboxed
	// under the addon's ident. The game's databases stay sealed by design.

	public static void SaveData( string addonIdent, string key, string json )
	{
		try
		{
			FileSystem.Data.CreateDirectory( "csaddons" );
			FileSystem.Data.WriteAllText( $"csaddons/{Sanitize( addonIdent )}.{Sanitize( key )}.json", json ?? "" );
		}
		catch ( Exception e ) { RebornLogSink.Warn( $"[addon:{addonIdent}] SaveData({key}) failed: {e.Message}" ); }
	}

	public static string LoadData( string addonIdent, string key )
	{
		try
		{
			var path = $"csaddons/{Sanitize( addonIdent )}.{Sanitize( key )}.json";
			return FileSystem.Data.FileExists( path ) ? FileSystem.Data.ReadAllText( path ) : null;
		}
		catch { return null; }
	}
}
