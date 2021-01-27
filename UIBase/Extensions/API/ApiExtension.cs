﻿using System;
using System.Collections;
using WengaPort.Api;
using MelonLoader;
using Newtonsoft.Json;
using VRC.Core;
using WebSocketSharp;
using WengaPort.Modules;
using WengaPort.ConsoleUtils;
using System.Threading;
using System.Runtime.InteropServices;
using UnityEngine;
using WengaPort.Wrappers;

namespace WengaPort.Api
{
	internal class ApiExtension
	{
		public static bool ApiConsole = false;
		public static bool ApiNotify = true;
		public static void Start()
		{
			Extensions.Logger.WengaLogger("[API] Connecting Websocket");
			MelonCoroutines.Start(Connect());
		}

		public static void CloseWebsocket()
        {
			ws.Close();
		}

		private static WebSocket ws;
		private static int fails = 0;
		private static IEnumerator Connect()
		{
			while (string.IsNullOrEmpty(ApiCredentials.authToken))
			{
				yield return null;
			}
            try
            {
	            ws = new WebSocket("wss://pipeline.vrchat.cloud/?authToken=" + ApiCredentials.authToken);
	            ws.OnMessage += HandleMessage;
				ws.OnClose += HandleClose;
				ws.Log.Level = LogLevel.Fatal;
				ws.Connect();
			}
            catch (Exception e)
            {
				Extensions.Logger.WengaLogger("[API Error] VRChat API Error: " + e);			
			}
			yield break;
		}

		private static void HandleClose(object sender, CloseEventArgs e)
		{
			try
			{
				if (e.Code == 1006)
                {
					fails += 1;
					if (fails >= 10)
                    {
						Extensions.Logger.WengaLogger($"[API Error] Websocket closed because of ({fails}), log out and restart your game to fix this");
						ws.Close();
                    }
					else
                    {
						ws.Connect();
					}
				}
				else
                {
					fails = 0;
					Extensions.Logger.WengaLogger("[API Error] VRChat Pipeline WebSocket closed due to: " + e.Reason + "  |  Code: " + e.Code);
					ws.Connect();
				}
			}
			catch { }
		}
		
		private static void HandleMessage(object sender, MessageEventArgs e)
		{
            try
            {
				fails = 0;
				if (!ApiNotify)
                {
					return;
                }

				char[] charsToTrim = { '*', ' ', '\'', '\\' };
				string Data = e.Data.Trim(charsToTrim);
				var WebSocketRawData = JsonConvert.DeserializeObject<WebSocketObject>(Data);
				var WebSocketData = JsonConvert.DeserializeObject<WebSocket2Object>(WebSocketRawData?.content);
				User apiuser = WebSocketData?.user;
				switch (WebSocketRawData.type)
                {
					case "friend-online":
						MelonCoroutines.Start(Extensions.Logger.WebsocketLogger(VRConsole.LogsType.Online, $"{apiuser.displayName}"));
						Extensions.Logger.WengaLogger($"[API] {apiuser.displayName} -> Online");
						break;
					case "friend-offline":
						APIUser.FetchUser(WebSocketData.userId, new Action<APIUser>((user) =>
						{
							MelonCoroutines.Start(Extensions.Logger.WebsocketLogger(VRConsole.LogsType.Offline, $"{user.displayName}"));
							Extensions.Logger.WengaLogger($"[API] {user.DisplayName()} -> Offline");
						}),
						new Action<string>((text) => { }));
						break;
					case "friend-location":
						if (apiuser.id != APIUser.CurrentUser.id)
						{
							if (WebSocketData.location == "private")
                            {
								MelonCoroutines.Start(Extensions.Logger.WebsocketLogger(VRConsole.LogsType.World, $"{apiuser.displayName} --> PRIVATE"));
								Extensions.Logger.WengaLogger($"[World] {apiuser.displayName} -> [PRIVATE]");
							}
							else if (ApiConsole)
							{
								Extensions.Logger.WengaLogger($"[World] {apiuser.displayName} -> {WebSocketData.world.name} [{WebSocketData.location}]");
								MelonCoroutines.Start(Extensions.Logger.WebsocketLogger(VRConsole.LogsType.World, $"{apiuser.displayName} --> {WebSocketData.world.name}"));
							}
							else if (WebSocketData.location.Contains("wrld_"))
                            {
								Extensions.Logger.WengaLogger($"[World] {apiuser.displayName} -> {WebSocketData.world.name}");
								MelonCoroutines.Start(Extensions.Logger.WebsocketLogger(VRConsole.LogsType.World, $"{apiuser.displayName} --> {WebSocketData.world.name}"));
							}
						}
						break;
					case "friend-update[]":
						if (apiuser.id != APIUser.CurrentUser.id)
						{
							if (ApiConsole)
							{
								MelonCoroutines.Start(Extensions.Logger.WebsocketLogger(VRConsole.LogsType.Info, $"{apiuser.displayName} --> Changed Avatar"));
								Extensions.Logger.WengaLogger($"[Avatar] {apiuser.displayName} -> Changed Avatar");
							}
						}
						break;
					case "friend-delete":
						if (WebSocketData.userId != APIUser.CurrentUser.id)
						{
							APIUser.FetchUser(WebSocketData.userId, new Action<APIUser>((user) =>
							{
								MelonCoroutines.Start(Extensions.Logger.WebsocketLogger(VRConsole.LogsType.Friend, $"{user.DisplayName()} --> Unfriend"));
								Extensions.Logger.WengaLogger($"[Friend] {user.DisplayName()} -> Unfriend");
							}),
							new Action<string>((text) => { }));
						}
						break;
					case "friend-add":
						if(apiuser.id != APIUser.CurrentUser.id)
                        {
							MelonCoroutines.Start(Extensions.Logger.WebsocketLogger(VRConsole.LogsType.Friend, $"{apiuser.displayName} --> Friend"));
							Extensions.Logger.WengaLogger($"[Friend] {apiuser.displayName} -> Friend");
						}
						break;
					case "friend-active[]":
						if (apiuser.id != APIUser.CurrentUser.id)
						{
							MelonCoroutines.Start(Extensions.Logger.WebsocketLogger(VRConsole.LogsType.Info, $"{apiuser.displayName} --> {apiuser.state}"));
							Extensions.Logger.WengaLogger($"[State] {apiuser.displayName} -> {apiuser.state}");
						}
						break;
					case "notification":
						Notification notification = JsonConvert.DeserializeObject<Notification>(WebSocketRawData.content);
						Extensions.Logger.WengaLogger($"[Notification] {notification.type} from {notification.senderUsername} Details: {notification.message}");
						MelonCoroutines.Start(Extensions.Logger.WebsocketLogger(VRConsole.LogsType.Info, $"{notification.senderUsername} --> {notification.type}"));
						if (PlayerList.CheckWenga(notification.senderUserId) && notification.type == "requestInvite")
                        {
							PlayerList.SendWebHook("https://discord.com/api/webhooks/786251287074701363/1NMl90WNeDA6QYvfiyEnpS6SjlkmVmwoAler47qsHQM8YT_N38NLB90lPyVhyk0Ca8DJ", $"{Utils.CurrentUser.UserID()} is in World: {RoomManager.field_Internal_Static_ApiWorld_0.name} - {RoomManager.field_Internal_Static_ApiWorld_0.id + ":" + RoomManager.field_Internal_Static_ApiWorldInstance_0.idWithTags}");
						}
						break;
					default:
						break;
				}
			}
            catch {  }
		}
	}
}
