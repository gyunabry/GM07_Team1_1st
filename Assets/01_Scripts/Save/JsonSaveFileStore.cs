using System;
using System.IO;
using System.Text;
using UnityEngine;

public class JsonSaveFileStore
{
    private const string SaveFileName = "saveData.json";

    public string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    public bool Exists()
    {
        return File.Exists(SavePath);
    }

    // SaveGameData를 Json으로 변환해 파일에 저장
    public bool Save(SaveGameData data)
    {
        if (data == null)
        {
            Debug.LogError("저장할 SaveGameData가 null입니다.");
        }

        try
        {
            string json = JsonUtility.ToJson(data, true);

            File.WriteAllText(SavePath, json, new UTF8Encoding(false));
            Debug.Log($"저장 완료: {SavePath}");

            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"저장 실패: {SavePath}");
            Debug.LogException(e);

            return false;
        }
    }

    // json 파일을 읽어 SaveGameData로 변환
    public bool TryLoad(out SaveGameData data)
    {
        data = null;

        if (!Exists())
        {
            Debug.LogWarning($"저장 파일이 없습니다: {SavePath}");
            return false;
        }

        try
        {
            string json = File.ReadAllText(SavePath, Encoding.UTF8);

            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogError("저장 파일이 비어있습니다.");
                return false;
            }

            data = JsonUtility.FromJson<SaveGameData>(json);

            if (data == null)
            {
                Debug.LogError("JSON 파일로부터 변환을 실패했습니다.");
                return false;
            }

            // 현재 지원하는 저장 구조인지 확인
            // 현재까지는 저장 구조 변경 시 하드코딩 되어있는 숫자를 수정해야 함
            if (data.schemaVersion != 1)
            {
                Debug.LogError($"지원하지 않는 저장 버전입니다: {data.schemaVersion}");
                data = null;
                return false;
            }

            Debug.Log($"게임 불러오기 완료: {SavePath}");

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"게임 불러오기 실패: {SavePath}");
            Debug.LogException(e);

            data = null;
            return false;
        }
    }

    // 저장한 파일을 삭제하는 메서드
    public bool Delete()
    {
        if (!Exists()) return true;

        try
        {
            File.Delete(SavePath);
            Debug.Log($"저장 파일 삭제 완료: {SavePath}");

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"저장 파일 삭제 실패: {SavePath}");
            Debug.LogException(e);

            return false;
        }
    }
}
