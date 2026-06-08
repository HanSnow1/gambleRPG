using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

/// <summary>
/// LiberationSans는 한글 글리프가 없어 □로 깨집니다. 시작 시 한글 폰트를 주입합니다.
/// </summary>
public static class KoreanFontSetup
{
  private static TMP_FontAsset _mainFont;
  private static bool _installed;

  public static TMP_FontAsset MainFont => _mainFont;
  public static bool IsReady => _mainFont != null;

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
  private static void InstallBeforeSceneLoad()
  {
    if (_installed)
      return;

    _mainFont = CreateKoreanFont();
    if (_mainFont == null)
    {
      Debug.LogWarning("KoreanFontSetup: 한글 폰트를 찾지 못했습니다. LiberationSans를 계속 사용합니다.");
      return;
    }

    var latin = TMP_Settings.defaultFontAsset;
    if (latin != null && latin != _mainFont)
    {
      _mainFont.fallbackFontAssetTable ??= new List<TMP_FontAsset>();
      if (!_mainFont.fallbackFontAssetTable.Contains(latin))
        _mainFont.fallbackFontAssetTable.Add(latin);
    }

    TMP_Settings.defaultFontAsset = _mainFont;
    _installed = true;
    Debug.Log($"KoreanFontSetup: 한글 폰트 적용 — {_mainFont.faceInfo.familyName}");
  }

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  private static void ApplyToLoadedScene()
  {
    if (_mainFont == null)
      return;

    ApplyToAll(FindObjectsInactive.Include);
  }

  public static void ApplyToAll(FindObjectsInactive includeInactive = FindObjectsInactive.Exclude)
  {
    if (_mainFont == null)
      return;

    foreach (var tmp in Object.FindObjectsByType<TextMeshProUGUI>(includeInactive, FindObjectsSortMode.None))
    {
      if (tmp != null)
        tmp.font = _mainFont;
    }
  }

  public static void Apply(TMP_Text tmp)
  {
    if (_mainFont == null || tmp == null)
      return;

    tmp.font = _mainFont;
  }

  private static TMP_FontAsset CreateKoreanFont()
  {
    var prebuilt = Resources.Load<TMP_FontAsset>("Fonts/KoreanSDF");
    if (prebuilt != null)
      return prebuilt;

    TMP_FontAsset osFont = TrySystemFont("Apple SD Gothic Neo", "Regular")
      ?? TrySystemFont("AppleSDGothicNeo", "Regular")
      ?? TrySystemFont("Malgun Gothic", "Regular")
      ?? TrySystemFont("Noto Sans KR", "Regular")
      ?? TrySystemFont("Noto Sans CJK KR", "Regular");
    if (osFont != null)
      return osFont;

    foreach (string path in GetBundledFontPaths())
    {
      if (!File.Exists(path))
        continue;

      var bundled = TMP_FontAsset.CreateFontAsset(path, 0, 36, 9, GlyphRenderMode.SDFAA, 1024, 1024);
      if (bundled != null)
        return bundled;
    }

    return null;
  }

  private static TMP_FontAsset TrySystemFont(string family, string style)
  {
    try
    {
      return TMP_FontAsset.CreateFontAsset(family, style, 36);
    }
    catch
    {
      return null;
    }
  }

  private static string[] GetBundledFontPaths()
  {
    return new[]
    {
      Path.Combine(Application.streamingAssetsPath, "Fonts", "AppleSDGothicNeo.ttc"),
      Path.Combine(Application.dataPath, "Fonts", "AppleSDGothicNeo.ttc"),
      Path.Combine(Application.dataPath, "StreamingAssets", "Fonts", "AppleSDGothicNeo.ttc")
    };
  }
}
