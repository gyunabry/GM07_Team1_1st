using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Tycoon/Enemy Data")]
public class EnemyDataSO : ScriptableObject
{
    //public int enemyCode;
    //public string enemyName;
    //public int hp;
    //public float patrolSpeed;
    //public float runSpeed;
    //public float runStartDis;
    //public float runEndDis;
    //public List<DropItemSO> dropItem;

    [SerializeField] private string enemyId;
    [SerializeField] private string enemyName;
    [SerializeField] private string discription;
    [SerializeField] private int hp;
    [SerializeField] private float patrolSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private EnemyReward reward;

    [Header("기본 정보")]
    public string EnemyId => enemyId;
    public string EnemyName => enemyName;
    public string Description => discription;

    [Header("능력치")]
    public int Hp => hp;

    public float PatrolSpeed => patrolSpeed;

    public float RunSpeed => runSpeed;

    public float RunStartDistance { get; private set; }

    public float RunEndDistance { get; private set; }

    [Header("처치 보상")]
    public EnemyReward Reward => reward;
}
