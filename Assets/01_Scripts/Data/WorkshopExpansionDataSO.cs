using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WorkshopExpansion", menuName = "Tycoon/Workshop Expansion DataSO")]
public class WorkshopExpansionDataSO : ScriptableObject
{
    [Header("식별 정보")]
    [SerializeField] private string expansionId; // 기존 시설과 공유
    [SerializeField] private string displayName;
    [SerializeField, TextArea] private string description;
    [SerializeField] private Sprite icon;

    [Header("확장 단계")]
    [SerializeField, Min(1)] private int stageIndex;

    [Header("구매조건")]
    [SerializeField] private int requiredLevel;
    [SerializeField] private int price;

    public string ExpansionId => expansionId;
    public string DisplayName => displayName;
    public string Description => description;
    public Sprite Icon => icon;

    public int StageIndex => stageIndex;

    public int RequiredLevel => requiredLevel;
    public int Price => price;
}
