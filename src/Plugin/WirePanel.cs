using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Extensions;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;

namespace K4BombWires;

/// Wire picker drawn on a custom_hud_layout panel from the workshop addon
/// (panorama/layout/custom_game/bombwires.xml). One entity is shared by every player who has it
/// open; each player's content is written with the *ForPlayer calls.
/// The panel is display only (no cursor, so the mouse keeps planting): W/A/S/D highlights a wire
/// and Jump confirms it. This also works without the addon.
public class WirePanel
{
	private const string DialogPanel = "bw_dialog";
	private const string HeaderPanel = "bw_header";
	private const string HintPanel = "bw_hint";
	private const string WirePrefix = "bw_wire_";
	private const string NamePrefix = "bw_name_";
	private const string OpenClass = "bw-open";
	private const string SelectedClass = "bw-selected";

	// Key slots in panel order; wire i of a view goes to slot i
	private static readonly (string Id, PlayerButtons Button)[] Keys =
	[
		("w", PlayerButtons.Forward),
		("a", PlayerButtons.Moveleft),
		("s", PlayerButtons.Back),
		("d", PlayerButtons.Moveright),
	];

	private sealed class View(WireColor[] wires, Action<CCSPlayerController, WireColor> onSelect, PlayerButtons buttons)
	{
		public WireColor[] Wires { get; } = wires;
		public Action<CCSPlayerController, WireColor> OnSelect { get; } = onSelect;
		public PlayerButtons LastButtons { get; set; } = buttons;
		public int Selected { get; set; } = -1;
	}

	private readonly Plugin _plugin;
	private CCSCustomHudLayout? _hud;
	private readonly Dictionary<int, View> _views = new();

	public WirePanel(Plugin plugin)
	{
		_plugin = plugin;
		plugin.RegisterListener<Listeners.OnTick>(OnTick);
		plugin.RegisterListener<Listeners.OnMapStart>(_ => Destroy());
	}

	public void Open(CCSPlayerController player, string header, string hint, IReadOnlyList<(WireColor Color, string Name)> wires, float timeout, Action<CCSPlayerController, WireColor> onSelect)
	{
		Close(player);

		// Keys held when the panel opens (e.g. walking onto the site) do not count as a press
		var view = new View([.. wires.Take(Keys.Length).Select(w => w.Color)], onSelect, player.Buttons);
		_views[player.Slot] = view;

		try
		{
			if (EnsureHud(out var hud))
			{
				SetText(hud, player, HeaderPanel, "bw_header", header);
				SetText(hud, player, HintPanel, "bw_hint", hint);

				for (int i = 0; i < Keys.Length; i++)
				{
					string id = Keys[i].Id;
					SetText(hud, player, $"{NamePrefix}{id}", $"bw_name_{id}", i < wires.Count ? wires[i].Name : "");
					for (var color = WireColor.Red; color < WireColor.Max; color++)
						hud.SetHasClassForPlayer(player, $"{WirePrefix}{id}", ColorClass(color), i < wires.Count && wires[i].Color == color);
				}

				ShowSelected(hud, player, view);
				hud.SetHasClassForPlayer(player, DialogPanel, OpenClass, true);
			}
		}
		catch (Exception ex)
		{
			_plugin.Logger.LogError(ex, "[WirePanel] Failed to show the panel for slot {Slot}.", player.Slot);
		}

		// Only closes this view: the player may have closed it and opened a new one meanwhile
		int slot = player.Slot;
		_plugin.AddTimer(timeout, () =>
		{
			if (_views.TryGetValue(slot, out var current) && current == view && Utilities.GetPlayerFromSlot(slot) is { IsValid: true } p)
				Close(p);
		});
	}

	public void Close(CCSPlayerController player)
	{
		if (!_views.Remove(player.Slot))
			return;

		if (_hud is not { IsValid: true } hud || !player.IsValid)
			return;

		try
		{
			hud.SetHasClassForPlayer(player, DialogPanel, OpenClass, false);
		}
		catch (Exception ex)
		{
			_plugin.Logger.LogError(ex, "[WirePanel] Failed to close the panel for slot {Slot}.", player.Slot);
		}
	}

	public void CloseAll()
	{
		foreach (int slot in _views.Keys.ToList())
		{
			if (Utilities.GetPlayerFromSlot(slot) is { IsValid: true } player)
				Close(player);
			else
				_views.Remove(slot);
		}
	}

	private static string ColorClass(WireColor color) => $"bw-{color.ToString().ToLowerInvariant()}";

	private bool EnsureHud(out CCSCustomHudLayout hud)
	{
		if (_hud is { IsValid: true } existing)
		{
			hud = existing;
			return true;
		}

		hud = null!;
		string layout = _plugin.Config.PanoramaLayout.Trim();
		if (layout.Length == 0)
			return false;

		var spawned = Utilities.CreateEntityByName<CCSCustomHudLayout>("custom_hud_layout");
		if (spawned == null || !spawned.IsValid)
			return false;

		using var kv = new CEntityKeyValues();
		kv.SetString("layout", layout);
		kv.SetString("targetname", "bombwires_panorama_menu");
		spawned.DispatchSpawn(kv);

		_hud = hud = spawned;
		return true;
	}

	// Written against the dialog and the label: the client's dialog-variable scope
	// resolution is not pinned down.
	private static void SetText(CCSCustomHudLayout hud, CCSPlayerController player, string panel, string variable, string value)
	{
		hud.SetDialogVariableStringForPlayer(player, DialogPanel, variable, value);
		hud.SetDialogVariableStringForPlayer(player, panel, variable, value);
	}

	private static void ShowSelected(CCSCustomHudLayout hud, CCSPlayerController player, View view)
	{
		for (int i = 0; i < Keys.Length; i++)
			hud.SetHasClassForPlayer(player, $"{WirePrefix}{Keys[i].Id}", SelectedClass, i == view.Selected);
	}

	private void OnTick()
	{
		if (_views.Count == 0)
			return;

		foreach (var (slot, view) in _views.ToList())
		{
			var player = Utilities.GetPlayerFromSlot(slot);
			if (player is not { IsValid: true })
			{
				_views.Remove(slot);
				continue;
			}

			var buttons = player.Buttons;
			var pressed = buttons & ~view.LastButtons;
			view.LastButtons = buttons;

			if ((pressed & PlayerButtons.Jump) != 0 && view.Selected >= 0)
			{
				Close(player);
				view.OnSelect(player, view.Wires[view.Selected]);
				continue;
			}

			for (int i = 0; i < view.Wires.Length; i++)
			{
				if ((pressed & Keys[i].Button) != 0 && view.Selected != i)
				{
					view.Selected = i;
					try
					{
						if (_hud is { IsValid: true } hud)
							ShowSelected(hud, player, view);
					}
					catch (Exception ex)
					{
						_plugin.Logger.LogError(ex, "[WirePanel] Failed to highlight the wire for slot {Slot}.", slot);
					}
					break;
				}
			}
		}
	}

	public void Destroy()
	{
		var hud = _hud;
		_hud = null;
		_views.Clear();

		if (hud is not { IsValid: true })
			return;

		try
		{
			hud.AcceptInput("Kill");
		}
		catch (Exception ex)
		{
			_plugin.Logger.LogError(ex, "[WirePanel] Failed to kill the panel entity.");
		}
	}
}
