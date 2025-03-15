using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lotec.Utils {
    /// <summary>
    /// Converts emoji names to unicode characters.
    /// Usage: EmojiIconManager.GetEmoji("grinning face");
    /// Data source: https://github.com/muan/unicode-emoji-json/blob/main/data-by-group.json
    /// Example emoji font: https://github.com/googlefonts/noto-emoji
    /// </summary>
    public static class EmojiIconManager {
        static readonly Dictionary<string, string> s_emojiFromName = new Dictionary<string, string>();
        static bool s_isInitialized;

        static EmojiIconManager() {
            Initialize();
        }

        public static string GetEmoji(string name) {
            if (!s_isInitialized)
                Initialize();
            return s_emojiFromName.TryGetValue(name.ToLower(), out var emoji) ? emoji : string.Empty;
        }

        static void Initialize() {
            if (s_isInitialized)
                return;

            var json = Resources.Load<TextAsset>("data-by-group");
            if (json == null) {
                Debug.LogError("Emoji data not found: Need data-by-group.json in Resources folder.");
                return;
            }
            // Hack to fix json format. TODO: Add utility to download file + fix asset instead.
            string jsonText = "{\"groups\":" + json.text + "}";
            EmojiGroupList list = JsonUtility.FromJson<EmojiGroupList>(jsonText);
            foreach (var group in list.groups) {
                foreach (var emoji in group.emojis) {
                    s_emojiFromName[emoji.name.ToLower()] = emoji.emoji;
                }
            }

            s_isInitialized = true;
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
