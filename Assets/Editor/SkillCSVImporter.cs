using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Text;

public class SkillCSVImporter : EditorWindow
{
    private const string TARGET_FOLDER = "Assets/04_Data/Skill/SkillData";

    [MenuItem("Tools/Import Skill Data from CSV")]
    public static void ImportCSV()
    {
        // 1. CSV 파일 선택
        string filePath = EditorUtility.OpenFilePanel("Select Skill Data CSV File", "", "csv");
        if (string.IsNullOrEmpty(filePath)) return;

        string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);
        if (lines.Length <= 1)
        {
            Debug.LogWarning("CSV 파일이 비어있거나 헤더만 존재합니다.");
            return;
        }

        // 헤더 파싱 및 컬럼 인덱스 맵 생성
        List<string> headerTokens = ParseCSVLine(lines[0]);
        Dictionary<string, int> colMap = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < headerTokens.Count; i++)
        {
            string colName = headerTokens[i].Trim();
            if (!string.IsNullOrEmpty(colName) && !colMap.ContainsKey(colName))
            {
                colMap[colName] = i;
            }
        }

        // 저장 대상 폴더 생성
        if (!Directory.Exists(TARGET_FOLDER))
        {
            Directory.CreateDirectory(TARGET_FOLDER);
        }

        // 폴더 내 기존 에셋 매핑 (skillID -> SkillDataSO)
        Dictionary<string, SkillDataSO> existingBySkillID = CacheExistingSkillsBySkillID(TARGET_FOLDER);

        Dictionary<string, SkillDataSO> createdSkills = new Dictionary<string, SkillDataSO>();
        Dictionary<string, string> rawNeedSkillIDs = new Dictionary<string, string>();

        // ===================================================
        // 1-PASS: SkillDataSO 생성/로드 및 기본 데이터 설정
        // ===================================================
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            List<string> tokens = ParseCSVLine(line);

            string soName = GetColumnValue(tokens, colMap, "soName", -1);
            string skillID = GetColumnValue(tokens, colMap, "skillID", 0);
            string skillName = GetColumnValue(tokens, colMap, "skillName", 1);
            string skillDesc = GetColumnValue(tokens, colMap, "skillDesc", 2);

            int maxLevel = ParseIntOrDefault(GetColumnValue(tokens, colMap, "maxLevel", 3), 0);
            int needPoint = ParseIntOrDefault(GetColumnValue(tokens, colMap, "needPoint", 4), 0);
            int needLevel = ParseIntOrDefault(GetColumnValue(tokens, colMap, "needLevel", 5), 0);
            int needMoney = ParseIntOrDefault(GetColumnValue(tokens, colMap, "needMoney", 6), 0);

            string valueRaw = GetColumnValue(tokens, colMap, "value", 7);
            float[] values = string.IsNullOrEmpty(valueRaw)
                ? new float[0]
                : valueRaw.Split('|')
                          .Where(v => !string.IsNullOrWhiteSpace(v))
                          .Select(v => ParseFloatOrDefault(v.Trim(), 0f))
                          .ToArray();

            string needSkillsRaw = GetColumnValue(tokens, colMap, "needSkillIDs", 8);
            string spritePath = GetColumnValue(tokens, colMap, "spritePath", 9);
            string effectPath = GetColumnValue(tokens, colMap, "effectPath", 10);
            string changeStat = GetColumnValue(tokens, colMap, "changeStat", 11);

            // 에셋 파일 이름 및 대상 경로 결정
            string targetSOName = !string.IsNullOrEmpty(soName) ? soName : null;
            SkillDataSO skillSO = null;
            string assetPath = null;

            if (!string.IsNullOrEmpty(targetSOName))
            {
                if (!targetSOName.EndsWith(".asset", System.StringComparison.OrdinalIgnoreCase))
                {
                    assetPath = $"{TARGET_FOLDER}/{targetSOName}.asset";
                }
                else
                {
                    assetPath = $"{TARGET_FOLDER}/{targetSOName}";
                }
                skillSO = AssetDatabase.LoadAssetAtPath<SkillDataSO>(assetPath);
            }

            // soName으로 로드하지 못한 경우 기존 skillID로 검색
            if (skillSO == null && !string.IsNullOrEmpty(skillID) && existingBySkillID.TryGetValue(skillID, out SkillDataSO existingSO))
            {
                skillSO = existingSO;
                assetPath = AssetDatabase.GetAssetPath(skillSO);
            }

            // 기존 에셋이 없으면 새 에셋 생성
            if (skillSO == null)
            {
                string fileName = !string.IsNullOrEmpty(targetSOName)
                    ? (targetSOName.EndsWith(".asset", System.StringComparison.OrdinalIgnoreCase) ? targetSOName : $"{targetSOName}.asset")
                    : $"{skillID}_{skillName}.asset";

                fileName = SanitizeFileName(fileName);
                assetPath = $"{TARGET_FOLDER}/{fileName}";

                skillSO = ScriptableObject.CreateInstance<SkillDataSO>();
                AssetDatabase.CreateAsset(skillSO, assetPath);
            }

            // 기본 데이터 설정
            skillSO.skillID = skillID;
            skillSO.skillName = skillName;
            skillSO.skillDesc = skillDesc;
            skillSO.skillMaxLevel = maxLevel;
            skillSO.skillNeedSkillPoint = needPoint;
            skillSO.skillNeedLevel = needLevel;
            skillSO.skillNeedMoney = needMoney;
            skillSO.value = values;

            if (!string.IsNullOrEmpty(spritePath))
            {
                skillSO.skillSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            }
            else
            {
                skillSO.skillSprite = null;
            }

            if (!string.IsNullOrEmpty(effectPath))
            {
                skillSO.effect = AssetDatabase.LoadAssetAtPath<SkillEffectSO>(effectPath);
            }
            else
            {
                skillSO.effect = null;
            }

            skillSO.skillChangeStat = changeStat;

            EditorUtility.SetDirty(skillSO);

            if (!string.IsNullOrEmpty(skillID))
            {
                createdSkills[skillID] = skillSO;
                rawNeedSkillIDs[skillID] = needSkillsRaw;
            }
        }

        // ===================================================
        // 2-PASS: 선행 스킬(needSkill) 참조 연결
        // ===================================================
        foreach (var kvp in createdSkills)
        {
            string skillID = kvp.Key;
            SkillDataSO skillSO = kvp.Value;
            string rawNeedIDs = rawNeedSkillIDs.ContainsKey(skillID) ? rawNeedSkillIDs[skillID] : "";

            if (!string.IsNullOrEmpty(rawNeedIDs))
            {
                string[] idStrings = rawNeedIDs.Split('|');
                List<SkillDataSO> needList = new List<SkillDataSO>();
                foreach (var idStr in idStrings)
                {
                    string trimmedId = idStr.Trim();
                    if (!string.IsNullOrEmpty(trimmedId) && createdSkills.TryGetValue(trimmedId, out SkillDataSO reqSO))
                    {
                        needList.Add(reqSO);
                    }
                }
                skillSO.needSkill = needList.ToArray();
            }
            else
            {
                skillSO.needSkill = new SkillDataSO[0];
            }

            EditorUtility.SetDirty(skillSO);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"<color=green>SkillDataSO {createdSkills.Count}개 파싱 및 에셋 생성/갱신 완료!</color>");
    }

    private static List<string> ParseCSVLine(string line)
    {
        List<string> result = new List<string>();
        if (line == null) return result;

        StringBuilder sb = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    sb.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(sb.ToString());
                sb.Clear();
            }
            else
            {
                sb.Append(c);
            }
        }
        result.Add(sb.ToString());
        return result;
    }

    private static string GetColumnValue(List<string> tokens, Dictionary<string, int> colMap, string columnName, int fallbackIndex)
    {
        if (colMap.TryGetValue(columnName, out int idx) && idx >= 0 && idx < tokens.Count)
        {
            return tokens[idx].Trim();
        }
        if (fallbackIndex >= 0 && fallbackIndex < tokens.Count)
        {
            return tokens[fallbackIndex].Trim();
        }
        return "";
    }

    private static int ParseIntOrDefault(string text, int defaultValue)
    {
        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
        {
            return result;
        }
        return defaultValue;
    }

    private static float ParseFloatOrDefault(string text, float defaultValue)
    {
        if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
        {
            return result;
        }
        return defaultValue;
    }

    private static Dictionary<string, SkillDataSO> CacheExistingSkillsBySkillID(string folderPath)
    {
        Dictionary<string, SkillDataSO> map = new Dictionary<string, SkillDataSO>();
        string[] guids = AssetDatabase.FindAssets("t:SkillDataSO", new[] { folderPath });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SkillDataSO so = AssetDatabase.LoadAssetAtPath<SkillDataSO>(path);
            if (so != null && !string.IsNullOrEmpty(so.skillID) && !map.ContainsKey(so.skillID))
            {
                map[so.skillID] = so;
            }
        }
        return map;
    }

    private static string SanitizeFileName(string fileName)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        foreach (char c in invalidChars)
        {
            fileName = fileName.Replace(c, '_');
        }
        return fileName;
    }
}