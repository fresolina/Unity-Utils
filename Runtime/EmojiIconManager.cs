using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lotec.Utils {
    /// <summary>
    /// Converts emoji names to unicode characters.
    /// Usage: EmojiIconManager.GetEmoji("grinning face");
    /// Data source:
    /// https://github.com/muan/unicode-emoji-json/blob/main/data-by-group.json
    /// Example emoji font: https://github.com/googlefonts/noto-emoji
    /// </summary>
    public class EmojiIconManager : MonoBehaviour2 {
        static EmojiIconManager s_instance;
        readonly Dictionary<string, string> _emojiFromName = new Dictionary<string, string>();

        public static string GetEmoji(string name) => s_instance.GetEmojiFromText(name);
        public string GetEmojiFromText(string name) => _emojiFromName.TryGetValue(name.ToLower(), out var emoji) ? emoji : string.Empty;

        void Awake() {
            if (s_instance == null) {
                s_instance = this;
            }
            LoadEmojiData();
        }

        bool LoadEmojiData() {
            // Load json file with emoji mappings
            var json = Resources.Load<TextAsset>("data-by-group");
            if (json == null) {
                Debug.LogError("Emoji data not found: Need data-by-group.json in Resources folder.");
                return false;
            }
            // Hack to fix json format. TODO: Add utility to download file + fix asset instead.
            string jsonText = "{\"groups\":" + json.text + "}";
            EmojiGroupList list = JsonUtility.FromJson<EmojiGroupList>(jsonText);
            // Remap emoji names to unicode
            foreach (var group in list.groups) {
                foreach (var emoji in group.emojis) {
                    _emojiFromName[emoji.name.ToLower()] = emoji.emoji;
                }
            }

            return true;
        }

        /// <summary>
        /// Emoji data format:
        /// [
        //   {
        //     "name": "Smileys & Emotion",
        //     "slug": "smileys_emotion",
        //     "emojis": [
        //       {
        //         "emoji": "😀",
        //         "skin_tone_support": false,
        //         "name": "grinning face",
        //         "slug": "grinning_face",
        //         "unicode_version": "1.0",
        //         "emoji_version": "1.0"
        //       },
        //       ...
        //     ]
        //   },
        //   ...
        // ]
        /// </summary>
        [Serializable]
        class EmojiGroupList {
            public EmojiGroup[] groups;
        }
        [Serializable]
        class EmojiGroup {
            public string name;
            public string slug;
            public EmojiData[] emojis;
        }
        [Serializable]
        class EmojiData {
            public string emoji;
            public bool skin_tone_support;
            public string name;
            public string slug;
            public string unicode_version;
            public string emoji_version;
        }
    }
}
