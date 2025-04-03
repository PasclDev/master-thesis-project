using UnityEngine;

public class UIButtonActions : MonoBehaviour
{
    public void OnLoadLevelButton(int levelIndex)
    {
        Debug.Log("UIButtonActions: Load Level " + levelIndex);
        LevelManager.instance.LoadLevel(levelIndex, false);
        UIManager.instance.HideManageLevelUI();
    }
    public void OnRestartLevelButton()
    {
        Debug.Log("UIButtonActions: Restart Level");
        LevelManager.instance.LoadLevel(LevelManager.instance.currentLevel, false);
        UIManager.instance.HideManageLevelUI();
    }
    public void OnQuitApplicationButton()
    {
        Debug.Log("UIButtonActions: Quit Application");
        Application.Quit();
        //Stop runtime in editor mode
        if(Application.isEditor)
        {
            #if UNITY_EDITOR // UnityEditor is not available in build, so a preprocessor directive is used
                UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
    }
}
