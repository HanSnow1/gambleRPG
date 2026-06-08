using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class AugmentRerollService
{
  public static string RerollAllAugments(PlayerAugmentState augmentState, AugmentCatalog catalog)
  {
    if (augmentState == null)
      return "Reroll augments: no augment state.";

    var pool = BuildAugmentPool(catalog);
    if (pool.Count == 0)
      return "Reroll augments: catalog empty.";

    int count = augmentState.Owned.Count;
    if (count == 0)
      return "Reroll augments: none owned.";

    augmentState.ClearOwnedForReroll();

    var picks = new List<AugmentDefinition>();
    var available = new List<AugmentDefinition>(pool);
    for (int i = 0; i < count && available.Count > 0; i++)
    {
      int index = Random.Range(0, available.Count);
      picks.Add(available[index]);
      available.RemoveAt(index);
    }

    foreach (var def in picks)
      augmentState.AddOwnedFromReroll(def);

    augmentState.ResetRunCounters();
    return $"Augments rerolled → {count} random card(s).";
  }

  public static string RerollAllRelics(PlayerRelicState relicState)
  {
    if (relicState == null)
      return "Reroll relics: no relic state.";

    int count = relicState.Owned.Count;
    if (count == 0)
      return "Reroll relics: none owned.";

    relicState.RerollAllInPlace();
    return $"Relics rerolled → {count} random item(s).";
  }

  public static string RerollAugmentsAndRelics(
    PlayerAugmentState augmentState,
    PlayerRelicState relicState,
    AugmentCatalog catalog)
  {
    var log = new StringBuilder();
    log.AppendLine(RerollAllAugments(augmentState, catalog));
    log.Append(RerollAllRelics(relicState));
    return log.ToString().TrimEnd();
  }

  private static List<AugmentDefinition> BuildAugmentPool(AugmentCatalog catalog)
  {
    var pool = new List<AugmentDefinition>();
    if (catalog?.allAugments == null)
      return pool;

    foreach (var def in catalog.allAugments)
    {
      if (def != null)
        pool.Add(def);
    }

    return pool;
  }
}
