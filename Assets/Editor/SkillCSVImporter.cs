using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
public class SkillCSVImporter : EditorWindow
{
    // 저장될 SO 폴더 경로
    private const string TARGET_FOLDER = "Assets/04_Data/Skill/SkillData";
    [MenuItem("Tools/Import Skill Data from CSV")]
    public static void ImportCSV()
    {
        // 1. CSV 파일 선택 대화상자 오픈
        string filePath = EditorUtility.OpenFilePanel("Select Skill Data CSV File", "", "csv");
        if (string.IsNullOrEmpty(filePath)) return;
        string[] lines = File.ReadAllLines(filePath);
        if (lines.Length <= 1)
        {
            Debug.LogWarning("CSV 파일이 비어있거나 헤더만 존재합니다.");
            return;
        }
        // 2-Pass 처리를 위한 임시 딕셔너리
        Dictionary<string, SkillDataSO> createdSkills = new Dictionary<string, SkillDataSO>();
        Dictionary<string, string> rawNeedSkillIDs = new Dictionary<string, string>();
        // 저장 폴더가 없으면 생성
        if (!Directory.Exists(TARGET_FOLDER))
        {
            Directory.CreateDirectory(TARGET_FOLDER);
        }
        // ===================================================
        // 1-PASS: SkillDataSO 에셋 생성/불러오기 및 기본 데이터 세팅
        // ===================================================
        for (int i = 1; i < lines.Length; i++) // 0번은 헤더 행이므로 1번부터 시작
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;
            string[] tokens = line.Split(',');
            // CSV 칼럼 인덱스 매핑 예시:
            // 0: skillID, 1: skillName, 2: skillDesc, 3: maxLevel, 4: needPoint,
            // 5: needLevel, 6: needMoney, 7: value(| 구분), 8: needSkillIDs(| 구분), 9: spritePath, 10: effectPath
            string skillID = tokens[0].Trim();
            string skillIDName = tokens[0].Trim();
            string skillName = tokens[1].Trim();
            string skillDesc = tokens[2].Trim();
            int maxLevel = int.Parse(tokens[3].Trim());
            int needPoint = int.Parse(tokens[4].Trim());
            int needLevel = int.Parse(tokens[5].Trim());
            int needMoney = int.Parse(tokens[6].Trim());
            // float[] 배열 파싱 (예: "10|20|30" -> float[]{10f, 20f, 30f})
            float[] values = tokens[7].Split('|')
                                     .Where(v => !string.IsNullOrEmpty(v))
                                     .Select(v => float.Parse(v.Trim()))
                                     .ToArray();
            string needSkillsRaw = tokens[8].Trim();
            string spritePath = tokens[9].Trim();
            string effectPath = tokens[10].Trim();
            // 생성될 에셋 파일 이름 규격 (예: Assets/04_Data/Skill/001_MagicArrow.asset)
            string fileName = $"{skillID}_{skillName}.asset";
            string assetPath = $"{TARGET_FOLDER}/{fileName}";
            // 기존 SO 에셋이 존재하는지 확인 (덮어쓰기/갱신 지원)
            SkillDataSO skillSO = AssetDatabase.LoadAssetAtPath<SkillDataSO>(assetPath);
            if (skillSO == null)
            {
                skillSO = ScriptableObject.CreateInstance<SkillDataSO>();
                AssetDatabase.CreateAsset(skillSO, assetPath);
            }
            // 기본 데이터 할당
            skillSO.skillID = skillID;
            skillSO.skillName = skillName;
            skillSO.skillDesc = skillDesc;
            skillSO.skillMaxLevel = maxLevel;
            skillSO.skillNeedSkillPoint = needPoint;
            skillSO.skillNeedLevel = needLevel;
            skillSO.skillNeedMoney = needMoney;
            skillSO.value = values;
            // 아이콘(Sprite) 로드 및 연결
            if (!string.IsNullOrEmpty(spritePath))
            {
                skillSO.skillSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            }
            // SkillEffectSO 로드 및 연결
            if (!string.IsNullOrEmpty(effectPath))
            {
                skillSO.effect = AssetDatabase.LoadAssetAtPath<SkillEffectSO>(effectPath);
            }
            // 변경사항 에디터에 알림
            EditorUtility.SetDirty(skillSO);
            // 2-Pass 조회를 위해 기록
            createdSkills[skillID] = skillSO;
            rawNeedSkillIDs[skillID] = needSkillsRaw;
        }
        // ===================================================
        // 2-PASS: 선행 스킬(needSkill) 레퍼런스 연결
        // ===================================================
        foreach (var kvp in createdSkills)
        {
            string skillID = kvp.Key;
            SkillDataSO skillSO = kvp.Value;
            string rawNeedIDs = rawNeedSkillIDs[skillID];
            if (!string.IsNullOrEmpty(rawNeedIDs))
            {
                string[] idStrings = rawNeedIDs.Split('|');
                List<SkillDataSO> needList = new List<SkillDataSO>();
                foreach (var idStr in idStrings)
                {
                    if (createdSkills.TryGetValue(idStr, out SkillDataSO reqSO))
                    {
                        needList.Add(reqSO);
                    }
                }
                skillSO.needSkill = needList.ToArray();
                EditorUtility.SetDirty(skillSO);
            }
        }
        // 3. 에셋 저장 및 에디터 갱신
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"<color=green>SkillDataSO {createdSkills.Count}개 파싱 및 에셋 생성/갱신 완료!</color>");
    }
}