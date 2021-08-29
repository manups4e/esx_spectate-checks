using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using CitizenFX.Core.UI;
using ESXSpectateControl.Client.Script;
using ESXSpectateControl.Shared;
using NativeUI;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace ESXSpectateControl.Client.Menu
{
	static class MainMenu
	{
		private static Vector3 lastPosition;
		public static async void Open(string jsonPlayers)
		{
			UIMenu mainMenu = new("Spectate", "Choose the Player");
			mainMenu.MouseControlsEnabled = false;
			MainScript.MenuPool.Add(mainMenu);
			Dictionary<int, string> people = jsonPlayers.FromJson<Dictionary<int, string>>();
			foreach(var p in people)
			{
				UIMenu pMenu = MainScript.MenuPool.AddSubMenu(mainMenu, p.Value + $" [{p.Key}]");
				pMenu.ParentItem.ItemData = new { Handle = p.Key, Name = p.Value };
			}

			mainMenu.OnMenuStateChanged += async (a, b, c) =>
			{
				Game.PlayerPed.IsVisible = false;
				Vector3 position = Vector3.Zero;
				int pedNetId = 0;
				if (c == MenuState.ChangeForward)
				{
					b.Clear();
					Screen.Fading.FadeOut(500);
					await BaseScript.Delay(600);

					var infoMenu = MainScript.MenuPool.AddSubMenu(b, ClientMain.Texts["Info"]);
					var invMenu = MainScript.MenuPool.AddSubMenu(b, ClientMain.Texts["Inventory"]);
					var weapMenu = MainScript.MenuPool.AddSubMenu(b, ClientMain.Texts["Weapons"]);
					var vehMenu = MainScript.MenuPool.AddSubMenu(b, ClientMain.Texts["Vehicles"]);
					var propMenu = MainScript.MenuPool.AddSubMenu(b, ClientMain.Texts["Properties"]);

					int handle = b.ParentItem.ItemData.Handle;
					string PlayerName = b.ParentItem.ItemData.Name;
					ClientMain.Instance.ESX.TriggerServerCallback("spectate:getOtherPlayerData", new Action<dynamic>(async (data) =>
					{
						pedNetId = data.netId;
						position = data.position;
						SetFocusPosAndVel(data.position.X, data.position.Y, data.position.Z, 0, 0, 0);
						var jobLabel = "";
						if (!string.IsNullOrWhiteSpace(data.job.grade_label))
							jobLabel = $"{data.job.label} - {data.job.grade_label}";
						else
							jobLabel = $"{data.job.label}";

						UIMenuItem nameItem = new UIMenuItem(ClientMain.Texts["FullName"]);
						nameItem.SetRightLabel($"{data.firstname} {data.lastname}");
						UIMenuItem moneyItem = new UIMenuItem(ClientMain.Texts["Money"]);
						moneyItem.SetRightLabel("$" + data.money);
						UIMenuItem bankItem = new UIMenuItem(ClientMain.Texts["Bank"]);
						bankItem.SetRightLabel("$" + data.bank);
						UIMenuItem blackItem = new UIMenuItem(ClientMain.Texts["Black"]);
						blackItem.SetRightLabel("$" + (int)data.dirty);
						UIMenuItem jobItem = new(ClientMain.Texts["Job"]);
						jobItem.SetRightLabel(jobLabel);
						UIMenuItem playerNameItem = new(ClientMain.Texts["PlayerName"]);
						playerNameItem.SetRightLabel(PlayerName + $" [{handle}]");
						infoMenu.AddItem(nameItem);
						infoMenu.AddItem(moneyItem);
						infoMenu.AddItem(bankItem);
						infoMenu.AddItem(blackItem);
						infoMenu.AddItem(jobItem);
						infoMenu.AddItem(playerNameItem);

						if ((data.inventory as IDictionary<string, object>).Count == 0)
						{
							UIMenuItem itm = new(ClientMain.Texts["InvEmpty"]);
							invMenu.AddItem(itm);
						}
						else
						{
							foreach (var inv in data.inventory)
							{
								UIMenuItem itm = new(inv.Key);
								itm.SetRightLabel("" + inv.Value);
								invMenu.AddItem(itm);
							}
						}


						if ((data.loadout as IDictionary<string, object>).Count == 0)
						{
							UIMenuItem itm = new(ClientMain.Texts["WeapEmpty"]);
							weapMenu.AddItem(itm);
						}
						else
						{
							foreach (var wea in data.loadout)
							{
								UIMenuItem wpn = new(ClientMain.Instance.ESX.GetWeaponLabel(wea.Key));
								weapMenu.AddItem(wpn);
							}
						}

						if ((data.vehs as IEnumerable<object>).Count() == 0)
						{
							UIMenuItem itm = new(ClientMain.Texts["VehEmpty"]);
							vehMenu.AddItem(itm);
						}
						else
						{
							foreach (var veh in data.vehs)
							{
								dynamic props = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(veh.vehicle as string);
								UIMenuItem vehItem = new(GetDisplayNameFromVehicleModel((uint)Convert.ToInt32((props as JObject).GetValue("model"))) + " [" + veh.plate + "]");
								vehItem.SetRightLabel("Type: " + veh.type);
								vehMenu.AddItem(vehItem);
							}
						}

						if((data.props as IEnumerable<object>).Count() == 0)
						{
							UIMenuItem itm = new(ClientMain.Texts["PropertiesEmpty"]);
							propMenu.AddItem(itm);
						}
						else
						{
							foreach(var prop in data.props)
							{
								UIMenuItem propItem = new(prop.name);
								propMenu.AddItem(propItem);
							}
						}
						lastPosition = Game.PlayerPed.Position;
						NetworkFadeOutEntity(Game.PlayerPed.Handle, true, false);
						Game.PlayerPed.IsCollisionEnabled = false;
						Game.PlayerPed.IsPositionFrozen = true;
						Game.PlayerPed.Position = position;
						Ped ped = null;
						while (ped is null)
						{
							ped = World.GetAllPeds().FirstOrDefault(x => x.NetworkId == pedNetId);
							if (ped != null)
								Game.PlayerPed.AttachTo((Ped)Entity.FromNetworkId(pedNetId));
							await BaseScript.Delay(250);
						}
						MainScript.spectatingPlayer = new(NetworkGetPlayerIndexFromPed(ped.Handle));
						while (infoMenu.MenuItems.Count == 0) await BaseScript.Delay(2000);
						MainScript.InSpectatorMode = true;
						Screen.Fading.FadeIn(500);
					}), handle);
				}
				else if (c == MenuState.ChangeBackward)
				{
					Screen.Fading.FadeOut(500);
					await BaseScript.Delay(600);
					Game.PlayerPed.Detach();
					Game.PlayerPed.Position = lastPosition;
					ClearFocus();
					Game.PlayerPed.IsCollisionEnabled = true;
					Game.PlayerPed.IsPositionFrozen = false;
					NetworkFadeInEntity(Game.PlayerPed.Handle, false);
					Game.PlayerPed.IsVisible = true;
					MainScript.InSpectatorMode = false;
					Screen.Fading.FadeIn(500);
				}
			};
			mainMenu.Visible = true;
		}
	}
}
