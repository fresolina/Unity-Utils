using System;
using System.Collections;
using Lotec.Utils.Triggers;
using UnityEngine;
using UnityEngine.AI;

// 1. Skapa GuidManager så vi kan referera scene-objekt.
// 2. Referere source till npc:n.
// 3. Referere destination till target position i scenen.
// 4. Animera flyttningen.
// 5. Kanske flytta ut actionet till generic actions.

namespace Lotec.Utils.DialogueSystem {
    [Serializable]
    public class MoveAction : IAction {
        [SerializeField] Transform _source;
        [SerializeField] Transform _destination;
        [SerializeField, Min(0.01f)] float _speed = 2f;
        [SerializeField, Min(0f)] float _stoppingDistance = 0.1f;
        [SerializeField] bool _useNavMeshAgentIfAvailable = true;
        [SerializeField] bool _faceDestinationWhileMoving = true;
        [SerializeField] bool _matchDestinationRotationOnComplete = false;
        [SerializeField, Min(0.1f)] float _rotationSpeed = 720f;

        public IEnumerator Execute() {
            if (_source == null || _destination == null) {
                Debug.LogWarning("MoveAction requires both a source and destination transform.");
                yield break;
            }

            Transform source = _source;
            Vector3 targetPosition = _destination.position;
            Quaternion targetRotation = _destination.rotation;

            if (_useNavMeshAgentIfAvailable && TryMoveWithAgent(source, targetPosition, out NavMeshAgent agent)) {
                // Prefer NavMeshAgent if present so movement respects baked navigation data.
                yield return WaitForAgent(agent);
            } else {
                yield return MoveWithTransform(source, targetPosition);
            }

            if (_matchDestinationRotationOnComplete) {
                source.rotation = targetRotation;
            }
        }

        bool TryMoveWithAgent(Transform source, Vector3 targetPosition, out NavMeshAgent agent) {
            agent = source.GetComponent<NavMeshAgent>();
            if (agent == null || !agent.enabled) return false;

            if (!_faceDestinationWhileMoving) {
                agent.updateRotation = false;
            } else {
                agent.updateRotation = true;
            }

            agent.isStopped = false;
            agent.SetDestination(targetPosition);
            return true;
        }

        IEnumerator WaitForAgent(NavMeshAgent agent) {
            if (agent == null) yield break;

            while (agent.pathPending) {
                yield return null;
            }

            float stopDistance = Mathf.Max(agent.stoppingDistance, _stoppingDistance);
            while (agent.enabled && agent.remainingDistance > stopDistance) {
                yield return null;
            }

            agent.isStopped = true;
        }

        IEnumerator MoveWithTransform(Transform source, Vector3 targetPosition) {
            float stoppingDistanceSqr = _stoppingDistance * _stoppingDistance;

            while ((source.position - targetPosition).sqrMagnitude > stoppingDistanceSqr) {
                Vector3 direction = targetPosition - source.position;
                source.position = Vector3.MoveTowards(source.position, targetPosition, _speed * Time.deltaTime);

                if (_faceDestinationWhileMoving && direction.sqrMagnitude > 0f) {
                    Quaternion lookRotation = Quaternion.LookRotation(direction.normalized);
                    source.rotation = Quaternion.RotateTowards(source.rotation, lookRotation, _rotationSpeed * Time.deltaTime);
                }

                yield return null;
            }

            source.position = targetPosition;
        }
    }
}
