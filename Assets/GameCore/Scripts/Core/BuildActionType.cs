namespace SlotsTavern.Core
{
    public enum BuildActionType
    {
        None = 0,

        BeginBuildPhase = 10,
        GenerateOffers = 20,

        AddSymbol = 100,
        AddItem = 110,
        RemoveSymbol = 120,

        UpgradeSymbol = 200,
        UpgradeItem = 210,
        UpgradePlayerStat = 220,

        Ready = 300
    }

    public enum BuildPlayerStatType
    {
        None = 0,
        MaxHealth = 10,
        ShieldCapacity = 20
    }
}