using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
    public int tutorialStep = 0;

    private string[] tutorialTexts = new string[] {
        "Mit den <color=#FF5C55>Rot-markierten Greiftasten</color> an den Controllern kannst du \"Farbformen\" aufsammeln.",
        "Halte eine Farbform mit der <color=#FF5C55>Greiftaste</color> in deiner Hand und drücke währenddessen die <color=#FFE455>Gelb-markierte Triggertaste</color> der gleichen Hand, um die Farbform durchsichtig zu machen.",
        "Gut gemacht! In jedem Level muss die Gitterform mit den Farbformen gefüllt werden.\n Bewege nun die Farbform in die Gitterform.",
        "Manchmal kann es schwer sein, Objekte zu rotieren.\nStrecke deinen Arm aus, <color=#FF5C55>Greife</color> die Farbform und ziehe dann deinen Arm zu dir, um die Farbform besser rotieren zu können. Packe sie dann in die Gitterform.",
        "In späteren Leveln kann man schnell den Überblick verlieren.\nDrücke die <color=#FFE455>Gelb-markierte Taste</color> des Controllers, um alle Farbformen in der Gitterform durchsichtig zu machen und alle freien Felder zu markieren.",
        "Super! Um die Höhe des Levels anzupassen, greife die Kugel unter der Farbform und ziehe sie nach oben oder unten.",
        "Das war es mit der Einführung!\nUm die Einführung zu beenden, platziere die letzte Farbform in die Gitterform."
    };

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
        heightInteractableObject = GameObject.Find("HeightInteractableObject");
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
        UpdateTutorialText();
        InitializeGameObjects();

    }
    private void UpdateTutorialText()
    {
        if (tutorialStep < tutorialTexts.Length)
        {
            tutorialText.text = tutorialTexts[tutorialStep];
        }
    }
    public void NextTutorialStep()
    {
        StatisticManager.instance.levelStatistic.tutorialStep = tutorialStep;
        StatisticManager.instance.WriteLevelLog();
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
        fillableObject1.GetComponent<FillableManager>().Initialize(transform.position, new Vector3Int(3, 3, 3), 0.1f, transform.parent.GetComponent<VoxelMeshGenerator>());
        fillableObject2.GetComponent<FillableManager>().Initialize(transform.position, new Vector3Int(3, 3, 3), 0.1f, transform.parent.GetComponent<VoxelMeshGenerator>());
        fillableObject3.GetComponent<FillableManager>().Initialize(transform.position, new Vector3Int(3, 3, 3), 0.08f, transform.parent.GetComponent<VoxelMeshGenerator>());
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
