using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 聊天室核心控制器。
/// 管理對話流的顯示進度，支援多階段切換、中途插斷與恢復。
/// 使用 Update 狀態機驅動播放流程，不使用 Coroutine。
/// </summary>
public class Chatroom : MonoBehaviour
{
    /// <summary>播放狀態機的內部階段。</summary>
    private enum PlaybackPhase
    {
        Idle,
        BeginTurn,
        DisplayContent,
        WaitingForClick,
        WaitingForSelector,
        AdvanceContent,
        AdvanceTurn,
        StageEnd
    }

    /// <summary>當某階段全部內容顯示完畢時觸發，參數為 stageKey。</summary>
    public event Action<string> OnStageEnd;

    /// <summary>當某回合開始播放時觸發，參數為 (stageKey, turnId)。</summary>
    public event Action<string, string> OnTurnBegin;

    /// <summary>當單一內容項目顯示完成時觸發，參數為 (stageKey, turnIndex, contentIndex, ChatContent)。</summary>
    public event Action<string, int, int, ChatContent> OnContentDisplay;

    /// <summary>當玩家選擇分岐選項時觸發，參數為 (stageKey, turnId, ChatSelectorOption)。</summary>
    public event Action<string, string, ChatSelectorOption> OnSelectorChosen;

    private IChatExternalProvider _provider;
    private ChatStageData _currentStage;
    private ChatStageState _currentState;
    private readonly Dictionary<string, ChatStageState> _stateCache = new Dictionary<string, ChatStageState>();

    private PlaybackPhase _phase = PlaybackPhase.Idle;
    private bool _clickReceived;
    private ChatSelectorOption _selectedOption;

    /// <summary>當前是否正在播放中。</summary>
    public bool IsProcessing
    {
        get
        {
            if (_currentStage == null) return false;
            return GetOrCreateState(_currentStage).IsProcessing;
        }
    }

    /// <summary>當前開啟的 stageKey，若未開啟則為 null。</summary>
    public string CurrentStageKey => _currentStage?.stageKey;

    /// <summary>
    /// 初始化。必須在使用前調用，注入外部介面。
    /// </summary>
    public void Initialize(IChatExternalProvider provider)
    {
        _provider = provider;
    }

    /// <summary>
    /// 取得指定階段的閱讀狀態（用於外部 UI 紅點/驚嘆號顯示）。
    /// </summary>
    public ChatReadState GetReadState(string stageKey)
    {
        if (_stateCache.TryGetValue(stageKey, out var state))
            return state.ReadState;

        return ChatReadState.Unread;
    }

    /// <summary>
    /// 取得指定階段的強制閱讀完成狀態。
    /// ForceRead 的階段未讀完時回傳 ForceUnread。
    /// </summary>
    public ChatReadState GetForceReadState(string stageKey)
    {
        var stageData = _provider.GetStageData(stageKey);
        if (stageData == null) return ChatReadState.Unread;

        var readState = GetReadState(stageKey);
        if (stageData.forceRead && readState != ChatReadState.Read)
            return ChatReadState.ForceUnread;

        return readState;
    }

    // ──────────────────────────────────────────────
    //  Public API
    // ──────────────────────────────────────────────

    /// <summary>
    /// 開啟聊天室並開始播放指定階段。
    /// 若該階段之前被中斷過，會從上次進度繼續。
    /// </summary>
    public void Open(string stageKey)
    {
        if (_provider == null)
        {
            Debug.LogError("[Chatroom] Provider not initialized. Call Initialize() first.");
            return;
        }

        var stageData = _provider.GetStageData(stageKey);
        if (stageData == null)
        {
            Debug.LogError($"[Chatroom] Stage data not found: {stageKey}");
            return;
        }

        // 若正在播放其他階段，先停止
        StopPlayback();

        _currentStage = stageData;
        _currentState = GetOrCreateState(stageData);
        _currentState.IsProcessing = true;
        _clickReceived = false;
        _selectedOption = null;

        // 判斷是否有內容可播放
        if (stageData.turns == null || stageData.turns.Count == 0)
        {
            _phase = PlaybackPhase.StageEnd;
        }
        else
        {
            _phase = PlaybackPhase.BeginTurn;
        }
    }

