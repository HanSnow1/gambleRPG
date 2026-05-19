using System.Collections.Generic;
using UnityEngine;

public static class AugmentSelector
{
  public static List<AugmentDefinition> PickChoices(
    IReadOnlyList<AugmentDefinition> pool,
    PlayerAugmentState playerState,
    int count = 3)
  {
    var result = new List<AugmentDefinition>();
    if (pool == null || pool.Count == 0)
      return result;

    var candidates = new List<AugmentDefinition>();
    foreach (var def in pool)
    {
      if (def == null)
        continue;
      if (playerState != null && playerState.IsMaxedOut(def))
        continue;
      candidates.Add(def);
    }

    if (candidates.Count == 0)
      return result;

    count = Mathf.Min(count, candidates.Count);
    for (int i = 0; i < count; i++)
    {
      int index = Random.Range(0, candidates.Count);
      result.Add(candidates[index]);
      candidates.RemoveAt(index);
    }

    return result;
  }
}
