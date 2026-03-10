using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 事件資源。代表一次劇情事件中玩家可閱覽的聊天室清單。
/// 如：拿出手機後，可以看到有三個聊天室可以看。
/// </summary>
[CreateAssetMenu(fileName = "NewChatEvent", menuName = "Chatroom/Chat Event Asset")]
public class ChatEventAsset : ScriptableObject
{
    /// <summary>事件編號，供 AVG 接口直接呼叫。</summary>
    public string eventId;

    /// <summary>
    /// 按順序排列的聊天室清單。
    /// 第一個聊天室 = 打開時自動顯示該對話，通常為強制閱讀。
    /// </summary>
    public List<ChatEventEntry> entries;
}

/// <summary>
/// 事件中的聊天室條目。
/// </summary>
[Serializable]
public class ChatEventEntry
{
    /// <summary>關聯的聊天階段資源。</summary>
    public ChatStageAsset stageAsset;

    /// <summary>在事件清單中的排序權重，數值越小越靠前。</summary>
    public int sortOrder;
}
