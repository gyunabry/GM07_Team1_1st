using UnityEngine;
using UnityEngine.UI;

public class PoolPreLoader : MonoBehaviour
{
    [Header("积己且 橇府普")]
    [SerializeField] private EnemySpawnEntry[] enemies;
    [SerializeField] private Dropitem dropItemPrefab;
    [SerializeField] private AttackPoint attackPointPrefab;
    [SerializeField] private ChasingSickle chasingSicklePrefab;
    [SerializeField] private FireCircle fireCirclePrefab;
    [SerializeField] private FlowerThorns flowerThornsPrefab;
    [SerializeField] private LightningRay lightningRayPrefab;
    [SerializeField] private AttackEquPrefab buttonPrefab;
    [SerializeField] private SkillTreePopUp skillTreePopUpPrefab;

    [Header("积己 俺荐")]
    [SerializeField] private int dropItemCount;
    [SerializeField] private int attackPointCount;
    [SerializeField] private int chasingSickleCount;
    [SerializeField] private int fireCircleCount;
    [SerializeField] private int flowerThornsCount;
    [SerializeField] private int lightningRayCount;
    [SerializeField] private int buttonCount;
    [SerializeField] private int skillTreePopUpCount;

    private bool setup = false;

    private void Start()
    {
        if (setup) return;
        //MonsterPoolManager.Instance.PreLoadPool(enemyPrefab, enemyCount);

        foreach (EnemySpawnEntry entry in enemies)
        {
            MonsterPoolManager.Instance.PreLoadPool(entry.prefab, entry.maxEnemyCount);
        }

        MonsterPoolManager.Instance.PreLoadPool(dropItemPrefab, dropItemCount);
        MonsterPoolManager.Instance.PreLoadPool(attackPointPrefab, attackPointCount);
        MonsterPoolManager.Instance.PreLoadPool(chasingSicklePrefab, chasingSickleCount);
        MonsterPoolManager.Instance.PreLoadPool(fireCirclePrefab, fireCircleCount);
        MonsterPoolManager.Instance.PreLoadPool(flowerThornsPrefab, flowerThornsCount);
        MonsterPoolManager.Instance.PreLoadPool(lightningRayPrefab, lightningRayCount);
        MonsterPoolManager.Instance.PreLoadPool(buttonPrefab, buttonCount);
        MonsterPoolManager.Instance.PreLoadPool(skillTreePopUpPrefab, skillTreePopUpCount);
        setup = true;
    }

}
