using System.Text.Json.Serialization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Config;

namespace K4BombWires;

public enum WireColor
{
	Red,
	Blue,
	Yellow,
	Green,
	Max
}

public class Config : BasePluginConfig
{
	[JsonPropertyName("ConfigVersion")]
	public override int Version { get; set; } = 2;

	[JsonPropertyName("MenuTimeout")]
	/// <summary>
	/// Timeout in seconds for the wire selection menu
	/// </summary>
	public int MenuTimeout { get; set; } = 5;

	[JsonPropertyName("RandomizeWireOrder")]
	/// <summary>
	/// If enabled, the order of wire colors in the menu will be randomized.
	/// </summary>
	public bool RandomizeWireOrder { get; set; } = false;

	[JsonPropertyName("PanoramaLayout")]
	/// <summary>
	/// Panel layout in the workshop addon. Empty = no panel, wires can still be picked with W/A/S/D.
	/// </summary>
	public string PanoramaLayout { get; set; } = "panorama/layout/custom_game/bombwires.xml";
}

public class Plugin : BasePlugin, IPluginConfig<Config>
{
	public override string ModuleName => "K4 - Bomb Wires";
	public override string ModuleVersion => "1.1.0";
	public override string ModuleAuthor => "K4ryuu";
	public override string ModuleDescription => "A bomb defuse minigame where CT optionally can guess the correct wire color set by T.";

	public Config Config { get; set; } = new ();
	
	private WireColor _currentWire = WireColor.Max;
	private WirePanel? _wirePanel;

	public override void Load(bool hotReload)
	{
		_wirePanel = new WirePanel(this);
		RegisterEventHandlers();
	}

	public override void Unload(bool hotReload)
	{
		_wirePanel?.Destroy();
	}

	public void OnConfigParsed(Config config)
	{
		Config = config;
	}

	private void RegisterEventHandlers()
	{
		RegisterEventHandler<EventRoundStart>(OnRoundStart, HookMode.Post);
		RegisterEventHandler<EventRoundPrestart>(OnRoundPrestart, HookMode.Post);
		RegisterEventHandler<EventRoundEnd>(OnRoundEnd, HookMode.Post);
		RegisterEventHandler<EventBombBeginplant>(OnBombBeginPlant, HookMode.Post);
		RegisterEventHandler<EventBombPlanted>(OnBombPlanted, HookMode.Post);
		RegisterEventHandler<EventBombBegindefuse>(OnBombBeginDefuse, HookMode.Post);
		RegisterEventHandler<EventBombDefused>(OnBombDefused, HookMode.Post);
		RegisterEventHandler<EventBombExploded>(OnBombExploded, HookMode.Post);
		RegisterEventHandler<EventBombAbortdefuse>(OnDefuseAborted, HookMode.Post);
		RegisterEventHandler<EventBombAbortplant>(OnPlantAborted, HookMode.Post);
	}
	
	private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
	{
		_currentWire = WireColor.Max;
		return HookResult.Continue;
	}

	private HookResult OnRoundPrestart(EventRoundPrestart @event, GameEventInfo info)
	{
		// Close wire menus for all players when round ends
		_wirePanel?.CloseAll();

		return HookResult.Continue;
	}

	private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
	{
		if (_currentWire != WireColor.Max)
		{
			foreach (var p in Utilities.GetPlayers().Where(x => x.IsValid && !x.IsBot))
			{
				string correctWireName = GetWireTranslation(_currentWire, p);
				p.PrintToChat($"{Localizer.ForPlayer(p, "k4.general.prefix")} {Localizer.ForPlayer(p, "k4.chat.round_end", correctWireName)}");
			}
		}
		_currentWire = WireColor.Max;
		return HookResult.Continue;
	}

	private HookResult OnBombBeginPlant(EventBombBeginplant @event, GameEventInfo info)
	{
		CCSPlayerController? player = @event.Userid;

		if (player?.IsValid != true || player.IsBot)
			return HookResult.Continue;

		ShowWireMenu(player, true);
		return HookResult.Continue;
	}

	private HookResult OnBombPlanted(EventBombPlanted @event, GameEventInfo info)
	{
		CCSPlayerController? player = @event.Userid;
		if (player != null)	ClearMenuIfValidPlayer(player);

		// If T didn't select a wire, pick random
		if (_currentWire == WireColor.Max)
		{
			_currentWire = (WireColor)Random.Shared.Next(0, (int)WireColor.Max);
			if (player?.IsValid == true && !player.IsBot)
			{
				string wireName = GetWireTranslation(_currentWire, player);
				player.PrintToChat($"{Localizer.ForPlayer(player, "k4.general.prefix")} {Localizer.ForPlayer(player, "k4.chat.random", wireName)}");
			}
		}

		return HookResult.Continue;
	}

	private HookResult OnBombBeginDefuse(EventBombBegindefuse @event, GameEventInfo info)
	{
		CCSPlayerController? player = @event.Userid;
		if (player?.IsValid != true || player.IsBot)
			return HookResult.Continue;

		ShowWireMenu(player, false);
		return HookResult.Continue;
	}

