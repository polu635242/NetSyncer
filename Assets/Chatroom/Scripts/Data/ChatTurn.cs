using System;
using System.Collections.Generic;

/// <summary>
/// 對話回合。以回合為單位包裝連續的訊息，控制玩家點擊進度與已讀標記。
/// 同一回合內的多條 texts 共用頭像顯示邏輯（同一 from 連續出現只顯示一次頭像）。
/// </summary>
[Serializable]
public class ChatTurn
{
    /// <summary>回合唯一識別碼，如 "t1", "t2"。</summary>
    public string turnId;

    /// <summary>發言者的 userKey，對應 ChatMember。</summary>
    public string from;

    /// <summary>已讀策略。</summary>
    public ChatReadPolicy readPolicy;

    /// <summary>
    /// 本回合包含的聊天內容列表。
    /// 每一項對應一個氣泡 / 貼圖 / 系統訊息 / 選項。
    /// </summary>
    public List<ChatContent> contents;
}
