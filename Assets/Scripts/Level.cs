using UnityEngine;

[System.Serializable]
public class LevelStage
{
	public string name = "";
	public string bossText;
	public string bossTalkFace;
	public string bossNormalFace;
	public string placables;

	[Space]

	public bool enableMinHeight;
	public float minHeight;
}

[CreateAssetMenu(fileName = "Level", menuName = "ScriptableObjects/Level", order = 1)]
public class Level : ScriptableObject
{
	public LevelStage[] stages;
	public string levelEndText;
}