    /// <summary>
    /// 關閉聊天室。
    /// </summary>
    /// <param name="force">
    /// false: 若 IsProcessing 且為強制閱讀，則拒絕關閉並回傳 false。
    /// true: 立即停止並關閉。
    /// </param>
    /// <returns>是否成功關閉。</returns>
    public bool Close(bool force = false)
    {
        if (_currentStage == null) return true;

        if (!force)
        {
            if (_currentStage.forceRead && _currentState != null && _currentState.IsProcessing)
            {
                Debug.LogWarning($"[Chatroom] Cannot close force-read stage: {_currentStage.stageKey}");
                return false;
            }
        }

        StopPlayback();
        return true;
    }

    /// <summary>
    /// 配合多語系切換，重新調用外部 GetText 刷新 UI 上已存在的文字。
    /// 子類或外部 UI 管理器應覆寫/監聽此方法以實際刷新 UI 元素。
    /// </summary>
    public void ReloadContext()
    {
        if (_provider == null || _currentStage == null) return;

        var refreshed = _provider.GetStageData(_currentStage.stageKey);
        if (refreshed != null)
        {
            _currentStage = refreshed;
        }

        Debug.Log($"[Chatroom] ReloadContext for stage: {_currentStage.stageKey}");
    }

    /// <summary>
    /// 玩家點擊推進對話（由 UI 按鈕或觸控事件調用）。
    /// </summary>
    public void OnPlayerClick()
    {
        if (_phase == PlaybackPhase.WaitingForClick)
        {
            _clickReceived = true;
        }
    }

    /// <summary>
    /// 玩家選擇分岐選項（由選項按鈕調用）。
    /// </summary>
    public void OnSelectorSelect(ChatSelectorOption option)
    {
        if (_phase == PlaybackPhase.WaitingForSelector)
        {
            _selectedOption = option;
        }
    }

    // ──────────────────────────────────────────────
    //  Update 狀態機
    // ──────────────────────────────────────────────

    private void Update()
    {
        if (_phase == PlaybackPhase.Idle || _currentStage == null || _currentState == null)
            return;

        switch (_phase)
        {
            case PlaybackPhase.BeginTurn:
                UpdateBeginTurn();
                break;
            case PlaybackPhase.DisplayContent:
                UpdateDisplayContent();
                break;
            case PlaybackPhase.WaitingForClick:
                UpdateWaitingForClick();
                break;
            case PlaybackPhase.WaitingForSelector:
                UpdateWaitingForSelector();
                break;
            case PlaybackPhase.AdvanceContent:
                UpdateAdvanceContent();
                break;
            case PlaybackPhase.AdvanceTurn:
                UpdateAdvanceTurn();
                break;
            case PlaybackPhase.StageEnd:
                UpdateStageEnd();
                break;
        }
    }

    private void UpdateBeginTurn()
    {
        var turns = _currentStage.turns;

        // 跳過沒有內容的回合
        while (_currentState.CurrentTurnIndex < turns.Count)
        {
            var turn = turns[_currentState.CurrentTurnIndex];
            if (turn.contents != null && turn.contents.Count > 0)
                break;
            _currentState.CurrentTurnIndex++;
            _currentState.CurrentContentIndex = 0;
        }

        if (_currentState.CurrentTurnIndex >= turns.Count)
        {
            _phase = PlaybackPhase.StageEnd;
            return;
        }

        var currentTurn = turns[_currentState.CurrentTurnIndex];
        OnTurnBegin?.Invoke(_currentStage.stageKey, currentTurn.turnId);
        _phase = PlaybackPhase.DisplayContent;
    }

