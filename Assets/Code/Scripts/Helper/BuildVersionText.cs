using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class BuildVersionText : MonoBehaviour
{
	private static string prefix = "v";
	private TextMeshProUGUI versionLabel;

	private void Awake()
	{
		versionLabel = GetComponent<TextMeshProUGUI>();
		versionLabel.text = $"{prefix}{Application.version}";
	}

}
