using UnityEngine;

[CreateAssetMenu(menuName = "GambleRogue/Augment Definition", fileName = "Augment_")]
public class AugmentDefinition : ScriptableObject
{
  [Header("Identity")]
  public string augmentId = "aug_default";
  public string displayName = "Unknown Augment";
  [TextArea(2, 4)] public string shortDescription = "";
  [TextArea(2, 5)] public string[] levelDescriptions = new string[3];

  [Header("Tier")]
  public AugmentTier tier = AugmentTier.Silver;
  public bool alsoOfferInPrismaticPool;

  [Header("Progression")]
  [Tooltip("Max level = 1 + number of + in design doc. Capped at 3 for this project.")]
  public int maxLevel = 3;
  [Tooltip("If true, cannot appear in draft after reaching max level.")]
  public bool removeFromPoolAtMaxLevel = true;

  [Header("Effect")]
  public AugmentEffectType effectType = AugmentEffectType.FloorMaxHpPercent;
  [Tooltip("Primary value per level (e.g. percent, chance). Index 0 = Lv1.")]
  public float[] valuesPerLevel = { 30f, 40f, 50f };
  [Tooltip("Optional secondary value per level (e.g. bonus damage %, heal %).")]
  public float[] secondaryValuesPerLevel;

  public float GetValue(int level) =>
    valuesPerLevel == null || valuesPerLevel.Length == 0
      ? 0f
      : valuesPerLevel[Mathf.Clamp(level - 1, 0, valuesPerLevel.Length - 1)];

  public float GetSecondaryValue(int level)
  {
    if (secondaryValuesPerLevel == null || secondaryValuesPerLevel.Length == 0)
      return 0f;
    return secondaryValuesPerLevel[Mathf.Clamp(level - 1, 0, secondaryValuesPerLevel.Length - 1)];
  }

  public string GetDescriptionForLevel(int level)
  {
    if (levelDescriptions != null && levelDescriptions.Length > 0)
    {
      int i = Mathf.Clamp(level - 1, 0, levelDescriptions.Length - 1);
      if (!string.IsNullOrWhiteSpace(levelDescriptions[i]))
        return levelDescriptions[i];
    }
    return shortDescription;
  }
}
