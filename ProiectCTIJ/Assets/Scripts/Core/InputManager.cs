using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private Keyboard keyboard;

    private void Start()
    {
        keyboard = Keyboard.current;
    }

    void Update()
    {
        if (keyboard == null || GameManager.Instance.isGameOver) return;

        // Spawn Units
        if (keyboard.digit1Key.wasPressedThisFrame) UnitSpawner.Instance.SpawnUnit(0, Team.Player);
        if (keyboard.digit2Key.wasPressedThisFrame) UnitSpawner.Instance.SpawnUnit(1, Team.Player);
        if (keyboard.digit3Key.wasPressedThisFrame) UnitSpawner.Instance.SpawnUnit(2, Team.Player);

        // Abilities
        if (keyboard.qKey.wasPressedThisFrame) UseRallyAbility();

        // UI
        if (keyboard.uKey.wasPressedThisFrame) ToggleUpgradePanel();
        if (keyboard.escapeKey.wasPressedThisFrame) GameManager.Instance.TogglePause();
    }

    private void UseRallyAbility()
    {
        Debug.Log("Rally Ability Activated! +20% Attack Rate for 6s");
        // Implementation would involve a temporary buff to all player units
    }

    private void ToggleUpgradePanel()
    {
        Debug.Log("Toggling Upgrade Panel");
        // Implementation would involve UI Manager
    }
}
