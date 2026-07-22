-- ============================================================================
--  My Addon - DarkRP Reborn starter skeleton (Lua)
--  Docs: see docs/ in this repository (API reference + tutorials)
--  Deploy: data/rebornstudio/cityliferp/addons/my-addon/ then addon_reload
-- ============================================================================

-- 1) Your texts, ALWAYS fr + en:
locale.Register("myaddon.hello", "Bonjour %s !", "Hello %s!")

-- 2) React to game events:
hook.Add("PlayerInitialSpawn", "myaddon_join", function(ply)
	util.Log(ply:Nick() .. " joined - my addon knows it.")
end)

-- 3) A chat command:
chat.AddCommand("hello", function(ply, args, raw)
	ply:ChatPrint(string.format(locale.T("myaddon.hello"), ply:Nick()))
end, "Say hello")

-- 4) A timer:
-- timer.Create("myaddon_tick", 60, 0, function() ... end)

-- 5) Persistence:
-- data.Set("counter", (data.Get("counter", 0)) + 1)

util.Log("My Addon loaded (v" .. addon.version .. ")")
