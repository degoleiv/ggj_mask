using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // Add this line

public class MainMenuUI : MonoBehaviour
{
    public string startSceneName; // Set this in the inspector to the name of your game scene
    public GameObject settingsPanel;
    public GameObject mainMenuPanel;
    public Rotator rotator; // Assign this in the Inspector
    public Material dissolveMaterial; // Assign this in the Inspector

    void Start()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false); // Ensure settings panel is inactive initially
        
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true); // Ensure main menu panel is active initially

        if (rotator != null)
        {
            rotator.SetIsRotating(true); // Use the new method
        }
    }

    public void OnStartButtonClicked()
    {
        if (rotator != null)
        {
            rotator.StopRotation();
        }

        // Trigger dissolve effect
        StartCoroutine(DissolveObject());
        SceneManager.LoadScene(startSceneName);
    }

    public void OnContinueButtonClicked()
    {
        // Load the last save
        // Placeholder for loading logic
        Debug.Log("Load last save");
    }

    public void OnSettingsButtonClicked()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false); // Deactivate main menu panel

        if (settingsPanel != null)
            settingsPanel.SetActive(true); // Activate settings panel

        Debug.Log("settings panel activated");
    }

    public void OnBackButtonClicked()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false); // Deactivate settings panel

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true); // Activate main menu panel

        Debug.Log("main menu panel reactivated");
    }

    public void OnExitButtonClicked()
    {
        Application.Quit();
        Debug.Log("Bye");
    }

    private IEnumerator DissolveObject()
    {
        float dissolveProgress = 0.0f;
        while (dissolveProgress < 1.0f)
        {
            dissolveMaterial.SetFloat("_DissolveProgress", dissolveProgress);
            dissolveProgress += Time.deltaTime * 2.0f; // Adjust speed as needed
            yield return null;
        }
    }
}
