using System;
using System.Collections.Generic;

/// <summary>
/// 單一聊天內容項目。對應一則訊息、貼圖、系統提示或選項。
/// </summary>
[Serializable]
public class ChatContent
{
    /// <summary>關聯至 ChatMember.userKey。system 類型時可為空。</summary>
    public string userKey;

    /// <summary>內容類型。</summary>
    public ChatContentType type;

    /// <summary>
    /// 內容值。
    /// msg: 文字內容或多語系 Key。
    /// texture: 貼圖路徑或 Key。
    /// system: 系統文字 Key。
    /// selector: 不使用此欄位，改用 selectorOptions。
    /// </summary>
    public string contentValue;

    /// <summary>訊息傳送狀態，預設為 Normal(V)，標記 Failed 時顯示(!)。</summary>
    public ChatMessageStatus status;

    /// <summary>selector 類型專用：分岐選項列表。</summary>
    public List<ChatSelectorOption> selectorOptions;
}

/// <summary>
/// 分岐選項。
/// </summary>
[Serializable]
public class ChatSelectorOption
{
    /// <summary>選項顯示文字 Key。</summary>
    public string textKey;

    /// <summary>選擇後跳轉的 turnId 或外部標識。</summary>
    public string targetTurnId;
}
