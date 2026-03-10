/// <summary>
/// 聊天內容類型。
/// msg=泡泡框對話, texture=貼圖, system=系統文字(置中), selector=分岐選擇
/// </summary>
public enum ChatContentType
{
    Msg,
    Texture,
    System,
    Selector
}

/// <summary>
/// 訊息傳送狀態。
/// Normal=已送達(V), Failed=傳送失敗(!)
/// </summary>
public enum ChatMessageStatus
{
    Normal,
    Failed
}

/// <summary>
/// 聊天室佈局類型。
/// Private=密語(對稱式), Group=群聊(流式)
/// </summary>
public enum ChatLayoutType
{
    Private,
    Group
}

/// <summary>
/// 對話階段的閱讀狀態。
/// Unread=未讀(紅點), PartialRead=部分已讀, Read=已讀(V), ForceUnread=強制閱讀未完成(!)
/// </summary>
public enum ChatReadState
{
    Unread,
    PartialRead,
    Read,
    ForceUnread
}

/// <summary>
/// 回合已讀策略。
/// Immediate=立即標記已讀, OnNextTurn=進入下一回合時標記已讀
/// </summary>
public enum ChatReadPolicy
{
    Immediate,
    OnNextTurn
}
