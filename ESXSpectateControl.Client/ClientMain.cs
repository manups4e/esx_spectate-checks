using CitizenFX.Core;
using CitizenFX.Core.Native;
using ESXSpectateControl.Client.Script;
using ESXSpectateControl.Shared;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESXSpectateControl.Client
{
	public class ClientMain : BaseScript
	{
		public static Log Logger = new();
		public static ClientMain Instance { get; protected set; }
		public ExportDictionary GetExports { get { return Exports; } }
		public PlayerList GetPlayers { get { return Players; } }
		public static Dictionary<string, string> Texts = new();
		public dynamic ESX = null;
		public dynamic PlayerData;

		public ClientMain()
		{
			EventHandlers["esx:setJob"] += new Action<dynamic>(setJob);
			EventHandlers["esx:playerLoaded"] += new Action<dynamic>(Loaded);
			Instance = this;
			string text = API.LoadResourceFile(API.GetCurrentResourceName(), "locales.json");
			Texts = text.FromJson<Dictionary<string, string>>();
			Loaded(null);
		}

		private void setJob(dynamic job)
		{
			PlayerData.job = job;
		}

		private async void Loaded(dynamic val)
		{
			while (ESX == null)
			{
				await Delay(0);
				TriggerEvent("esx:getSharedObject", new object[] { new Action<dynamic>(esx => {
					ESX = esx;
				})});
			}
			PlayerData = val;
			if (PlayerData is null) PlayerData = ESX.GetPlayerData();
			MainScript.Init();
		}

		/// <summary>
		/// registra un evento client (TriggerEvent)
		/// </summary>
		/// <param name="eventName">Nome evento</param>
		/// <param name="action">Azione legata all'evento</param>
		public void AddEventHandler(string eventName, Delegate action)
		{
			EventHandlers[eventName] += action;
		}

		/// <summary>
		/// Rimuove un evento client (TriggerEvent)
		/// </summary>
		/// <param name="eventName">Nome evento</param>
		/// <param name="action">Azione legata all'evento</param>
		public void RemoveEventHandler(string eventName, Delegate action)
		{
			EventHandlers[eventName] -= action;
		}

		/// <summary>
		/// Registra una funzione OnTick
		/// </summary>
		/// <param name="onTick"></param>
		public void AddTick(Func<Task> onTick) { Tick += onTick; }

		/// <summary>
		/// Rimuove la funzione OnTick
		/// </summary>
		/// <param name="onTick"></param>
		public void RemoveTick(Func<Task> onTick) { Tick -= onTick; }

		/// <summary>
		/// registra un export, Registered Exports still have to be defined in the fxmanifest.lua file
		/// </summary>
		/// <param name="name"></param>
		/// <param name="action"></param>
		public void RegisterExport(string name, Delegate action)
		{
			GetExports.Add(name, action);
		}
	}
}
