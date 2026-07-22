using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using DarkRPReborn.Addons;

// Example addon: a small stock exchange - shows the addon UI framework
// (contract v3). /bourse opens a themed window (the game's design by default),
// prices move every few seconds, Buy/Sell round-trip to the server, and a HUD
// card tracks the player's portfolio. State persists across restarts.

public sealed class StockMarketAddon : RebornAddon
{
	public override string Ident => "example-stock-market";
	public override string Name => "Stock market";
	public override string Author => "DarkRP Reborn";
	public override string Version => "1.0";

	readonly Dictionary<string, float> _prices = new()
	{
		["NORTHBOUND"] = 42f,
		["COCOFASHION"] = 18f,
		["REBORNOIL"] = 87f,
	};

	// steamid -> (symbol -> shares owned)
	Dictionary<string, Dictionary<string, int>> _portfolios = new();

	public override void OnAddonLoaded()
	{
		var saved = Reborn.LoadData( Ident, "portfolios" );
		if ( saved != null )
		{
			try { _portfolios = Json.Deserialize<Dictionary<string, Dictionary<string, int>>>( saved ) ?? new(); }
			catch { _portfolios = new(); }
		}

		// Prices drift every 5 seconds; refresh every open portfolio HUD card.
		Reborn.Every( 5f, () =>
		{
			foreach ( var symbol in _prices.Keys.ToList() )
			{
				var drift = ( Time.Now * 37f + symbol.Length * 13f ) % 7f - 3f;
				_prices[symbol] = MathF.Max( 1f, _prices[symbol] + drift * 0.4f );
			}
			foreach ( var p in Reborn.Players ) RefreshHud( p );
		} );

		Reborn.RegisterChatCommand( "bourse", "Open the stock market", ( player, args ) => ShowMarket( player ), Ident );
		Reborn.Log( Ident, "loaded - type /bourse in game (devkit_say \"/bourse\" in the Dev Kit)" );
	}

	Dictionary<string, int> PortfolioOf( IRebornPlayer p )
	{
		if ( !_portfolios.TryGetValue( p.SteamId, out var folio ) )
			_portfolios[p.SteamId] = folio = new Dictionary<string, int>();
		return folio;
	}

	void ShowMarket( IRebornPlayer player )
	{
		var folio = PortfolioOf( player );
		var menu = new RebornMenu( "Bourse de Reborn City" ) { Icon = "trending_up" };
		// Want your own look? Uncomment to override the game's default skin:
		// menu.Accent = "#2ecc71"; menu.Background = "#0c1a12";

		menu.Header( "Cours du marché" )
			.Table(
				new[] { "Action", "Cours", "Vous" },
				_prices.Select( kv => new[] { kv.Key, $"{kv.Value:0.0}$", $"{folio.GetValueOrDefault( kv.Key )} parts" } ) )
			.Divider()
			.Header( "Passer un ordre" )
			.Input( "symbol", "Action", "NORTHBOUND" )
			.Input( "qty", "Quantité", "10" )
			.Button( "buy", "Acheter", Buy )
			.Button( "sell", "Vendre", Sell, RebornButtonStyle.Ghost )
			.Text( $"Liquide : {player.Money}$" );

		Reborn.ShowMenu( player, menu );
	}

	void Buy( IRebornPlayer player, Dictionary<string, string> inputs ) => Trade( player, inputs, buying: true );
	void Sell( IRebornPlayer player, Dictionary<string, string> inputs ) => Trade( player, inputs, buying: false );

	void Trade( IRebornPlayer player, Dictionary<string, string> inputs, bool buying )
	{
		var symbol = (inputs.GetValueOrDefault( "symbol" ) ?? "").Trim().ToUpperInvariant();
		if ( !_prices.TryGetValue( symbol, out var price ) )
		{
			player.Notify( "Bourse", $"Action inconnue : {symbol}", RebornNotifyType.Warning );
			return;
		}
		if ( !int.TryParse( inputs.GetValueOrDefault( "qty" ), out var qty ) || qty <= 0 || qty > 10000 )
		{
			player.Notify( "Bourse", "Quantité invalide.", RebornNotifyType.Warning );
			return;
		}

		var folio = PortfolioOf( player );
		int cost = (int)MathF.Ceiling( price * qty );

		if ( buying )
		{
			if ( !player.TakeMoney( cost, $"Achat {qty}x {symbol}" ) )
			{
				player.Notify( "Bourse", "Fonds insuffisants.", RebornNotifyType.Error );
				return;
			}
			folio[symbol] = folio.GetValueOrDefault( symbol ) + qty;
			player.Notify( "Bourse", $"Acheté {qty}x {symbol} pour {cost}$.", RebornNotifyType.Success );
		}
		else
		{
			if ( folio.GetValueOrDefault( symbol ) < qty )
			{
				player.Notify( "Bourse", $"Vous n'avez pas {qty} parts de {symbol}.", RebornNotifyType.Error );
				return;
			}
			folio[symbol] -= qty;
			player.GiveMoney( cost, $"Vente {qty}x {symbol}" );
			player.Notify( "Bourse", $"Vendu {qty}x {symbol} pour {cost}$.", RebornNotifyType.Success );
		}

		Reborn.SaveData( Ident, "portfolios", Json.Serialize( _portfolios ) );
		RefreshHud( player );
		ShowMarket( player );   // refresh the open window with new numbers
	}

	void RefreshHud( IRebornPlayer player )
	{
		var folio = PortfolioOf( player );
		var held = folio.Where( kv => kv.Value > 0 ).ToList();
		if ( held.Count == 0 ) { Reborn.RemoveHudCard( player, "stock-folio" ); return; }

		var worth = held.Sum( kv => (int)( _prices.GetValueOrDefault( kv.Key ) * kv.Value ) );
		Reborn.ShowHudCard( player, "stock-folio", "Portefeuille",
			held.Select( kv => $"{kv.Key} × {kv.Value}" ).Append( $"Valeur : {worth}$" ) );
	}
}
