using System.Collections.Generic;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class LevelButtonManager : MonoBehaviour
{
    public GameObject LevelButtonPrefab;
    public GameObject NextPageButton;
    public GameObject PreviousPageButton;
    private LevelDataProvider levelDataProvider;
    private int currentPage = 0;
    private int pageCount = 0;
    int levelsPerRow = 5;
    int rows = 4;
    List<GameObject> pages = new List<GameObject>();
    void Start()
    {
        
        levelDataProvider = GameObject.Find("LevelDataProvider").GetComponent<LevelDataProvider>();
        if (levelDataProvider.levelCollection == null || levelDataProvider.levelCollection.levels.Count == 0)
        {
            Debug.LogError("LevelButtonManager: Error - No levels found in LevelDataProvider!");
            return;
        }
        InitializeLevelButtons();
        PreviousPageButton.SetActive(false);
    }
    private void InitializeLevelButtons()
    {

        int levelsPerPage = levelsPerRow * rows;
        int totalLevels = LevelDataProvider.instance.levelCollection.levels.Count;
        pageCount = Mathf.CeilToInt((float)totalLevels / levelsPerPage);
        for (int i = 0; i < pageCount; i++)
        {
            GameObject page = new GameObject("Page_" + (i+1), typeof(RectTransform));
            page.transform.SetParent(transform, false);
            if (i > 0) page.SetActive(false);
            this.pages.Add(page);
        }
        for (int i = 1; i < totalLevels; i++)
        {
            int levelIndex = i; // Local copy to avoid listener reference to change as well (all buttons would lead to the last level)
            int pageIndex = (i-1) / levelsPerPage; // 18 = 0; 21 = 1
            int posX = -120 + ((i-1) % levelsPerRow * 60); // Position X is -120, -60, 0, 60, 120
            int posY = 90 - ((i-1) / levelsPerRow % rows * 60); // Position Y is 90, 30, -30, -90
            GameObject levelButton = Instantiate(LevelButtonPrefab, pages[pageIndex].transform);
            levelButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(posX, posY);
            levelButton.GetComponentInChildren<TextMeshProUGUI>().text = i.ToString();
            levelButton.GetComponent<Button>().onClick.AddListener(() => 
                levelButton.GetComponent<UIButtonActions>().OnLoadLevelButton(levelIndex));
        }
    }
    public void ChangeLevelPage(bool nextPage)
    {
        Debug.Log("LevelButtonManager: Changing Level Page. Next Page: " +(currentPage + (nextPage ? 1 : -1)));
        if (nextPage ? currentPage < pageCount - 1 : currentPage > 0)
        {
            pages[currentPage].gameObject.SetActive(false);
            currentPage = nextPage ? currentPage + 1 : currentPage - 1;
            pages[currentPage].gameObject.SetActive(true);
        }
        UpdatePageButtons();
    }
    private void UpdatePageButtons()
    {
        PreviousPageButton.SetActive(currentPage > 0);
        NextPageButton.SetActive(currentPage < pageCount - 1);
    } 

}
