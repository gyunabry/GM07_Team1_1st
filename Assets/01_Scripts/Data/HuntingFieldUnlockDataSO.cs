using UnityEngine;

[CreateAssetMenu(fileName = "HuntingFieldUnlock", menuName = "Tycoon/Hunting Field Unlock Data")]
public class HuntingFieldUnlockDataSO : ScriptableObject
{
    [SerializeField] private Sprite fieldIcon;
    [SerializeField] private string destinationId;
    [SerializeField] private string displayName;

    [SerializeField] private bool isUnlocked;
    [SerializeField] private int requiredLevel = 1;
    [SerializeField] private int unlockCost;

    public Sprite FieldIcon => fieldIcon;
    public string DestinationId => destinationId;
    public string DisplayName => displayName;

    public bool IsUnlocked => isUnlocked;
    public int RequiredLevel => requiredLevel;
    public int UnlockCost => unlockCost;
}
