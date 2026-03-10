using System;
using System.Collections.Generic;

/// <summary>
/// 對話階段資料包。描述一個聊天室的完整對話內容。
/// </summary>
[Serializable]
public class ChatStageData
{
    /// <summary>本階段的主鍵值。</summary>
    public string stageKey;

    /// <summary>聊天室顯示名稱（多語系 Key）。</summary>
    public string stageDisplayName;

    /// <summary>佈局類型：密語或群聊。</summary>
    public ChatLayoutType layoutType;

    /// <summary>是否為強制閱讀（未完成閱讀無法離開事件）。</summary>
    public bool forceRead;

    /// <summary>預設已讀數量（進入時已經存在的舊訊息數量）。</summary>
    public int preReadCount;

    /// <summary>本階段所有參與者身分定義。</summary>
    public List<ChatMember> header;

    /// <summary>按順序排列的對話回合列表。</summary>
    public List<ChatTurn> turns;
}
