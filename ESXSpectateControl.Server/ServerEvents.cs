using CitizenFX.Core;
using CitizenFX.Core.Native;
using ESXSpectateControl.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace ESXSpectateControl.Server
{
	static class ServerEvents
	{
		public static void Init()
		{
			ServerMain.Instance.AddCommand("spectate", new Action<int, List<object>, string>(Command), true);
			ServerMain.Instance.ESX.RegisterServerCallback("spectate:getOtherPlayerData", new Action<int, dynamic, int>(GetOtherPlayerData));
			ServerMain.Instance.AddEventHandler("spectate:CheckAdmin", new Action<Player>(CheckIsAdmin));
			ServerMain.Instance.AddEventHandler("spectate:spectate", new Action<Player>(spectate));
			ServerMain.Instance.AddEventHandler("RestoreCulling", new Action<Player, int>(RestoreCullingRadius));
		}

		private static async void spectate([FromSource] Player player)
		{
			Dictionary<int, string> people = new();
			if (API.IsPlayerAceAllowed(player.Handle, "spectate"))
			{
				foreach (var p in ServerMain.Instance.GetPlayers)
				{
					if (p.Handle == player.Handle) continue;
					people.Add(Convert.ToInt32(p.Handle), p.Name);
				}
				player.TriggerEvent("spectate:openMenu", people.ToJson());
			}
			else
				player.TriggerEvent("chat:addMessage", new { args = new[] { "[ERROR] = ", "You re not allowed!" }, color = new[] { 255, 0, 0 } });
		}

		private static async void CheckIsAdmin([FromSource] Player player)
		{
			var isAdmin = API.IsPlayerAceAllowed(player.Handle, "spectate");
			player.TriggerEvent("spectate:checkedAdmin", isAdmin);
		}

		private static async void Command(int a, List<object> b, string c)
		{
			Dictionary<int, string> people = new();
			if (API.IsPlayerAceAllowed(a.ToString(), "spectate"))
			{
				foreach (var p in ServerMain.Instance.GetPlayers)
				{
					if (p.Handle == a.ToString()) continue;
					people.Add(Convert.ToInt32(p.Handle), p.Name);
				}
				ServerMain.Instance.GetPlayers[a].TriggerEvent("spectate:openMenu", people.ToJson());
			}
			else
				ServerMain.Instance.GetPlayers[a].TriggerEvent("chat:addMessage", new { args = new[] { "[ERROR] = ", "You re not allowed!" }, color = new[] { 255, 0, 0 } });
		}

		private static async void GetOtherPlayerData(int a, dynamic cb, int d)
		{
			try
			{
				Player player = ServerMain.Instance.GetPlayers[d];
				API.SetEntityDistanceCullingRadius(player.Character.Handle, 5000f);
				dynamic xPlayer = ServerMain.Instance.ESX.GetPlayerFromId(d);
				await BaseScript.Delay(0);
				// using identifier
				dynamic result = await MySQL.QuerySingleAsync("SELECT * FROM users WHERE identifier = @identifier", new { identifier = xPlayer.identifier });
				dynamic resultVeh = await MySQL.QueryListAsync("SELECT * FROM owned_vehicles WHERE owner = @identifier", new { identifier = xPlayer.identifier });
				dynamic resultProps = await MySQL.QueryListAsync("SELECT * FROM owned_properties WHERE owner = @identifier", new { identifier = xPlayer.identifier });
				await BaseScript.Delay(0);
				int money = (int)xPlayer.getMoney();
				int bank = (int)xPlayer.getAccount("bank").money;
				int dirty = (int)xPlayer.getAccount("black_money").money;
				string firstname = string.IsNullOrWhiteSpace(result.firstname) ? "EmptyName" : result.firstname;
				string lastname = string.IsNullOrWhiteSpace(result.lastname) ? "EmptyLastName" : result.lastname;
				dynamic job = xPlayer.getJob();
				dynamic inventory = xPlayer.getInventory(true);
				dynamic loadout = xPlayer.getLoadout(true);

				Vector3 pos = player.Character.Position;
				int netid = player.Character.NetworkId;
				object returning = new
				{
					money,
					bank,
					dirty,
					firstname,
					lastname,
					job,
					inventory,
					loadout,
					position = pos,
					netId = netid,
					vehs = resultVeh,
					props = resultProps
				};
				cb(returning);
			}
			catch (Exception e)
			{
				ServerMain.Logger.Error(e.ToString());
			}
		}

		private static void RestoreCullingRadius([FromSource] Player player, int netId)
		{
			var entity = (Ped)Entity.FromNetworkId(netId);
			API.SetEntityDistanceCullingRadius(entity.Handle, 0f);
		}
	}
}
