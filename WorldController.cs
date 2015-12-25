using UnityEngine;

public class WorldController : MonoBehaviour
{
    public static string GamePath = "W:\\! GAMES !\\Mafia Game\\";
    public const float WorldScale = 0.254f;

    public void Start()
    {
		_4DSSceneLoader.LoadModel("\\MISSIONS\\MISE08-HOTEL\\scene");
    }
}