    private void UpdateDisplayContent()
    {
        var turns = _currentStage.turns;
        var turn = turns[_currentState.CurrentTurnIndex];
        int c = _currentState.CurrentContentIndex;
        var content = turn.contents[c];

        OnContentDisplay?.Invoke(_currentStage.stageKey, _currentState.CurrentTurnIndex, c, content);

        bool isPreRead = GetGlobalContentIndex(turns, _currentState.CurrentTurnIndex, c) < _currentStage.preReadCount;

        if (content.type == ChatContentType.Selector)
        {
            _selectedOption = null;
            _phase = PlaybackPhase.WaitingForSelector;
        }
        else if (isPreRead)
        {
            // preRead 範圍內直接推進，不等待點擊
            _phase = PlaybackPhase.AdvanceContent;
        }
        else
        {
            _clickReceived = false;
            _phase = PlaybackPhase.WaitingForClick;
        }
    }

    private void UpdateWaitingForClick()
    {
        if (!_clickReceived) return;

        _clickReceived = false;
        _phase = PlaybackPhase.AdvanceContent;
    }

    private void UpdateWaitingForSelector()
    {
        if (_selectedOption == null) return;

        var turn = _currentStage.turns[_currentState.CurrentTurnIndex];
        OnSelectorChosen?.Invoke(_currentStage.stageKey, turn.turnId, _selectedOption);
        _selectedOption = null;
        _phase = PlaybackPhase.AdvanceContent;
    }

    private void UpdateAdvanceContent()
    {
        var turns = _currentStage.turns;
        int t = _currentState.CurrentTurnIndex;
        int c = _currentState.CurrentContentIndex;

        // 更新已讀計數
        int globalIndex = GetGlobalContentIndex(turns, t, c);
        if (globalIndex >= _currentState.ReadContentCount)
        {
            _currentState.ReadContentCount = globalIndex + 1;
        }

        // 推進到下一個 content
        c++;
        if (c < turns[t].contents.Count)
        {
            _currentState.CurrentContentIndex = c;
            _phase = PlaybackPhase.DisplayContent;
        }
        else
        {
            // 本回合結束
            _currentState.CurrentContentIndex = 0;
            _phase = PlaybackPhase.AdvanceTurn;
        }
    }

    private void UpdateAdvanceTurn()
    {
        _currentState.CurrentTurnIndex++;
        _currentState.CurrentContentIndex = 0;

        if (_currentState.CurrentTurnIndex >= _currentStage.turns.Count)
        {
            _phase = PlaybackPhase.StageEnd;
        }
        else
        {
            _phase = PlaybackPhase.BeginTurn;
        }
    }

    private void UpdateStageEnd()
    {
        _currentState.IsProcessing = false;
        _currentState.CurrentTurnIndex = _currentStage.turns != null ? _currentStage.turns.Count : 0;
        _currentState.CurrentContentIndex = 0;

        var stageKey = _currentStage.stageKey;
        _phase = PlaybackPhase.Idle;

        OnStageEnd?.Invoke(stageKey);
    }

    // ──────────────────────────────────────────────
    //  Helpers
    // ──────────────────────────────────────────────

    private void StopPlayback()
    {
        if (_currentState != null)
        {
            _currentState.IsProcessing = false;
        }

        _phase = PlaybackPhase.Idle;
        _currentStage = null;
        _currentState = null;
        _clickReceived = false;
        _selectedOption = null;
    }

    private ChatStageState GetOrCreateState(ChatStageData stageData)
    {
        if (_stateCache.TryGetValue(stageData.stageKey, out var existing))
            return existing;

        int totalContent = 0;
        if (stageData.turns != null)
        {
            foreach (var turn in stageData.turns)
            {
                if (turn.contents != null)
                    totalContent += turn.contents.Count;
            }
        }

        var state = new ChatStageState(stageData.stageKey, totalContent);
        state.ApplyPreRead(stageData.preReadCount);
        _stateCache[stageData.stageKey] = state;
        return state;
    }

    private static int GetGlobalContentIndex(List<ChatTurn> turns, int turnIndex, int contentIndex)
    {
        int index = 0;
        for (int t = 0; t < turnIndex; t++)
        {
            if (turns[t].contents != null)
                index += turns[t].contents.Count;
        }
        return index + contentIndex;
    }
}
