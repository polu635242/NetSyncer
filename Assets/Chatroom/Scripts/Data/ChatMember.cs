using System;

/// <summary>
/// 聊天室成員資料。定義參與者身分與 UI 樣式。
/// </summary>
[Serializable]
public class ChatMember
{
    /// <summary>唯一識別碼。</summary>
    public string userKey;

    /// <summary>多語系名稱 Key，透過外部 GetText 取得顯示名稱。</summary>
    public string nameKey;

    /// <summary>頭像素材載入 Key，透過外部 LoadSprite 取得頭像。</summary>
    public string iconKey;

    /// <summary>是否為玩家角色，決定訊息靠右佈局。</summary>
    public bool isPlayer;
}
