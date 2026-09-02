using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class SkillCSVExporter : EditorWindow
{
    private const string SOURCE_FOLDER = "Assets/04_Data/Skill/SkillData";

    [MenuItem("Tools/Export Skill Data to CSV")]
    public static void ExportCSV()
    {
        // 1. 저장할 CSV 파일 경로 선택
        string savePath = EditorUtility.SaveFilePanel("Save Skill Data to CSV", "", "SkillData_Export.csv", "csv");
        if (string.IsNullOrEmpty(savePath)) return;

        // 2. 지정된 폴더 내의 모든 SkillDataSO 에셋 구하기
        string[] guids = AssetDatabase.FindAssets("t:SkillDataSO", new[] { SOURCE_FOLDER });
        if (guids.Length == 0)
        {
            Debug.LogWarning($"[{SOURCE_FOLDER}] 폴더 내에 SkillDataSO 에셋이 없습니다.");
            return;
        }

        List<string> csvLines = new List<string>();

        // 3. CSV 헤더(Header) 생성
        // 0: soName, 1: skillID, 2: skillName, 3: skillDesc, 4: maxLevel, 5: needPoint,
        // 6: needLevel, 7: needMoney, 8: value, 9: needSkillIDs, 10: spritePath, 11: effectPath, 12: changeStat
        string header = "soName,skillID,skillName,skillDesc,maxLevel,needPoint,needLevel,needMoney,value,needSkillIDs,spritePath,effectPath,changeStat";
        csvLines.Add(header);

        // ID순으로 정렬하여 Export
        List<SkillDataSO> skillList = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<SkillDataSO>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(so => so != null)
            .OrderBy(so => so.skillID)
            .ToList();

        // 4. 각 SkillDataSO의 데이터를 CSV 줄 문자열로 변환
        foreach (SkillDataSO skillSO in skillList)
        {
            // float[] 배열 -> "10|20|30"
            string valueStr = skillSO.value != null ? string.Join("|", skillSO.value) : "";

            // SkillDataSO[] 배열 -> "001|002" (skillID들을 '|'로 연결)
            string needSkillsStr = "";
            if (skillSO.needSkill != null && skillSO.needSkill.Length > 0)
            {
                needSkillsStr = string.Join("|", skillSO.needSkill
                    .Where(reqSO => reqSO != null)
                    .Select(reqSO => reqSO.skillID));
            }

            // Sprite 에셋 상대 경로
            string spritePath = skillSO.skillSprite != null ? AssetDatabase.GetAssetPath(skillSO.skillSprite) : "";

            // SkillEffectSO 에셋 상대 경로
            string effectPath = skillSO.effect != null ? AssetDatabase.GetAssetPath(skillSO.effect) : "";

            // CSV 줄 생성
            string line = $"{EscapeCSV(skillSO.name)}," +
                          $"{EscapeCSV(skillSO.skillID)}," +
                          $"{EscapeCSV(skillSO.skillName)}," +
                          $"{EscapeCSV(skillSO.skillDesc)}," +
                          $"{skillSO.skillMaxLevel}," +
                          $"{skillSO.skillNeedSkillPoint}," +
                          $"{skillSO.skillNeedLevel}," +
                          $"{skillSO.skillNeedMoney}," +
                          $"{valueStr}," +
                          $"{needSkillsStr}," +
                          $"{spritePath}," +
                          $"{effectPath}," +
                          $"{EscapeCSV(skillSO.skillChangeStat)}";

            csvLines.Add(line);
        }

        // 5. CSV 파일 저장 (UTF-8 인코딩)
        File.WriteAllLines(savePath, csvLines, Encoding.UTF8);

        AssetDatabase.Refresh();
        Debug.Log($"<color=green>SkillDataSO {skillList.Count}개의 데이터가 [{savePath}]에 성공적으로 저장되었습니다!</color>");
    }

    // 쉼표, 큰따옴표, 줄바꿈 포함 문자열 이스케이프 처리
    private static string EscapeCSV(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        if (text.Contains(",") || text.Contains("\"") || text.Contains("\n") || text.Contains("\r"))
        {
            text = text.Replace("\"", "\"\"");
            return $"\"{text}\"";
        }
        return text;
    }
}
