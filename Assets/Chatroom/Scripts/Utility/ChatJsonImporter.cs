using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// JSON 匯入工具。將企劃編寫的 JSON 檔案轉換為 ChatStageData。
/// 
/// 預期 JSON 格式範例:
/// {
///   "stageKey": "stage_01",
///   "stageDisplayName": "STAGE_01_NAME",
///   "layoutType": "Group",
///   "forceRead": true,
///   "preReadCount": 5,
///   "header": [
///     { "userKey": "player", "nameKey": "PLAYER_NAME", "iconKey": "icon_player", "isPlayer": true },
///     { "userKey": "skuld",  "nameKey": "SKULD_NAME",  "iconKey": "icon_skuld",  "isPlayer": false }
///   ],
///   "turns": [
///     {
///       "turnId": "t1",
///       "from": "skuld",
///       "readPolicy": "OnNextTurn",
///       "contents": [
///         { "type": "Msg", "contentValue": "TEXT_KEY_001", "status": "Normal" },
///         { "type": "Msg", "contentValue": "TEXT_KEY_002", "status": "Failed" }
///       ]
///     }
///   ]
/// }
/// </summary>
public static class ChatJsonImporter
{
    /// <summary>
    /// 從 JSON 字串解析為 ChatStageData。
    /// </summary>
    public static ChatStageData Import(string json)
    {
        var raw = JsonUtility.FromJson<ChatStageDataJson>(json);
        return raw.ToChatStageData();
    }

    // ──────────────────────────────────────────────
    //  JSON 映射用中間結構（因 JsonUtility 不支援 Enum 字串直接反序列化）
    // ──────────────────────────────────────────────

    [Serializable]
    private class ChatStageDataJson
    {
        public string stageKey;
        public string stageDisplayName;
        public string layoutType;
        public bool forceRead;
        public int preReadCount;
        public List<ChatMember> header;
        public List<ChatTurnJson> turns;

        public ChatStageData ToChatStageData()
        {
            var data = new ChatStageData
            {
                stageKey = stageKey,
                stageDisplayName = stageDisplayName,
                layoutType = ParseEnum(layoutType, ChatLayoutType.Group),
                forceRead = forceRead,
                preReadCount = preReadCount,
                header = header ?? new List<ChatMember>(),
                turns = new List<ChatTurn>()
            };

            if (turns != null)
            {
                foreach (var t in turns)
                    data.turns.Add(t.ToChatTurn());
            }

            return data;
        }
    }

    [Serializable]
    private class ChatTurnJson
    {
        public string turnId;
        public string from;
        public string readPolicy;
        public List<ChatContentJson> contents;

        public ChatTurn ToChatTurn()
        {
            var turn = new ChatTurn
            {
                turnId = turnId,
                from = from,
                readPolicy = ParseEnum(readPolicy, ChatReadPolicy.Immediate),
                contents = new List<ChatContent>()
            };

            if (contents != null)
            {
                foreach (var c in contents)
                    turn.contents.Add(c.ToChatContent(from));
            }

            return turn;
        }
    }

    [Serializable]
    private class ChatContentJson
    {
        public string type;
        public string contentValue;
        public string status;
        public List<ChatSelectorOption> selectorOptions;

        public ChatContent ToChatContent(string fromUserKey)
        {
            return new ChatContent
            {
                userKey = fromUserKey,
                type = ParseEnum(type, ChatContentType.Msg),
                contentValue = contentValue,
                status = ParseEnum(status, ChatMessageStatus.Normal),
                selectorOptions = selectorOptions
            };
        }
    }

    private static T ParseEnum<T>(string value, T defaultValue) where T : struct
    {
        if (string.IsNullOrEmpty(value)) return defaultValue;
        if (Enum.TryParse<T>(value, true, out var result)) return result;
        return defaultValue;
    }
}
