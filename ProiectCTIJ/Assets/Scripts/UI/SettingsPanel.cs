using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    [Header("UI References")]
    public GameObject settingsPanel;
    public Slider volumeSlider;
    public GameObject[] menuButtons; // Array cu butoanele PLAY, SETTINGS, QUIT
    public GameObject menuBackground; // Imaginea de fundal cu AGE OF LANES
    
    private const string VOLUME_PREF = "MasterVolume";

    void Start()
    {
        // Ascunde panoul la început
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        
        // Încarcă volumul salvat
        LoadVolume();
        
        // Adaugă listener pentru slider
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
        
        // Ascunde butoanele meniului
        foreach (GameObject button in menuButtons)
        {
            if (button != null)
            {
                button.SetActive(false);
            }
        }
        
        // Ascunde fundalul meniului
        if (menuBackground != null)
        {
            menuBackground.SetActive(false);
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        
        // Arată din nou butoanele meniului
        foreach (GameObject button in menuButtons)
        {
            if (button != null)
            {
                button.SetActive(true);
            }
        }
        
        // Arată din nou fundalul meniului
        if (menuBackground != null)
        {
            menuBackground.SetActive(true);
        }
    }

    public void SetVolume(float volume)
    {
        // Setează volumul global (0 = mut, 1 = maxim)
        AudioListener.volume = volume;
        
        // Salvează volumul
        PlayerPrefs.SetFloat(VOLUME_PREF, volume);
        PlayerPrefs.Save();
    }

    void LoadVolume()
    {
        // Încarcă volumul salvat (default 1 = maxim)
        float savedVolume = PlayerPrefs.GetFloat(VOLUME_PREF, 1f);
        
        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
        }
        
        SetVolume(savedVolume);
    }
}
