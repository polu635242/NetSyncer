/// <summary>
/// 單一聊天階段的執行期狀態。
/// 紀錄播放進度，支援中途插斷後恢復。
/// </summary>
public class ChatStageState
{
    /// <summary>對應的 stageKey。</summary>
    public string StageKey { get; private set; }

    /// <summary>當前播放到的回合索引。</summary>
    public int CurrentTurnIndex { get; set; }

    /// <summary>當前回合內播放到的內容索引。</summary>
    public int CurrentContentIndex { get; set; }

    /// <summary>已讀的內容總數（用於部分已讀判定與紅點顯示）。</summary>
    public int ReadContentCount { get; set; }

    /// <summary>內容總數（所有回合的 contents 加總）。</summary>
    public int TotalContentCount { get; set; }

    /// <summary>
    /// 是否正在播放中。
    /// True: 尚未全部顯示完畢。
    /// False: 最後一筆已渲染完成。
    /// </summary>
    public bool IsProcessing { get; set; }

    /// <summary>
    /// 閱讀狀態。
    /// </summary>
    public ChatReadState ReadState
    {
        get
        {
            if (ReadContentCount >= TotalContentCount)
                return ChatReadState.Read;
            if (ReadContentCount > 0)
                return ChatReadState.PartialRead;
            return ChatReadState.Unread;
        }
    }

    public ChatStageState(string stageKey, int totalContentCount)
    {
        StageKey = stageKey;
        TotalContentCount = totalContentCount;
        CurrentTurnIndex = 0;
        CurrentContentIndex = 0;
        ReadContentCount = 0;
        IsProcessing = false;
    }

    /// <summary>
    /// 從預設已讀數量初始化（用於已有舊訊息的場景）。
    /// </summary>
    public void ApplyPreRead(int preReadCount)
    {
        if (preReadCount <= 0) return;

        ReadContentCount = preReadCount < TotalContentCount ? preReadCount : TotalContentCount;
    }
}
