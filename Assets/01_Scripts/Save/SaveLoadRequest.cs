using System;

public static class SaveLoadRequest
{
    private static SaveGameData pendingData;

    // 현재 적용을 기다리는 저장 데이터가 있는지 반환
    public static bool HasPending => pendingData != null;

    // 타이틀씬에서 읽은 저장 데이터를 등록
    public static void RequestLoad(SaveGameData data)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        pendingData = data;
    }

    public static bool TryConsumeSaveData(out SaveGameData data)
    {
        data = pendingData;
        pendingData = null;

        return data != null;
    }

    // 새 게임 선택 시 호출
    public static void Clear()
    {
        pendingData = null;
    }
}
