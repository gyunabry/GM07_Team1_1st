//BGM
public enum EBGMType
{
    None,
    Title,
    Atelier,
    HuntingSpot,
}

//SFX
public enum ESFXType
{
    None,

    // UI 효과음
    UI_ButtonClick,
    UI_Open,
    UI_Close,
    UI_Hovor,
    UI_Comfirm,
    UI_Cancel,
    UI_ImpossibleClick,
    UI_LackGoods,

    // 상호작용 효과음
    Inter_Refining,
    Inter_MagicKettle,
    Inter_Sending,
    Inter_Selling,
    Work_Production,
    Work_Sending,
    Inven_Get,
    Inven_Full,
    Inven_Supply,
    Portal,
    PlayerMove,
    PlayerLevelUp,

    // 손님 효과음
    Costomer_Imminent,
    Costomer_Exit,
    Costmer_Calculate,

    // 건설 효과음
    CanBuild,
    ImpossibleBuild,

    // 스킬 효과음
    Active_ChasingSickle,
    Hit_ChasingSickle,
    Active_MagicArrow,
    Hit_MagicArrow,
    Active_FireCircle,
    Hit_FireCircle,
    Active_LightningRay,
    Hit_LightningRay,
    Active_FlowerThorns,
    Hit_FlowerThorns,

    // 몬스터 효과음
    Hit_Mushroom,
    Die_Mushroom,
    Hit_Bat,
    Die_Bat,
    Hit_Plant,
    Die_Plant,
}