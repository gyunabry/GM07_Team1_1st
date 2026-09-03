using UnityEngine;
using UnityEngine.UI;

public class PoolPreLoader : MonoBehaviour
{
    [Header("积己且 橇府普")]
    [SerializeField] private EnemySpawnEntry[] enemies;
    [SerializeField] private CarrierWorker carrierWorkerPrefab;
    [SerializeField] private HunterWorker hunterWorkerPrefab;
    [SerializeField] private CustomerController customerPrefab;
    [SerializeField] private Dropitem dropItemPrefab;
    [SerializeField] private AttackPoint attackPointPrefab;
    [SerializeField] private ChasingSickle chasingSicklePrefab;
    [SerializeField] private FireCircle fireCirclePrefab;
    [SerializeField] private FlowerThorns flowerThornsPrefab;
    [SerializeField] private LightningRay lightningRayPrefab;
    [SerializeField] private AttackEquPrefab buttonPrefab;
    [SerializeField] private SkillTreePopUp skillTreePopUpPrefab;
    [SerializeField] private SkillDesc skillDesc;
    [SerializeField] private ItemTransferEffect transferEffectPrefab;

    [Header("积己 俺荐")]
    [SerializeField] private int carrierCount;
    [SerializeField] private int hunterCount;
    [SerializeField] private int customerCount;
    [SerializeField] private int dropItemCount;
    [SerializeField] private int attackPointCount;
    [SerializeField] private int chasingSickleCount;
    [SerializeField] private int fireCircleCount;
    [SerializeField] private int flowerThornsCount;
    [SerializeField] private int lightningRayCount;
    [SerializeField] private int buttonCount;
    [SerializeField] private int skillTreePopUpCount;
    [SerializeField] private int skillDescCount;
    [SerializeField] private int transferEffectCount;

    private bool setup = false;

    private void Start()
    {
        if (setup) return;
        //MonsterPoolManager.Instance.PreLoadPool(enemyPrefab, enemyCount);

        foreach (EnemySpawnEntry entry in enemies)
        {
            PoolManager.Instance.PreLoadPool(entry.prefab, entry.maxEnemyCount);
        }

        PoolManager.Instance.PreLoadPool(carrierWorkerPrefab, carrierCount);
        PoolManager.Instance.PreLoadPool(hunterWorkerPrefab, hunterCount);
        PoolManager.Instance.PreLoadPool(customerPrefab, customerCount);
        PoolManager.Instance.PreLoadPool(dropItemPrefab, dropItemCount);
        PoolManager.Instance.PreLoadPool(attackPointPrefab, attackPointCount);
        PoolManager.Instance.PreLoadPool(chasingSicklePrefab, chasingSickleCount);
        PoolManager.Instance.PreLoadPool(fireCirclePrefab, fireCircleCount);
        PoolManager.Instance.PreLoadPool(flowerThornsPrefab, flowerThornsCount);
        PoolManager.Instance.PreLoadPool(lightningRayPrefab, lightningRayCount);
        PoolManager.Instance.PreLoadPool(buttonPrefab, buttonCount);
        PoolManager.Instance.PreLoadPool(skillTreePopUpPrefab, skillTreePopUpCount);
        PoolManager.Instance.PreLoadPool(skillDesc, skillDescCount);
        PoolManager.Instance.PreLoadPool(transferEffectPrefab, transferEffectCount);
        setup = true;
    }

}
