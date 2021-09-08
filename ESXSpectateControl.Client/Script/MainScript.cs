using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;
using NativeUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESXSpectateControl.Client.Script
{
	static class MainScript
	{
		public static MenuPool MenuPool = new();
		public static bool InSpectatorMode;
		public static Player spectatingPlayer;

		public static void Init()
		{
			MenuPool.RefreshIndex();
			ClientMain.Instance.AddEventHandler("spectate:openMenu", new Action<string>(Menu.MainMenu.Open));
			ClientMain.Instance.AddEventHandler("spectate:checkedAdmin", new Action<bool>((a) => { if (a) ClientMain.Instance.AddTick(MainTick); }));
			BaseScript.TriggerServerEvent("spectate:CheckAdmin");
			Game.PlayerPed.Style.SetDefaultClothes();
		}

		private static async Task MainTick()
		{
			MenuPool.ProcessMenus();

			/* FOR TESTING PURPUSES.. USE COMMAND IS BETTER AND SAFER
			if(Game.IsControlJustPressed(0, Control.Detonate) && !MenuPool.IsAnyMenuOpen())
				BaseScript.TriggerServerEvent("spectate:spectate");
			*/

			if (!InSpectatorMode) return;

			Ped ped = spectatingPlayer.Character;
			Game.EnableControlThisFrame(0, Control.NextCamera);
			Game.EnableControlThisFrame(1, Control.NextCamera);
			Game.EnableControlThisFrame(2, Control.NextCamera);
			Game.EnableControlThisFrame(0, Control.PushToTalk);
			Game.EnableControlThisFrame(1, Control.PushToTalk);
			Game.EnableControlThisFrame(2, Control.PushToTalk);
			Game.EnableControlThisFrame(0, Control.LookLeftRight);
			Game.EnableControlThisFrame(1, Control.LookLeftRight);
			Game.EnableControlThisFrame(2, Control.LookLeftRight);
			Game.EnableControlThisFrame(0, Control.LookUpDown);
			Game.EnableControlThisFrame(1, Control.LookUpDown);
			Game.EnableControlThisFrame(2, Control.LookUpDown);

			Notifications.DrawText(0.35f, 0.7f, $"GodMode: {(ped.IsInvincible ? $"~r~{ClientMain.Texts["Enabled"]}~w~." : $"~g~{ClientMain.Texts["Disabled"]}~w~")}");
			if (!ped.IsInVehicle())
			{
				Notifications.DrawText(0.35f, 0.725f, $"AntiRagdoll: {(ped.CanRagdoll && !ped.IsInVehicle() && (ped.ParachuteState == ParachuteState.None || ped.ParachuteState == ParachuteState.FreeFalling) && !ped.IsInParachuteFreeFall ? "~g~Disabled~w~" : "~r~Enabled~w~.")}");
				Notifications.DrawText(0.35f, 0.75f, $"{ClientMain.Texts["Health"]}: {ped.Health}/{ped.MaxHealth}");
				Notifications.DrawText(0.35f, 0.775f, $"{ClientMain.Texts["Armor"]}: {ped.Armor}");
			}
			else
			{
				Vehicle veicolo = ped.CurrentVehicle;
				Vector3 entityPos = veicolo.Position;
				float x = 0, y = 0;
				World3dToScreen2d(entityPos.X, entityPos.Y, entityPos.Z, ref x, ref y);
				Vector2 pos = new(x, y);
				if (pos.X <= 0f || pos.Y <= 0f || pos.X >= 1f || pos.Y >= 1f) pos = new(0.6f, 0.5f);
				float dist = Vector3.Distance(ped.Position, entityPos);
				float offsetX = MathUtil.Clamp((1f - dist / 100f) * 0.1f, 0f, 0.1f);
				pos.X += offsetX;
				Dictionary<string, string> data = new()
				{
					[ClientMain.Texts["Model"]] = GetDisplayNameFromVehicleModel((uint)veicolo.Model.Hash),
					["Hash"] = $"{(uint)veicolo.Model.Hash}",
					["Hash (Hex)"] = $"0x{(uint)veicolo.Model.Hash:X}",
					[ClientMain.Texts["Plate"]] = veicolo.Mods.LicensePlate,
					[""] = string.Empty,
					[ClientMain.Texts["EngineHealth"]] = $"{Math.Round(veicolo.EngineHealth, 2)} / 1,000.0",
					[ClientMain.Texts["BodyHealth"]] = $"{Math.Round(veicolo.BodyHealth, 2)} / 1,000.0",
					[ClientMain.Texts["Speed"]] = $"{Math.Round(veicolo.Speed * 3.6f, 0)} KM/H - {Math.Round(veicolo.Speed * 2.236936f, 0)} MPH",
					[ClientMain.Texts["RPM"]] = $"{Math.Round(veicolo.CurrentRPM * 1000, 2)}",
					[ClientMain.Texts["Gear"]] = $"{veicolo.CurrentGear}",
					[ClientMain.Texts["Acceleration"]] = $"{Math.Round(veicolo.Acceleration, 3)}",
					[ClientMain.Texts["MaxBraking"]] = $"{Math.Round(veicolo.MaxBraking, 3)}",
					[ClientMain.Texts["MaxTraction"]] = $"{Math.Round(veicolo.MaxTraction, 3)}"
				};
				DrawRect(pos.X + 0.12f, pos.Y, 0.24f, data.Count * 0.024f + 0.048f, 0, 0, 0, 120);
				float offsetY = data.Count * 0.012f;
				pos.Y -= offsetY;
				pos.X += 0.07f;

				// Draw data
				foreach (KeyValuePair<string, string> entry in data)
				{
					if (!string.IsNullOrEmpty(entry.Value)) Notifications.DrawText(pos.X, pos.Y, $"{entry.Key}: {entry.Value}");
					pos.Y += 0.024f;
				}
			}
		}
	}
}
