using System.Collections.Generic;
using UnityEngine;

namespace AI.GOAP.Sensor
{
    public class Sensor : SensorBase
    {
        [SerializeField] private Collider sensorCollider;
        private List<Pawn> _cachedTargets = new();
        protected override void Awake()
        {
            base.Awake();
            if (!sensorCollider)
            {
                sensorCollider = GetComponent<Collider>();
            }

            if (!sensorCollider)
            {
                return;
            }
            
            sensorCollider.isTrigger = true;

            switch (sensorCollider)
            {
                // Optionally, configure dimensions based on the collider type
                case CapsuleCollider capsule:
                    capsule.radius = detectionRadius;
                    break;
                case SphereCollider sphere:
                    sphere.radius = detectionRadius;
                    break;
                case BoxCollider box:
                    box.size = boxSize;
                    break;
            }
        }

        private void RemoveTargetsOutOfRange()
        {
            _cachedTargets.Clear();
            _cachedTargets.AddRange(targets);
            foreach (var target in _cachedTargets)
            {
                if (sensorCollider is SphereCollider sphere)
                {
                    if (Vector3.Distance(SensorOwner.transform.position, target.transform.position) > detectionRadius)
                    {
                        RemoveTarget(target);
                    }
                }
                else
                {
                    if (!(target.transform.position.x >= sensorCollider.bounds.min.x
                                             && target.transform.position.x <= sensorCollider.bounds.max.x
                                             && target.transform.position.z >= sensorCollider.bounds.min.z
                                             && target.transform.position.z <= sensorCollider.bounds.max.z))
                    {
                        RemoveTarget(target);
                    }
                }
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (((1 << other.gameObject.layer) & detectionLayer) == 0)
            {
                return;
            }

            var pawn = other.gameObject.GetComponentInParent<Pawn>();

            if (!pawn)
            {
                return;
            }

            AddTarget(pawn);
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (((1 << other.gameObject.layer) & detectionLayer) == 0)
            {
                return;
            }

            var pawn = other.gameObject.GetComponentInParent<Pawn>();

            if (!pawn)
            {
                return;
            }

            RemoveTarget(pawn);
        }

        public override void UpdateTargets()
        {
            targets.RemoveAll(target => !target);

            RemoveTargetsOutOfRange();
        }
    }
}
