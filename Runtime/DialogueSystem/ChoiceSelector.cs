using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lotec.Utils.DialogueSystem {
    public interface IChoiceSelector {
        /// <summary>
        /// Tells the choice selector to show choices and run callback when player has selected a choice.
        /// </summary>
        /// <param name="choices"></param>
        /// <param name="selected">Action to run after choice was selected</param>
        /// <typeparam name="T"></typeparam>
        void RequestChoice<T>(List<T> choices, System.Action<T> selected);
        /// <summary>
        /// Programatically select the choice. Good for testing.
        /// </summary>
        /// <param name="idx"></param>
        void Select(int idx);
    }

    /// <summary>
    /// Execute callback when button clicked on item.
    /// </summary>
    ///
    public class ChoiceSelector : MonoBehaviour, IChoiceSelector {
        [SerializeField] bool _prefixWithNumber = true;
        [SerializeField] Button[] _buttons;

        public bool IsVisible { get; private set; }
        public Button[] Buttons => _buttons;

        public int ActiveButtons { get; private set; } = 0;

        void Start() {
            for (int i = 0; i < _buttons.Length; i++) {
                _buttons[i].gameObject.SetActive(false);
            }
        }

        // IChoiceSelector
        /// <summary>
        /// * Setup choices
        /// * Show UI
        /// * Wait for selection
        /// </summary>
        /// <typeparam name="T">Type of choices</typeparam>
        /// <param name="choices"></param>
        /// <param name="selected">Action to run after selection</param>
        public void RequestChoice<T>(List<T> choices, System.Action<T> selected) {
            Clear();
            for (int i = 0; i < choices.Count; i++) {
                AddItem(choices[i], selected);
            }
            UpdateVisibility();
        }

        // Simulates mouse click on the button.
        public void Select(int idx) {
            if (_buttons.Length >= idx + 1 && _buttons[idx].IsActive()) {
                _buttons[idx].onClick.Invoke();
            }
        }

        /// <summary>
        /// Add a printable item to the list.
        /// </summary>
        /// <param name="item">Should override ToString() with what is to be shown.</param>
        /// <param name="clicked">Callback method called after button was clicked.</param>
        /// <returns>If item was added to list. If the list was full, it can't be added.</returns>
        bool AddItem<T>(T item, System.Action<T> clicked) {
            // Add text to next available button.
            for (int i = 0; i < _buttons.Length; i++) {
                if (_buttons[i].gameObject.activeSelf) continue;

                _buttons[i].gameObject.SetActive(true);
                if (_prefixWithNumber) {
                    _buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = $"{i + 1}. {item}";
                } else {
                    _buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = item.ToString();
                }
                _buttons[i].onClick.RemoveAllListeners();
                _buttons[i].onClick.AddListener(() => { clicked(item); Close(); });
                ActiveButtons++;
                return true;
            }
            return false;
        }

        void Close() {
            Clear();
            UpdateVisibility();
        }

        void Clear() {
            for (int i = 0; i < _buttons.Length; i++) {
                _buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = string.Empty;
                _buttons[i].gameObject.SetActive(false);
            }
            ActiveButtons = 0;
        }

        void UpdateVisibility() {
            if (!gameObject.activeSelf && ActiveButtons > 0) {
                gameObject.SetActive(true);
                IsVisible = true;
            } else if (gameObject.activeSelf && ActiveButtons == 0) {
                gameObject.SetActive(false);
                IsVisible = false;
            }
            // This will redraw the UI. Apparently Unity doesn't guarantee it automatically. :/
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }

        // Editor
        void OnValidate() {
            Transform group = GetComponentInChildren<VerticalLayoutGroup>().transform;
            _buttons = new Button[group.childCount];
            for (int i = 0; i < group.childCount; i++) {
                Transform t = group.GetChild(i);
                _buttons[i] = t.GetComponent<Button>();
            }
        }
    }
}
