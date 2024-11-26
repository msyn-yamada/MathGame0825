﻿using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;
using SO;
using System.Collections.Generic;

namespace Main.Handler
{
    internal sealed class SceneryMover : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer elementPrefab;
        [SerializeField] private Transform parent;

        private SceneryMoverImpl whiteLineImpl;
        private SceneryMoverImpl buildingsLeftImpl;
        private SceneryMoverImpl buildingsRightImpl;

        private void OnEnable()
        {
            InstantiateThis(ref whiteLineImpl, SO_Scenery.Entity.WhitelineProperty);
            InstantiateThis(ref buildingsLeftImpl, SO_Scenery.Entity.BuildingsLeftProperty);
            InstantiateThis(ref buildingsRightImpl, SO_Scenery.Entity.BuildingsRightProperty);

            void InstantiateThis(ref SceneryMoverImpl impl, SceneryElementProperty property)
            {
                if (impl != null) return;
                if (property == null) return;

                LinkedList<SceneryElement> elements = new();

                for (int i = 0; i < 5; i++)
                {
                    SpriteRenderer instance = Instantiate(elementPrefab, parent);
                    if (instance == null) continue;
                    SceneryElementReference reference = new(instance.transform, instance);
                    SceneryElement element = new(reference, property);
                    elements.AddLast(element);
                }

                impl = new(elements.ToArray());
            }
        }

        private void OnDisable()
        {
            whiteLineImpl?.Dispose();
            buildingsLeftImpl?.Dispose();
            buildingsRightImpl?.Dispose();

            whiteLineImpl = null;
            buildingsLeftImpl = null;
            buildingsRightImpl = null;
        }

        private void Update()
        {
            whiteLineImpl?.Update();
            buildingsLeftImpl?.Update();
            buildingsRightImpl?.Update();
        }
    }

    internal sealed class SceneryMoverImpl : IDisposable
    {
        private CancellationTokenSource cts = new();
        private SceneryElement[] elements;
        private bool isFirstUpdate = true;

        internal SceneryMoverImpl(SceneryElement[] elements) => this.elements = elements;

        public void Dispose()
        {
            cts.Cancel();
            cts.Dispose();
            cts = null;

            if (elements != null) foreach (var e in elements) e.Dispose();
            Array.Clear(elements, 0, elements.Length);
        }

        internal void Update()
        {
            if (isFirstUpdate)
            {
                isFirstUpdate = false;

                CreateElements(elements, cts.Token).Forget();
            }

            foreach (var e in elements) e.Update();
        }

        private async UniTask CreateElements(SceneryElement[] elements, CancellationToken ct)
        {
            if (elements == null) return;

            int len = elements.Length;
            if (len <= 0) return;
            int i = 0;
            while (true)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(elements[0].Interval), cancellationToken: ct);
                elements[i].IsActive = true;
                i = Looped(++i, 0, len - 1);
            }
        }

        private int Looped(int value, int min, int max)
        {
            if (max <= min) return value;

            int len = max - min + 1;
            while (value < min) value += len;
            while (max < value) value -= len;
            return value;
        }
    }

    internal sealed class SceneryElement : IDisposable
    {
        private SceneryElementReference reference;
        private SceneryElementProperty property;
        internal float Interval => property.Interval;

        internal bool IsActive { get; set; } = false;

        private float t = 0;

        internal SceneryElement(SceneryElementReference reference, SceneryElementProperty property)
        {
            if (reference == null) return;
            if (property == null) return;
            this.reference = reference;
            this.property = property;

            Sprite sprite = property.Sprite;
            if (sprite == null) return;
            reference.Sprite = sprite;
            Init();
        }

        public void Dispose()
        {
            reference.Dispose();

            reference = null;
            property = null;
        }

        internal void Update()
        {
            if (!IsActive)
            {
                if (reference.IsActive) reference.IsActive = false;
            }
            else
            {
                if (!reference.IsActive) reference.IsActive = true;

                reference.Position = reference.Position + CalcVelocity(t) * Time.deltaTime;
                reference.LocalScale = CalcLocalScale(t);

                t += Time.deltaTime;
                if (t >= property.Duration)
                {
                    t = 0;
                    Init();
                }
            }
        }

        private void Init()
        {
            reference.Position = property.StartPosition;
            reference.LocalScale = Vector3.one * property.StartLocalScale;
            IsActive = false;
        }

        private Vector3 CalcVelocity(float t)
        {
            return property.StartVelocity.normalized * (t * t * property.VelocityCoefficient) + property.StartVelocity;
        }

        private Vector3 CalcLocalScale(float t)
        {
            float s = t * property.ScaleCoefficient + property.StartLocalScale;
            return new(s, s, 1);
        }
    }

    [Serializable]
    internal sealed class SceneryElementReference : IDisposable
    {
        [SerializeField] private Transform transform;
        [SerializeField] private SpriteRenderer spriteRenderer;

        internal SceneryElementReference(Transform transform, SpriteRenderer spriteRenderer)
        {
            if (transform == null) return;
            if (spriteRenderer == null) return;

            this.transform = transform;
            this.spriteRenderer = spriteRenderer;
        }

        internal Vector3 Position
        {
            get
            {
                if (transform == null) return default;
                return transform.position;
            }
            set
            {
                if (transform == null) return;
                transform.position = value;
            }
        }

        internal Vector3 LocalScale
        {
            get
            {
                if (transform == null) return default;
                return transform.localScale;
            }
            set
            {
                if (transform == null) return;
                transform.localScale = value;
            }
        }

        internal bool IsActive
        {
            get
            {
                if (spriteRenderer == null) return false;
                return spriteRenderer.enabled;
            }
            set
            {
                if (spriteRenderer == null) return;
                spriteRenderer.enabled = value;
            }
        }

        internal Sprite Sprite
        {
            set
            {
                if (spriteRenderer == null) return;
                spriteRenderer.sprite = value;
            }
        }

        public void Dispose()
        {
            transform = null;
            spriteRenderer = null;
        }
    }
}