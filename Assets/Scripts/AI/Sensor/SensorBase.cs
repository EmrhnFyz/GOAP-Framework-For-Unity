using System;
using System.Collections.Generic;
using AI.Utils;
using UnityEngine;

namespace AI.GOAP.Sensor
{
    public abstract class SensorBase : MonoBehaviour
    {
        [Flags]
        public enum SensorDetectionType
        {
            Enemy = 1 << 0,
            Friendly = 1 << 1,
            Neutral = 1 << 2,
            Destructible = 1 << 3,
            Gate = 1 << 4,
            All = ~0
        }

        protected Pawn SensorOwner;
        [SerializeField] protected float timerInterval = 1f;
        [SerializeField] protected float detectionRadius = 10f;
        [SerializeField] protected Vector3 boxSize = Vector3.one;
        [SerializeField] protected LayerMask detectionLayer;
        [Tooltip("if this value is true, the sensor only detect when method is called.")]
        [SerializeField] protected bool manualDetection = false;
        [SerializeField] protected SensorDetectionType detectionType = SensorDetectionType.All;
        [SerializeField] protected SensorDetectionType ignoreType;

        [Tooltip("If its not None, only targets with this spawn object type will be detected.")]
        protected List<Pawn> targets = new(128);
        protected CountdownTimer Timer;

        public event Action OnTargetsChanged = delegate { };

        /// <summary>
        /// The transform that the OverlapSensor will track.
        /// </summary>
        protected Transform TrackingTransform;

        public virtual bool HasTargets => targets.Count > 0;
        public IReadOnlyList<Pawn> Targets => targets;

        public void SetTrackingTransform(Transform transform)
        {
            TrackingTransform = transform;
        }

        public void SetDetectionRadius(float radius)
        {
            detectionRadius = radius;
        }

        protected virtual void Awake()
        {
            SensorOwner = GetComponentInParent<Pawn>();
        }

        protected virtual void Start()
        {
            Timer = new CountdownTimer(timerInterval);

            Timer.OnTimerStop += () =>
            {
                if (!manualDetection)
                {
                    UpdateTargets();

                }

                Timer.Start();
            };

            Timer.Start();
        }

        protected virtual void Update()
        {
            Timer.Tick(Time.deltaTime);
        }

        protected bool IsTargetInRange(Pawn pawn)
        {
            return Vector3.Distance(TrackingTransform.position, pawn.transform.position) <= detectionRadius;
        }

        public abstract void UpdateTargets();

        private bool IsValidTarget(Pawn target)
        {
            if (!target || targets.Contains(target))
            {
                return false;
            }
            if (!SensorOwner)
            {
                return false;
            }
    

            bool sameParty =  false; // implementation of sameParty check is not provided in the original code

            bool shouldAdd = detectionType switch
            {
                SensorDetectionType.Enemy => !sameParty,
                SensorDetectionType.Friendly => sameParty,
                SensorDetectionType.Destructible => target.Tags.HasFlag(PawnTags.Destructible),
                SensorDetectionType.Gate => target.Tags.HasFlag(PawnTags.Gate),
                SensorDetectionType.Neutral => target.Tags.HasFlag(PawnTags.Neutral),
                SensorDetectionType.All => true,
                _ => false
            };

            if (shouldAdd && ignoreType != 0)
            {
                shouldAdd = !ignoreType.HasFlag(detectionType);
            }

            return shouldAdd;

        }
        protected void SignalTargetsChanged()
        {
            OnTargetsChanged?.Invoke();
        }

        protected void AddTarget(Pawn target)
        {
            if (!IsValidTarget(target))
            {
                return;
            }

            targets.Add(target);
            OnTargetsChanged?.Invoke();
        }

        protected void RemoveTarget(Pawn target)
        {
            if (!target || !targets.Contains(target))
            {
                return;
            }

            targets.Remove(target);
            OnTargetsChanged?.Invoke();
        }

        public virtual T GetFirstTarget<T>() where T : Component
        {
            if (!HasTargets)
            {
                return null;
            }

            CleanNullTargets();

            targets[0].TryGetComponent(out T component);

            return component;
        }

        /// <summary>
        /// Removes target entries whose key is null.
        /// </summary>
        private void CleanNullTargets()
        {
            List<Pawn> nullValues = new();
            foreach (var target in targets)
            {
                if (!target)
                {
                    nullValues.Add(target);
                }
            }

            foreach (var key in nullValues)
            {
                RemoveTarget(key);
            }
        }

        public virtual Vector3 GetClosestTargetPosition()
        {
            if (!HasTargets)
            {
                return Vector3.zero;
            }

            CleanNullTargets();
            var closestDistance = float.MaxValue;
            var closestPosition = Vector3.zero;

            foreach (var target in targets)
            {
                var distance = Vector3.Distance(TrackingTransform.position, target.transform.position);
                if (!(distance < closestDistance))
                {
                    continue;
                }
                
                closestDistance = distance;
                closestPosition = target.transform.position;
            }

            return closestPosition;
        }

        public virtual T GetClosestTarget<T>() where T : Component
        {
            if (!HasTargets)
            {
                return null;
            }
            CleanNullTargets();
            var closestDistance = float.MaxValue;
            T closestTarget = null;

            foreach (var target in targets)
            {
                if (!target.TryGetComponent(out T component))
                {
                    continue;
                }

                var distance = Vector3.Distance(TrackingTransform.position, target.transform.position);

                if (!(distance < closestDistance))
                {
                    continue;
                }
                closestDistance = distance;
                closestTarget = component;
            }

            return closestTarget;
        }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            Gizmos.color = HasTargets ? Color.red : Color.green;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
#endif
    }
}
