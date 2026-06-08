using UnityEngine;

[CreateAssetMenu(menuName = "GambleRogue/Relic Catalog", fileName = "RelicCatalog")]
public class RelicCatalog : ScriptableObject
{
  public RelicDefinition[] commonRelics;

  public RelicDefinition PickRandomCommon()
  {
    if (commonRelics == null || commonRelics.Length == 0)
      return null;

    var pool = new System.Collections.Generic.List<RelicDefinition>();
    foreach (var relic in commonRelics)
    {
      if (relic != null && relic.rarity == RelicRarity.Common)
        pool.Add(relic);
    }

    if (pool.Count == 0)
    {
      foreach (var relic in commonRelics)
      {
        if (relic != null)
          pool.Add(relic);
      }
    }

    if (pool.Count == 0)
      return null;

    return pool[Random.Range(0, pool.Count)];
  }

  public static RelicCatalog CreateRuntimeFallback()
  {
    var catalog = CreateInstance<RelicCatalog>();
    catalog.commonRelics = new[]
    {
      CreateRuntimeRelic("relic_lucky_coin", "Lucky Coin", RelicStatType.DealDamagePercent, 10f),
      CreateRuntimeRelic("relic_iron_charm", "Iron Charm", RelicStatType.MaxHpFlat, 15f),
      CreateRuntimeRelic("relic_cracked_lens", "Cracked Lens", RelicStatType.TakeDamagePercent, 8f)
    };
    return catalog;
  }

  private static RelicDefinition CreateRuntimeRelic(
    string id,
    string name,
    RelicStatType statType,
    float value)
  {
    var relic = CreateInstance<RelicDefinition>();
    relic.relicId = id;
    relic.displayName = name;
    relic.rarity = RelicRarity.Common;
    relic.statType = statType;
    relic.baseStatValue = value;
    relic.description = $"{name}: {statType} {value:+0;-0}";
    return relic;
  }
}
