using System.Collections.Generic;
using TMPro;
using Unity.Tutorials.Core.Editor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

//Gets loaded if level 0 is loaded in the level manager
public class TutorialManager : MonoBehaviour
{
    public GameObject grabbableObject1;
    public GameObject fillableObject1;
    public GameObject grabbableObjectGroup;
    public GameObject grabbableObject2;
    public GameObject fillableObject2;
    public TextMeshProUGUI tutorialText;
    public static TutorialManager instance;
    public int tutorialStep = 0;

    private string[] tutorialTexts = new string[] {
        "Mit den <color=#FF5C55>Rot-markierten Tasten</color> an den Controllern kannst du farbige Formen, auch \"Farbform\" genannt, aufsammeln.",
        "Solange du die Taste gedückt hältst, bleibt die Form in deiner Hand.\nDrücke die <color=#FFE455>Gelb-markierte Taste</color> des Controllers, der die Farbform hält, um sie durchsichtig zu machen.",
        "Gut gemacht! In jedem Level muss die Gitterform mit den Farbformen gefüllt werden.\n Bewege nun die Farbform in die Gitterform.",
        "In späteren Leveln kann man schnell den Überblick verlieren. Drücke die <color=#FFE455>Gelb-markierte Taste</color> des Controllers, um alle Farbformen in der Gitterform durchsichtig zu machen und alle freien Felder zu markieren.",
        "Super! um die Höhe des Levels anzupassen, greif die Kugel unter der Füllform und ziehe sie nach oben oder unten.",
        "Das war's mit der Einführung! Um die Einführung zu beenden, platziere die letzte Füllform in die Gitterform."
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
            (tutorialStep == 3 && logString == "Fillable: Highlight activated") ||
            (tutorialStep == 4 && logString == "HeightInteractable: Grabbed") ||
            (tutorialStep == 5 && logString == "LevelManager: Fillables filled!"))
        {
            NextTutorialStep();
        }
    }

    private bool RegexMatch(string input, string pattern)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(input, pattern);
    }
    void Awake()
    {
        Debug.Log("TutorialManager: Awake");
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateTutorialText();
        InitializeGameObjects();

    }
    private void UpdateTutorialText(){
        tutorialText.text = tutorialTexts[tutorialStep];
    }
    public void NextTutorialStep(){
        tutorialStep++;
        switch(tutorialStep){
            case 2:
                fillableObject1.SetActive(true);
                break;
            case 3:
                fillableObject1.SetActive(false);
                grabbableObject1.GetComponent<GrabbableManager>().Despawn();
                fillableObject2.SetActive(true);
                grabbableObjectGroup.SetActive(true);
                break;
            case 5:
                grabbableObject2.SetActive(true);
                break;
            case 6:
                LevelManager.instance.TutorialFinished();
                Destroy(gameObject);
                break;
        }
        UpdateTutorialText();
    }

    // Update is called once per frame
    private void InitializeGameObjects(){
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

        // Initialize
        grabbableObject1.GetComponent<GrabbableManager>().Initialize(grabbableObject1.GetComponent<GrabbableManager>().grabbable, 0.1f);
        grabbableObject2.GetComponent<GrabbableManager>().Initialize(grabbableObject2.GetComponent<GrabbableManager>().grabbable, 0.1f);
        // Grabbable Voxelcontent
        grabbableObject1.GetComponent<GrabbableManager>().grabbable.voxels = new int[3][][]{
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
        grabbableObject2.GetComponent<GrabbableManager>().grabbable.voxels = new int[2][][]{
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
        fillableObject1.GetComponent<FillableManager>().Initialize(transform.position, new Vector3Int(3,3,3),0.1f, transform.parent.GetComponent<VoxelMeshGenerator>());
        fillableObject2.GetComponent<FillableManager>().Initialize(transform.position, new Vector3Int(3,3,3),0.08f, transform.parent.GetComponent<VoxelMeshGenerator>());
        // Fillable fillable content
        fillableObject2.GetComponent<FillableManager>().fillableGrid = new int[3][][]{
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
        fillableObject2.GetComponent<FillableManager>().currentGrabbableObjects = grabbableManagers;

    }

}
