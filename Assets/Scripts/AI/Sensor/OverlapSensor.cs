using System.Collections.Generic;
using UnityEngine;

namespace AI.GOAP.Sensor
{
    public class OverlapSensor : SensorBase
    {
        private const int MaxColliders = 128;
        private Collider[] _colliders = new Collider[MaxColliders];
        private List<Pawn> _targetsCopy = new();
        public void RemoveOldTargetsOutOfRange()
        {
            targets.RemoveAll(target => !IsTargetInRange(target));
            SignalTargetsChanged();
        }

        public void ClearTargets()
        {
            targets.Clear();
            SignalTargetsChanged();
        }

        public override void UpdateTargets()
        {
            if (!TrackingTransform)
            {
                return;
            }

            int numberOfColliders = Physics.OverlapSphereNonAlloc(TrackingTransform.position, detectionRadius, _colliders, detectionLayer);
            ClearTargets();

            for (int i = 0; i < numberOfColliders; i++)
            {
                if (_colliders[i])
                {
                    Pawn pawn = _colliders[i].gameObject.GetComponentInParent<Pawn>();
                    if (!targets.Contains(pawn))
                    {
                        AddTarget(pawn);
                    }
                }
            }
        }

        public void UpdateTargetsWithGivenPosition(Vector3 position)
        {
            int numberOfColliders = Physics.OverlapSphereNonAlloc(position, detectionRadius, _colliders, detectionLayer);
            ClearTargets();

            for (int i = 0; i < numberOfColliders; i++)
            {
                if (_colliders[i] != null)
                {
                    Pawn pawn = _colliders[i].gameObject.GetComponentInParent<Pawn>();
                    if (!targets.Contains(pawn))
                    {
                        AddTarget(pawn);
                    }
                }
            }
        }

        /// <summary>
        /// Check if the target is in range of the sensor
        /// </summary>
        /// <param name="trackingTransform">Transform that should be used as the center of the detection area</param>
        /// <param name="detectionRadius"> Radius of the detection area</param>
        /// <returns></returns>
        public bool HasTargetsInArea(Transform trackingTransform, float detectionRadius)
        {
            SetTrackingTransform(trackingTransform);
            SetDetectionRadius(detectionRadius);
            UpdateTargets();
            bool hasTargets = HasTargets;
            ClearTargets();
            return hasTargets;
        }

        /// <summary>
        /// Get the targets in the detection area
        /// </summary>
        /// <param name="trackingTransform">Transform that should be used as the center of the detection area</param>
        /// <param name="detectionRadius"> Radius of the detection area</param>
        /// <returns></returns>
        public List<Pawn> GetTargetsInArea(Transform trackingTransform, float detectionRadius)
        {
            _targetsCopy.Clear();
            SetTrackingTransform(trackingTransform);
            SetDetectionRadius(detectionRadius);
            UpdateTargets();
            _targetsCopy.AddRange(targets);
            ClearTargets();
            return _targetsCopy;
        }


        /// <summary>
        /// Check if the target is in range of the sensor
        /// </summary>
        /// <param name="trackingTransform">Transform that should be used as the center of the detection area</param>
        /// <param name="detectionRadius"> Radius of the detection area</param>
        /// <returns></returns>
        public bool HasTargetsInArea(Vector3 position, float detectionRadius)
        {
            SetDetectionRadius(detectionRadius);
            UpdateTargetsWithGivenPosition(position);
            bool hasTargets = HasTargets;
            ClearTargets();
            return hasTargets;
        }

        /// <summary>
        /// Get the targets in the detection area
        /// </summary>
        /// <param name="trackingTransform">Transform that should be used as the center of the detection area</param>
        /// <param name="detectionRadius"> Radius of the detection area</param>
        /// <returns></returns>
        public List<Pawn> GetTargetsInArea(Vector3 position, float detectionRadius)
        {
            _targetsCopy.Clear();
            SetDetectionRadius(detectionRadius);
            UpdateTargetsWithGivenPosition(position);
            _targetsCopy.AddRange(targets);
            ClearTargets();
            return _targetsCopy;
        }

#if UNITY_EDITOR
        protected override void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (TrackingTransform != null)
            {
                Gizmos.color = HasTargets ? Color.red : Color.green;
                Gizmos.DrawWireSphere(TrackingTransform.position, detectionRadius);
            }
        }
#endif
    }
}
