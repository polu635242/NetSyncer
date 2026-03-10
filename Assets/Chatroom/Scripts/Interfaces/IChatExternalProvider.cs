using UnityEngine;

/// <summary>
/// 外部介面。Chatroom 不直接處理語系、資源載入與資料來源，
/// 由外部實作本介面提供具體功能。
/// </summary>
public interface IChatExternalProvider
{
    /// <summary>
    /// 取得多語系文字。輸入 nameKey 或文字內容 Key，回傳當前語言字串。
    /// </summary>
    string GetText(string key);

    /// <summary>
    /// 載入 Sprite 資源。輸入 iconKey 或圖片路徑，回傳 Sprite 供 UI 顯示。
    /// </summary>
    Sprite LoadSprite(string key);

    /// <summary>
    /// 取得階段資料。供系統獲取序列化後的 ChatStageData 物件。
    /// </summary>
    ChatStageData GetStageData(string key);
}
