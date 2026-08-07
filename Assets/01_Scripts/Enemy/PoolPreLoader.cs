using UnityEngine;

public class PoolPreLoader : MonoBehaviour
{
    [Header("积己且 橇府普")]
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private Dropitem dropItemPrefab;
    [SerializeField] private AttackPoint attackPointPrefab;
    [SerializeField] private ChasingSickle chasingSicklePrefab;
    [SerializeField] private FireCircle fireCirclePrefab;
    [SerializeField] private FlowerThorns flowerThornsPrefab;
    [SerializeField] private LightningRay lightningRayPrefab;

    [Header("积己 俺荐")]
    [SerializeField] private int enemyCount;
    [SerializeField] private int dropItemCount;
    [SerializeField] private int attackPointCount;
    [SerializeField] private int chasingSickleCount;
    [SerializeField] private int fireCircleCount;
    [SerializeField] private int flowerThornsCount;
    [SerializeField] private int lightningRayCount;

    private void Start()
    {
        MonsterPoolManager.Instance.PreLoadPool(enemyPrefab, enemyCount);
        MonsterPoolManager.Instance.PreLoadPool(dropItemPrefab, dropItemCount);
        MonsterPoolManager.Instance.PreLoadPool(attackPointPrefab, attackPointCount);
        MonsterPoolManager.Instance.PreLoadPool(chasingSicklePrefab, chasingSickleCount);
        MonsterPoolManager.Instance.PreLoadPool(fireCirclePrefab, fireCircleCount);
        MonsterPoolManager.Instance.PreLoadPool(flowerThornsPrefab, flowerThornsCount);
        MonsterPoolManager.Instance.PreLoadPool(lightningRayPrefab, lightningRayCount);
    }
}
