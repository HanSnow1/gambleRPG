using UnityEngine;

[CreateAssetMenu(menuName = "GambleRogue/Boss Definition", fileName = "BossDefinition")]
public class BossDefinition : ScriptableObject
{
  [Header("Identity")]
  public string bossId = "boss_default";
  public string displayName = "Unknown Boss";
  [TextArea] public string weaknessHint = "No hint.";
  [TextArea] public string riskPatternHintA = "Unknown pattern.";
  [TextArea] public string riskPatternHintB = "Unknown pattern.";
  public Sprite portrait;

  [Header("Combat")]
  public BossGambleType gambleType = BossGambleType.Dice;
  public int maxHp = 120;
  public int baseAttack = 10;
}
