#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
static class PlayOnLaunch
{
    static PlayOnLaunch()
    {
        if (System.Environment.GetEnvironmentVariable("GAMBLE_AUTO_PLAY") != "1")
            return;

        EditorApplication.update += TryEnterPlayMode;
    }

    static void TryEnterPlayMode()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            return;

        EditorApplication.update -= TryEnterPlayMode;

        if (!EditorApplication.isPlayingOrWillChangePlaymode)
            EditorApplication.isPlaying = true;
    }
}
#endif
