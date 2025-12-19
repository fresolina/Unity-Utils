using System;
using System.Collections;
using System.Collections.Generic;
using Lotec.Utils.Attributes;
using UnityEngine;

namespace Lotec.Utils.DialogueSystem {
    public class DialogueManager : MonoBehaviour2 {
        [SerializeField, NotNull] IChoiceSelector _choiceSelector;
        [SerializeField, NotNull] IText _output;
        [SerializeField] bool _autoAdvanceOnSingleOption = true;
        [SerializeField] bool _prefixTextWithActorName = true;

        public event Action DialogueEnded;
        public DialogueActor CurrentSpeaker { get; private set; }

        DialogueActor _currentTarget;
        DialogueActor _currentInitiator;
        DialogueNode _currentNode;
        readonly List<DialogueOption> _validOptions = new List<DialogueOption>();
        Coroutine _speakRoutine;

        public void StartDialogue(DialogueActor target, DialogueActor initiator) {
            _currentTarget = target;
            _currentInitiator = initiator;
            _currentNode = _currentTarget.StartNode;
            SelectOption();
        }

        public void EndDialogue() {
            Debug.Log("[DialogueManager] EndDialogue");
            if (_speakRoutine != null) {
                StopCoroutine(_speakRoutine);
                _speakRoutine = null;
            }
            _currentNode = null;
            _currentTarget = null;
            _currentInitiator = null;
            CurrentSpeaker = null;
            DialogueEnded?.Invoke();
        }

        void SelectOption() {
            UpdateValidOptions(_currentNode);

            if (_validOptions.Count == 0) {
                EndDialogue();
                return;
            }

            var nextActor = ResolveSpeaker(_validOptions[0].NextNode);

            // Auto-select first choice if there is only one, or if next actor is the same.
            if ((_autoAdvanceOnSingleOption && _validOptions.Count == 1)
                    || _validOptions[0].IsAutomatic
                    || _currentNode.SpeakerSlot == _validOptions[0].NextNode.SpeakerSlot) {
                SetNode(_validOptions[0].NextNode);
            }

            // Present multiple choices to player
            if (nextActor.IsPlayer)
                PresentOptionsToPlayer();
        }

        void PresentOptionsToPlayer() {
            // Multi-choice request
            // Debug.Log("Presenting options to player:");
            _choiceSelector.RequestChoice(_validOptions, (option) => {
                // Debug.Log($"Player selected button option: {option.OptionText}");
                SetNode(option.NextNode);
            });
        }

        void UpdateValidOptions(DialogueNode currentNode) {
            _validOptions.Clear();
            for (int i = 0; i < currentNode.Options.Count; i++) {
                var option = currentNode.Options[i];
                if (option.Condition == null || option.Condition.IsMet()) {
                    _validOptions.Add(option);
                }
            }
        }

        void SetNode(DialogueNode node) {
            // Debug.Log($"[DialogueManager] SetNode: {node.name}");
            _currentNode = node;
            CurrentSpeaker = ResolveSpeaker(node);
            StartSpeaking(CurrentSpeaker, _currentNode);
        }

        void StartSpeaking(DialogueActor actor, DialogueNode node) {
            if (_speakRoutine != null) {
                StopCoroutine(_speakRoutine);
            }
            _speakRoutine = StartCoroutine(SpeakRoutine(actor, node));
        }

        IEnumerator SpeakRoutine(DialogueActor actor, DialogueNode node) {
            // Speak text
            string textToSpeak = _prefixTextWithActorName ? $"{actor.name}: {node.Text}" : node.Text;
            yield return StartCoroutine(_output.SetTextRoutine(textToSpeak));
            _speakRoutine = null;

            // Execute action and wait for it to finish
            if (_currentNode.ActionOnSpoke != null) {
                yield return StartCoroutine(_currentNode.ActionOnSpoke.Execute());
            }

            // Proceed to next option
            SelectOption();
        }

        DialogueActor ResolveSpeaker(DialogueNode node) {
            if (node == null) {
                return null;
            }

            if (node.SpeakerSlot == DialogueSpeakerSlot.Custom) {
                return node.CustomActor;
            }

            return node.SpeakerSlot switch {
                DialogueSpeakerSlot.Initiator => _currentInitiator,
                DialogueSpeakerSlot.Target => _currentTarget,
                _ => node.CustomActor,
            };
        }
    }
}
