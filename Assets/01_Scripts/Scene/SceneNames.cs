using System.Collections.Generic;

public static class SceneNames
{
    private static readonly Dictionary<EScene, string> sceneTable =
        new Dictionary<EScene, string>()
        {
            { EScene.Title, "LobbyScene" },
            //{ EScene.Game, "HB_Scene" }
            { EScene.Game, "GamePlayScene" }
        };

    public static string GetSceneName(EScene sceneType)
    {
        return sceneTable[sceneType];
    }
}
