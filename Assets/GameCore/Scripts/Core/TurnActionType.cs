namespace SlotsTavern.Core
{
    public enum TurnActionType
    {
        None = 0,

        UseItem = 10,
        StartSpin = 20,

        ReadyBuild = 100,
        BuySymbol = 110,
        BuyItem = 120,
        RemoveSymbol = 130,
        UpgradeSymbol = 140,
        UpgradeItem = 150,
        UpgradePlayerStat = 160
    }
}