-- ============================================================================
--  Mon Addon — squelette de départ DarkRP Reborn
--  Docs : https://docs.darkrp-reborn.com (référence API + tutoriels)
--  Déployer : data/rebornstudio/cityliferp/addons/mon-addon/ puis addon_reload
-- ============================================================================

-- 1) Vos textes, TOUJOURS en fr + en :
locale.Register("monaddon.hello", "Bonjour %s !", "Hello %s!")

-- 2) Réagir aux événements du jeu :
hook.Add("PlayerInitialSpawn", "monaddon_join", function(ply)
	util.Log(ply:Nick() .. " a rejoint — mon addon le sait.")
end)

-- 3) Une commande chat :
chat.AddCommand("hello", function(ply, args, raw)
	ply:ChatPrint(string.format(locale.T("monaddon.hello"), ply:Nick()))
end, "Dire bonjour")

-- 4) Un timer :
-- timer.Create("monaddon_tick", 60, 0, function() ... end)

-- 5) De la persistance :
-- data.Set("compteur", (data.Get("compteur", 0)) + 1)

util.Log("Mon Addon chargé (v" .. addon.version .. ")")
