using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 總資料庫。存放遊戲內所有事件資源的總容器。
/// </summary>
[CreateAssetMenu(fileName = "ChatDatabase", menuName = "Chatroom/Chat Database Asset")]
public class ChatDatabaseAsset : ScriptableObject
{
    /// <summary>所有事件資源清單。</summary>
    public List<ChatEventAsset> events;
}
