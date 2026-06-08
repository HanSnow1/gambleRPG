using UnityEngine;

[CreateAssetMenu(menuName = "GambleRogue/Relic Definition", fileName = "Relic_")]
public class RelicDefinition : ScriptableObject
{
  public string relicId = "relic_default";
  public string displayName = "Unknown Relic";
  [TextArea(2, 3)] public string description = "";
  public RelicRarity rarity = RelicRarity.Common;
  public RelicStatType statType = RelicStatType.MaxHpFlat;
  public float baseStatValue = 10f;
}

public enum RelicRarity
{
  Common = 0,
  Uncommon = 1,
  Rare = 2
}
