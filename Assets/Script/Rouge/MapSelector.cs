using UnityEngine;
using UnityEngine.SceneManagement;

public class MapSelector : MonoBehaviour
{
    [SerializeField] private string[] mapSceneNames;   // 在 Inspector 中填入场景名称

    public void StartGame()
    {
        if (mapSceneNames.Length == 0)
        {
            Debug.LogError("没有指定任何地图场景！");
            return;
        }
        int randomIndex = Random.Range(0, mapSceneNames.Length);
        string selectedMap = mapSceneNames[randomIndex];
        Debug.Log($"随机选择地图：{selectedMap}");
        SceneManager.LoadScene(selectedMap);
    }
}