﻿using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

namespace General.Extension
{
    internal enum ClientMode
    {
        Editor_Editing,
        Editor_Playing,
        Build
    }

    internal static class Extension
    {
        internal static ClientMode GetClientMode()
        {
#if UNITY_EDITOR
            return UnityEditor.EditorApplication.isPlaying ? ClientMode.Editor_Playing : ClientMode.Editor_Editing;
#else
            return ClientMode.Build;
#endif
        }

        internal static async UniTask SecondsWaitAndDo(this float waitSeconds, Action act, CancellationToken ct)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(waitSeconds), cancellationToken: ct);
            act();
        }

        internal static async UniTask SecondsWait(this float waitSeconds, CancellationToken ct)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(waitSeconds), cancellationToken: ct);
        }

        /// <summary>
        /// EventTriggerにイベントを登録する
        /// </summary>
        internal static void AddListener(this EventTrigger eventTrigger, EventTriggerType type, Action action)
        {
            EventTrigger.Entry entry = new() { eventID = type };
            entry.callback.AddListener(_ => { action(); });
            eventTrigger.triggers.Add(entry);
        }

        internal static float Remap(this float x, float a, float b, float c, float d) => (x - a) * (d - c) / (b - a) + c;

        internal static bool IsClose(this float a, float b, float ofst = float.Epsilon) => MathF.Abs(a - b) < ofst;

        internal static bool IsIn(this int val, int min, int max, int ofst = default)
            => min + ofst <= val && val <= max + ofst;
        internal static bool IsIn(this float val, float min, float max, float ofst = default)
            => min + ofst <= val && val <= max + ofst;
        internal static bool IsIn(this Vector2 v, float sx, float ex, float sy, float ey, Vector2 ofst = default)
            => v.x.IsIn(sx, ex, ofst.x) && v.y.IsIn(sy, ey, ofst.y);

        internal static Vector2 ToVector2(this Vector3 v) => new(v.x, v.y);
        internal static Vector3 ToVector3(this Vector2 v, float z) => new(v.x, v.y, z);

        internal static void Pass()
        {
            return;
        }
    }

    internal static class IteratorExtension
    {
        internal static bool All<T>(this T val, params Func<T, bool>[] functions)
        {
            foreach (var f in functions)
            {
                if (!f(val)) return false;
            }
            return true;
        }

        internal static bool Any<T>(this T val, params Func<T, bool>[] functions)
        {
            foreach (var f in functions)
            {
                if (f(val)) return true;
            }
            return false;
        }

        internal static (T Element, int Index, bool IsFound) Find<T>(this IEnumerable<T> itr, Func<T, bool> f)
        {
            int i = 0;
            foreach (T e in itr)
            {
                if (f(e)) return (e, i, true);
                i++;
            }

            return (default, -1, false);
        }
    }

    internal interface INullExistable
    {
        bool IsNullExist();
    }
}