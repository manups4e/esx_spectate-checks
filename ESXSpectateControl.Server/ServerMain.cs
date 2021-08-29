using CitizenFX.Core;
using CitizenFX.Core.Native;
using ESXSpectateControl.Shared;
using Logger;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ESXSpectateControl.Server
{
	public class ServerMain : BaseScript
	{
		public static Log Logger { get; set; }
		public static ServerMain Instance { get; protected set; }
		public ExportDictionary GetExports { get { return Exports; } }
		public PlayerList GetPlayers { get { return Players; } }
		public dynamic ESX { get; set; }

		public ServerMain()
		{
			Logger = new();
			Instance = this;
			ChiamaESX();
		}

		private async void ChiamaESX()
		{
			while (ESX == null)
			{
				await Delay(0);
				TriggerEvent("esx:getSharedObject", new object[] { new Action<dynamic>(esx => {
					ESX = esx;
				})});
			}
			ServerEvents.Init();
		}

		/// <summary>
		/// registra un evento (TriggerEvent)
		/// </summary>
		/// <param name="name">Nome evento</param>
		/// <param name="action">Azione legata all'evento</param>
		public void AddEventHandler(string eventName, Delegate action) => EventHandlers[eventName] += action;

		/// <summary>
		/// registra un evento (TriggerEvent)
		/// </summary>
		/// <param name="name">Nome evento</param>
		/// <param name="action">Azione legata all'evento</param>
		public void DeAddEventHandler(string eventName, Delegate action) => EventHandlers[eventName] -= action;

		/// <summary>
		/// Registra una funzione OnTick
		/// </summary>
		/// <param name="action"></param>
		public void AddTick(Func<Task> onTick) => Tick += onTick;

		/// <summary>
		/// Rimuove la funzione OnTick
		/// </summary>
		/// <param name="action"></param>
		public void RemoveTick(Func<Task> onTick) => Tick -= onTick;


		/// <summary>
		/// registra un export, Registered exports still have to be defined in the __resource.lua file
		/// </summary>
		/// <param name="name"></param>
		/// <param name="action"></param>
		public void RegisterExport(string name, Delegate action) => Exports.Add(name, action);

		/// <summary>
		/// registra un comando di chat
		/// </summary>
		/// <param name="commandName">Nome comando</param>
		/// <param name="handler">Una nuova Action<int source, List<dynamic> args, string rawCommand</param>
		/// <param name="restricted">tutti o solo chi può?</param>
		public void AddCommand(string commandName, InputArgument handler, bool restricted) => API.RegisterCommand(commandName, handler, restricted);
	}
}
