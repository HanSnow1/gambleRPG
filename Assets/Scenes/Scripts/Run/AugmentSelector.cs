using System.Collections.Generic;
using UnityEngine;

public static class AugmentSelector
{
  public static List<AugmentDefinition> PickChoices(
    IReadOnlyList<AugmentDefinition> pool,
    PlayerAugmentState playerState,
    int count = 3,
    int tierBias = 0)
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

    int minTier = Mathf.Clamp((int)AugmentTier.Silver + tierBias, 0, (int)AugmentTier.Prismatic);
    var biasedCandidates = FilterByMinTier(candidates, minTier);
    if (biasedCandidates.Count == 0)
      biasedCandidates = candidates;

    count = Mathf.Min(count, biasedCandidates.Count);
    for (int i = 0; i < count; i++)
    {
      int index = Random.Range(0, biasedCandidates.Count);
      result.Add(biasedCandidates[index]);
      biasedCandidates.RemoveAt(index);
    }

    return result;
  }

  private static List<AugmentDefinition> FilterByMinTier(
    List<AugmentDefinition> source,
    int minTier)
  {
    var filtered = new List<AugmentDefinition>();
    foreach (var def in source)
    {
      if ((int)def.tier >= minTier)
        filtered.Add(def);
    }

    return filtered;
  }
}
