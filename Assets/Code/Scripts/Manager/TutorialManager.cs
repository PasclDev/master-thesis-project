using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//Gets loaded if level 0 is loaded in the level manager
public class TutorialManager : MonoBehaviour
{
    public GameObject grabbableObject1;
    public GameObject fillableObject1;
    public GameObject grabbableObjectGroup;
    public GameObject grabbableObject2;
    public GameObject fillableObject2;
    public GameObject grabbableObject3;
    public GameObject fillableObject3;
    public GameObject heightInteractableObject;
    public TextMeshProUGUI tutorialText;
    public Image TextboxBackground;

    private Color textboxBackgroundColor;
    public int tutorialStep = 0;

    private string[] tutorialTexts = new string[] {
        "Mit der <color=#CF2821><b>Rot-markierten Greiftaste</b></color> am linken und rechten Controller kannst du \"Farbformen\" aufsammeln.",
        "Halte eine Farbform mit der <color=#CF2821><b>Greiftaste</b></color> in deiner Hand und drücke währenddessen die <color=#E3C420><b>Gelb-markierte Triggertaste</b></color> der gleichen Hand, um die Farbform durchsichtig zu machen.",
        "Gut gemacht! In jedem Level muss die Gitterbox mit den Farbformen gefüllt werden.\n Bewege nun die Farbform in die Gitterbox.",
        "Manchmal können Farbformen\nzu weit weg sein.\nStrecke deinen Arm aus, <color=#CF2821><b>greife</b></color> die Farbform und ziehe dann deinen Arm zu dir, um die Farbform in deine Hand zu ziehen.\n\nPacke sie dann in die Gitterbox.",
        "In späteren Leveln kann man schnell den Überblick verlieren.\nDrücke die <color=#E3C420><b>Gelb-markierte Taste</b></color> des Controllers, um alle Farbformen in der Gitterbox durchsichtig zu machen und alle freien Felder zu markieren.",
        "Super! Um die Höhe des Levels anzupassen, <color=#CF2821><b>greife</b></color> die Kugel unter der Farbform und ziehe sie nach oben oder unten.",
        "Das war es mit der Einführung!\nUm die Einführung zu beenden, platziere die letzte Farbform in die Gitterbox.\n\n<size=7><i>Tipp zum Rotieren:\n<color=#CF2821><b>Greife</b></color> die Farmform, drehe dein Handgelenk, lasse los, drehe zurück und <color=#CF2821>greife</b></color> erneut.</i></size>",
    };

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }
    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }
    // Checking Debug.Logs for specific messages to advance the tutorial. Not ideal implementation but is "cleaner" than injecting tutorial steps into the general game logic
    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if ((tutorialStep == 0 && RegexMatch(logString, @"Grabbable:.*has been picked up! In hand:.*")) ||
            (tutorialStep == 1 && RegexMatch(logString, @"Grabbable:.*has been activated!.*")) ||
            (tutorialStep == 2 && logString == "LevelManager: Fillables filled!") ||
            (tutorialStep == 3 && logString == "LevelManager: Fillables filled!") ||
            (tutorialStep == 4 && logString == "Fillable: Highlight activated") ||
            (tutorialStep == 5 && logString == "HeightInteractable: Grabbed") ||
            (tutorialStep == 6 && logString == "LevelManager: Fillables filled!"))
        {
            NextTutorialStep();
        }
    }

    private bool RegexMatch(string input, string pattern)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(input, pattern);
    }

    void Start()
    {
        textboxBackgroundColor = TextboxBackground.color;
        UpdateTutorialText();
        InitializeGameObjects();
        heightInteractableObject = LevelManager.instance.GetComponent<YAxisGrabInteractable>().attachTransform.gameObject; // Secure way to find it even when it is deactivated
        Debug.Log("TutorialManager: HeightInteractable found:" + heightInteractableObject.name);
        heightInteractableObject.SetActive(false);
        LevelManager.instance.ResetLevelHeight();

    }
    private void UpdateTutorialText()
    {
        if (tutorialStep < tutorialTexts.Length)
        {
            Debug.Log("Current Tutorial Step: " + tutorialStep);
            tutorialText.text = tutorialTexts[tutorialStep];
            Debug.Log("UpdateTutorialText: " + tutorialStep + " - of" + tutorialTexts.Length);
            StartCoroutine(AnimateTextboxBackground());
        }
    }
    IEnumerator AnimateTextboxBackground()
    {
        Debug.Log("AnimateTextboxBackground: " + tutorialStep);
        float elapsedTime = 0f;
        float duration = 0.4f; // Duration of the animation in seconds
        int blinkCount = 1; // Number of blinks
        Color targetColor = new Color(1, 1, 1, textboxBackgroundColor.a);

        // Animate the background color, blink twice in the target color, reverting to the start color
        while (elapsedTime < duration)
        {
            TextboxBackground.color = Color.Lerp(textboxBackgroundColor, targetColor, 0.5f * (1f + Mathf.Sin(blinkCount * 2f * Mathf.PI * (elapsedTime / duration) - Mathf.PI / 2f)));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

    }
    public void NextTutorialStep()
    {
        StatisticManager.instance.levelStatistic.tutorialStep = tutorialStep;
        StatisticManager.instance.WriteLevelLog();
        AudioManager.instance.Play("Tutorial_NextStep");
        tutorialStep++;
        switch (tutorialStep)
        {
            case 2:
                fillableObject1.SetActive(true);
                break;
            case 3:
                fillableObject1.SetActive(false);
                grabbableObject1.GetComponent<GrabbableManager>().Despawn();
                fillableObject2.SetActive(true);
                grabbableObject2.SetActive(true);
                break;
            case 4:
                fillableObject2.SetActive(false);
                grabbableObject2.GetComponent<GrabbableManager>().Despawn();
                fillableObject3.SetActive(true);
                grabbableObjectGroup.SetActive(true);
                break;
            case 5:
                heightInteractableObject.SetActive(true);
                heightInteractableObject.GetComponent<Outline>().enabled = true;
                break;
            case 6:
                grabbableObject3.SetActive(true);
                heightInteractableObject.GetComponent<Outline>().enabled = false;
                break;
            case 7:
                LevelManager.instance.TutorialFinished();
                Destroy(gameObject);
                break;
        }
        UpdateTutorialText();
    }


    private void InitializeGameObjects()
    {
        // Grabbables
        // Grabbable Texture
        Material normalMaterial = grabbableObject1.GetComponent<Renderer>().material;
        Material transparentMaterial = new Material(grabbableObject1.GetComponent<GrabbableManager>().transparentMaterial);
        Color newColor = grabbableObject1.GetComponent<GrabbableManager>().grabbable.GetColor();
        newColor.a = transparentMaterial.color.a;
        normalMaterial.color = newColor;
        transparentMaterial.color = newColor;
        grabbableObject1.GetComponent<Renderer>().material = normalMaterial;
        grabbableObject1.GetComponent<GrabbableManager>().transparentMaterial = transparentMaterial;
        grabbableObject2.GetComponent<Renderer>().material = normalMaterial;
        grabbableObject2.GetComponent<GrabbableManager>().transparentMaterial = transparentMaterial;
        grabbableObject3.GetComponent<Renderer>().material = normalMaterial;
        grabbableObject3.GetComponent<GrabbableManager>().transparentMaterial = transparentMaterial;

        // Initialize
        grabbableObject1.GetComponent<GrabbableManager>().Initialize(grabbableObject1.GetComponent<GrabbableManager>().grabbable, 0.1f);
        grabbableObject2.GetComponent<GrabbableManager>().Initialize(grabbableObject2.GetComponent<GrabbableManager>().grabbable, 0.1f);
        grabbableObject3.GetComponent<GrabbableManager>().Initialize(grabbableObject3.GetComponent<GrabbableManager>().grabbable, 0.1f);
        // Grabbable Voxelcontent
        int[][][] grabbableCubeVoxels = new int[3][][]{
            new int[3][]{
                new int[3]{1,1,1},
                new int[3]{1,1,1},
                new int[3]{1,1,1}
            },
            new int[3][]{
                new int[3]{1,1,1},
                new int[3]{1,1,1},
                new int[3]{1,1,1}
            },
            new int[3][]{
                new int[3]{1,1,1},
                new int[3]{1,1,1},
                new int[3]{1,1,1}
            }
        };
        grabbableObject1.GetComponent<GrabbableManager>().grabbable.voxels = grabbableCubeVoxels;
        grabbableObject2.GetComponent<GrabbableManager>().grabbable.voxels = grabbableCubeVoxels;
        grabbableObject3.GetComponent<GrabbableManager>().grabbable.voxels = new int[2][][]{
            new int[2][]{
                new int[2]{0,1},
                new int[2]{0,0},
            },
            new int[2][]{
                new int[2]{1,1},
                new int[2]{0,1},
            }
        };
        // Grabbable Group
        foreach (Transform child in grabbableObjectGroup.transform)
        {
            child.GetComponent<GrabbableManager>().Initialize(child.GetComponent<GrabbableManager>().grabbable, 0.08f);
        }
        // Fillables
        fillableObject1.GetComponent<FillableManager>().Initialize(fillableObject1.transform.position, new Vector3Int(3, 3, 3), 0.1f, transform.parent.GetComponent<VoxelMeshGenerator>());
        fillableObject2.GetComponent<FillableManager>().Initialize(fillableObject2.transform.position, new Vector3Int(3, 3, 3), 0.1f, transform.parent.GetComponent<VoxelMeshGenerator>());
        fillableObject3.GetComponent<FillableManager>().Initialize(fillableObject3.transform.position, new Vector3Int(3, 3, 3), 0.08f, transform.parent.GetComponent<VoxelMeshGenerator>());
        // Fillable fillable content
        fillableObject3.GetComponent<FillableManager>().fillableGrid = new int[3][][]{
            new int[3][]{
                new int[3]{1,1,0},
                new int[3]{1,0,0},
                new int[3]{1,1,1}
            },
            new int[3][]{
                new int[3]{1,1,1},
                new int[3]{1,1,0},
                new int[3]{1,1,1}
            },
            new int[3][]{
                new int[3]{1,1,1},
                new int[3]{1,1,1},
                new int[3]{1,1,1}
            }
        };
        List<GrabbableManager> grabbableManagers = new List<GrabbableManager>();
        foreach (Transform child in grabbableObjectGroup.transform)
        {
            grabbableManagers.Add(child.GetComponent<GrabbableManager>());
        }
        fillableObject3.GetComponent<FillableManager>().currentGrabbableObjects = grabbableManagers;

    }

}
