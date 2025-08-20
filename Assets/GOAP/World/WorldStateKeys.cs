
public static class WorldStateKeys
{
    // --- World State Conditions ---
    public const string LogsInStockpile = "oakLogsInStockpile";
    public const string IronInStockpile = "ironIngotsInStockpile";
    public const string CrystalsInStockpile = "crystalShardsInStockpile";
    public const string EnchantedStaffBuilt = "enchantedStaffBuilt";
    public const string RunedShieldBuilt = "runedShieldBuilt";
    public const string CombinedArtifactBuilt = "combinedArtifactBuilt";

    // --- Action Effects / Goal States ---
    public const string LogsReadyForPickup = "logsReadyForPickup";
    public const string IronReadyForPickup = "ironReadyForPickup";
    public const string CrystalsReadyForPickup = "crystalsReadyForPickup";
    public const string ResourceDelivered = "resourceDelivered";

    // --- COMBAT ---
    public const string IsSafe = "isSafe";
    public const string HasTarget = "hasTarget";
    public const string TargetInAttackRange = "targetInAttackRange";
    public const string TargetAttacked = "targetAttacked";
    public const string IsHealed = "isHealed";
}