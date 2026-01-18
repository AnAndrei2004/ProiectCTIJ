using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private Keyboard keyboard;

    // Initializeaza dispozitivul de input.
    private void Start()
    {
        keyboard = Keyboard.current;
    }

    // Proceseaza inputul de la tastatura.
    void Update()
    {
        if (keyboard == null || GameManager.Instance.isGameOver) return;

        // Spawn unitati
        if (keyboard.digit1Key.wasPressedThisFrame) UnitSpawner.Instance.SpawnUnit(0, Team.Player);
        if (keyboard.digit2Key.wasPressedThisFrame) UnitSpawner.Instance.SpawnUnit(1, Team.Player);
        if (keyboard.digit3Key.wasPressedThisFrame) UnitSpawner.Instance.SpawnUnit(2, Team.Player);

        // Abilitati
        if (keyboard.qKey.wasPressedThisFrame) UseRallyAbility();

        // UI
        if (keyboard.uKey.wasPressedThisFrame) ToggleUpgradePanel();
        if (keyboard.escapeKey.wasPressedThisFrame) GameManager.Instance.TogglePause();
    }

    // Placeholder pentru abilitatea Rally.
    private void UseRallyAbility()
    {
        Debug.Log("Rally Ability Activated! +20% Attack Rate for 6s");
        // Implementarea ar aplica un buff temporar la unitati
    }

    // Placeholder pentru deschiderea panoului de upgrade.
    private void ToggleUpgradePanel()
    {
        Debug.Log("Toggling Upgrade Panel");
        // Implementarea ar folosi un manager UI
    }
}