	private HookResult OnBombDefused(EventBombDefused @event, GameEventInfo info)
	{
		CCSPlayerController? player = @event.Userid;
		if (player != null) ClearMenuIfValidPlayer(player);
		return HookResult.Continue;
	}

	private HookResult OnBombExploded(EventBombExploded @event, GameEventInfo info)
	{
		CCSPlayerController? player = @event.Userid;
		if (player != null) ClearMenuIfValidPlayer(player);
		return HookResult.Continue;
	}

	private HookResult OnDefuseAborted(EventBombAbortdefuse @event, GameEventInfo info)
	{
		CCSPlayerController? player = @event.Userid;
		if (player != null) ClearMenuIfValidPlayer(player);
		return HookResult.Continue;
	}

	private HookResult OnPlantAborted(EventBombAbortplant @event, GameEventInfo info)
	{
		CCSPlayerController? player = @event.Userid;
		if (player != null) ClearMenuIfValidPlayer(player);
		
		_currentWire = WireColor.Max;
		
		return HookResult.Continue;
	}

	private void ShowWireMenu(CCSPlayerController player, bool planting)
	{
		// Helper message
		player.PrintToChat($"{Localizer.ForPlayer(player, "k4.general.prefix")} {Localizer.ForPlayer(player, planting ? "k4.menu.helper_t" : "k4.menu.helper_ct")}");

		var wireOptions = new List<(WireColor color, string name)>
		{
			(WireColor.Red, Localizer.ForPlayer(player, "k4.menu.red")),
			(WireColor.Blue, Localizer.ForPlayer(player, "k4.menu.blue")),
			(WireColor.Yellow, Localizer.ForPlayer(player, "k4.menu.yellow")),
			(WireColor.Green, Localizer.ForPlayer(player, "k4.menu.green"))
		};

		if (Config.RandomizeWireOrder)
		{
			wireOptions = wireOptions.OrderBy(_ => Random.Shared.Next()).ToList();
		}

		player.PrintToChat($"{Localizer.ForPlayer(player, "k4.general.prefix")} {Localizer.ForPlayer(player, "k4.chat.keys", wireOptions.Select(w => GetWireTranslation(w.color, player)).ToArray<object>())}");

		_wirePanel?.Open(player,
			Localizer.ForPlayer(player, "k4.menu.title"),
			Localizer.ForPlayer(player, "k4.menu.hint"),
			wireOptions,
			Config.MenuTimeout,
			(p, color) => HandleWireSelection(p, color, planting));
	}

	private void HandleWireSelection(CCSPlayerController player, WireColor selectedWire, bool planting)
	{
		ClearMenuIfValidPlayer(player);

		if (planting)
		{
			// T is setting the wire
			_currentWire = selectedWire;
			string wireName = GetWireTranslation(selectedWire, player);
			player.PrintToChat($"{Localizer.ForPlayer(player, "k4.general.prefix")} {Localizer.ForPlayer(player, "k4.chat.select", wireName)}");
		}
		else
		{
			// CT is guessing the wire
			CPlantedC4? bomb = Utilities.FindAllEntitiesByDesignerName<CPlantedC4>("planted_c4").FirstOrDefault();
			if (bomb == null)
				return;

			string playerName = player.PlayerName ?? "Unknown";

			if (_currentWire == selectedWire)
			{
				// Correct! Instant defuse
				bomb.DefuseCountDown = 0;

				foreach (var p in Utilities.GetPlayers().Where(x => x.IsValid && !x.IsBot))
				{
					string wireName = GetWireTranslation(selectedWire, p);
					string correctWireName = GetWireTranslation(_currentWire, p);
					p.PrintToChat($"{Localizer.ForPlayer(p, "k4.general.prefix")} {Localizer.ForPlayer(p, "k4.chat.success", playerName, wireName, correctWireName)}");
				}
			}
			else
			{
				// Wrong! Instant explosion
				bomb.C4Blow = 1.0f;

				foreach (var p in Utilities.GetPlayers().Where(x => x.IsValid && !x.IsBot))
				{
					string wireName = GetWireTranslation(selectedWire, p);
					string correctWireName = GetWireTranslation(_currentWire, p);
					p.PrintToChat($"{Localizer.ForPlayer(p, "k4.general.prefix")} {Localizer.ForPlayer(p, "k4.chat.failure", playerName, wireName, correctWireName)}");
				}
			}

			_currentWire = WireColor.Max;
		}
	}

	private string GetWireTranslation(WireColor wire, CCSPlayerController player)
	{
		return wire switch
		{
			WireColor.Red => $"{ChatColors.Red}{Localizer.ForPlayer(player, "k4.menu.red")}",
			WireColor.Blue => $"{ChatColors.LightBlue}{Localizer.ForPlayer(player, "k4.menu.blue")}",
			WireColor.Yellow => $"{ChatColors.Yellow}{Localizer.ForPlayer(player, "k4.menu.yellow")}",
			WireColor.Green => $"{ChatColors.Green}{Localizer.ForPlayer(player, "k4.menu.green")}",
			_ => Localizer.ForPlayer(player, "k4.menu.red")
		};
	}

	private void ClearMenuIfValidPlayer(CCSPlayerController player)
	{
		if (player?.IsValid == true && !player.IsBot)
		{
			_wirePanel?.Close(player);
		}
	}
}
