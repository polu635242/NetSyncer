using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 單一聊天階段的 ScriptableObject 資源。
/// 企劃可在 Inspector 中編輯，或透過 JSON 匯入。
/// </summary>
[CreateAssetMenu(fileName = "NewChatStage", menuName = "Chatroom/Chat Stage Asset")]
public class ChatStageAsset : ScriptableObject
{
    public ChatStageData data;
}
