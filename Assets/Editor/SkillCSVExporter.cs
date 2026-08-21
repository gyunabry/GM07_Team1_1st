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
        // 1. 저장할 CSV 파일 경로 및 이름 지정
        string savePath = EditorUtility.SaveFilePanel("Save Skill Data to CSV", "", "SkillData_Export.csv", "csv");
        if (string.IsNullOrEmpty(savePath)) return;

        // 2. 지정된 폴더 내의 모든 SkillDataSO 에셋 검색 및 로드
        string[] guids = AssetDatabase.FindAssets("t:SkillDataSO", new[] { SOURCE_FOLDER });
        if (guids.Length == 0)
        {
            Debug.LogWarning($"[{SOURCE_FOLDER}] 폴더에서 SkillDataSO 에셋을 찾을 수 없습니다.");
            return;
        }

        List<string> csvLines = new List<string>();

        // 3. CSV 헤더(Header) 작성 (Importer의 토큰 인덱스와 동일한 순서)
        // 0: skillID, 1: skillName, 2: skillDesc, 3: maxLevel, 4: needPoint,
        // 5: needLevel, 6: needMoney, 7: value, 8: needSkillIDs, 9: spritePath, 10: effectPath
        string header = "skillID,skillName,skillDesc,maxLevel,needPoint,needLevel,needMoney,value,needSkillIDs,spritePath,effectPath";
        csvLines.Add(header);

        // ID순으로 정렬하여 수출(Export)
        List<SkillDataSO> skillList = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<SkillDataSO>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(so => so != null)
            .OrderBy(so => so.skillID)
            .ToList();

        // 4. 각 SkillDataSO의 데이터를 CSV 라인으로 변환
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

            // Sprite 에셋 경로 추출
            string spritePath = skillSO.skillSprite != null ? AssetDatabase.GetAssetPath(skillSO.skillSprite) : "";

            // SkillEffectSO 에셋 경로 추출
            string effectPath = skillSO.effect != null ? AssetDatabase.GetAssetPath(skillSO.effect) : "";

            // CSV 줄 구성
            string line = $"{EscapeCSV(skillSO.skillID)}," +
                          $"{EscapeCSV(skillSO.skillName)}," +
                          $"{EscapeCSV(skillSO.skillDesc)}," +
                          $"{skillSO.skillMaxLevel}," +
                          $"{skillSO.skillNeedSkillPoint}," +
                          $"{skillSO.skillNeedLevel}," +
                          $"{skillSO.skillNeedMoney}," +
                          $"{valueStr}," +
                          $"{needSkillsStr}," +
                          $"{spritePath}," +
                          $"{effectPath}";

            csvLines.Add(line);
        }

        // 5. CSV 파일로 쓰기 (UTF-8 인코딩)
        File.WriteAllLines(savePath, csvLines, Encoding.UTF8);

        AssetDatabase.Refresh();
        Debug.Log($"<color=green>SkillDataSO {skillList.Count}개의 데이터를 [{savePath}]로 성공적으로 내보냈습니다!</color>");
    }

    // 콤마(,)나 줄바꿈이 들어간 텍스트 처리용 헬퍼 함수
    private static string EscapeCSV(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        if (text.Contains(",") || text.Contains("\"") || text.Contains("\n"))
        {
            text = text.Replace("\"", "\"\"");
            return $"\"{text}\"";
        }
        return text;
    }
}
