﻿using ExitGames.Client.Photon;
using MelonLoader;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnhollowerBaseLib;
using Utils;
using VRC.Core;

namespace Protection
{
    public static class SetEventLogic
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void EventDelegate(IntPtr thisPtr, IntPtr eventDataPtr, IntPtr nativeMethodInfo);
        private static readonly List<object> _ourPinnedDelegates = new List<object>();
        private static readonly List<ISanitizer> Sanitizers = new List<ISanitizer>();
        public static void InitializeEventLogic()
        {
            IEnumerable<Type> types;
            try
            { types = Assembly.GetExecutingAssembly().GetTypes(); }
            catch (ReflectionTypeLoadException e)
            { types = e.Types.Where(t => t != null); }

            foreach (var t in types)
            {
                if (t.IsAbstract) continue;
                if (!typeof(ISanitizer).IsAssignableFrom(t)) continue;
                var sanitizer = Activator.CreateInstance(t) as ISanitizer;
                Sanitizers.Add(sanitizer);
                SetConsoleColor.WriteEmbeddedColorLine($"Added new Sanitizer: {t.Name}", SetConsoleColor.ConsoleLogType.Action);
            }

            unsafe
            {
                var originalMethodPtr = *(IntPtr*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(typeof(LoadBalancingClient).GetMethod(nameof(LoadBalancingClient.OnEvent))).GetValue(null);
                EventDelegate originalDelegate = null;
                void OnEventDelegate(IntPtr thisPtr, IntPtr eventDataPtr, IntPtr nativeMethodInfo)
                {
                    if (eventDataPtr == IntPtr.Zero)
                    {
                        originalDelegate(thisPtr, eventDataPtr, nativeMethodInfo);
                        return;
                    }
                    try
                    {
                        var eventData = new EventData(eventDataPtr);
                        if (OnEventPatch(new LoadBalancingClient(thisPtr), eventData)) originalDelegate(thisPtr, eventDataPtr, nativeMethodInfo);
                    }
                    catch (Exception ex)
                    {
                        originalDelegate(thisPtr, eventDataPtr, nativeMethodInfo);
                        MelonLogger.Error(ex.Message);
                    }
                }

                var patchDelegate = new EventDelegate(OnEventDelegate);
                _ourPinnedDelegates.Add(patchDelegate);
                MelonUtils.NativeHookAttach((IntPtr)(&originalMethodPtr), Marshal.GetFunctionPointerForDelegate(patchDelegate));
                originalDelegate = Marshal.GetDelegateForFunctionPointer<EventDelegate>(originalMethodPtr);
            }
            unsafe
            {
                var originalMethodPtr = *(IntPtr*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(typeof(VRCNetworkingClient).GetMethod(nameof(VRCNetworkingClient.OnEvent))).GetValue(null);
                EventDelegate originalDelegate = null;
                void OnEventDelegate(IntPtr thisPtr, IntPtr eventDataPtr, IntPtr nativeMethodInfo)
                {
                    if (eventDataPtr == IntPtr.Zero)
                    {
                        originalDelegate(thisPtr, eventDataPtr, nativeMethodInfo);
                        return;
                    }
                    var eventData = new EventData(eventDataPtr);
                    if (VRCNetworkingClientOnPhotonEvent(eventData)) originalDelegate(thisPtr, eventDataPtr, nativeMethodInfo);
                }
                var patchDelegate = new EventDelegate(OnEventDelegate);
                _ourPinnedDelegates.Add(patchDelegate);
                MelonUtils.NativeHookAttach((IntPtr)(&originalMethodPtr), Marshal.GetFunctionPointerForDelegate(patchDelegate));
                originalDelegate = Marshal.GetDelegateForFunctionPointer<EventDelegate>(originalMethodPtr);
            }
        }
        private static bool OnEventPatch(LoadBalancingClient loadBalancingClient, EventData eventData)
        {
            foreach (var sanitizer in Sanitizers) if (sanitizer.OnPhotonEvent(loadBalancingClient, eventData)) return false;  
            return true;
        }
        private static bool VRCNetworkingClientOnPhotonEvent(EventData eventData)
        {
            foreach (var sanitizer in Sanitizers) if (sanitizer.VRCNetworkingClientOnPhotonEvent(eventData)) return false;        
            return true;
        }
    }
}
