using UnityEngine;

public class InputManager : MonoBehaviour
{
    void Update()
    {
        if (GameManager.Instance.isGameOver) return;

        // Spawn Units
        if (Input.GetKeyDown(KeyCode.Alpha1)) UnitSpawner.Instance.SpawnUnit(0, Team.Player);
        if (Input.GetKeyDown(KeyCode.Alpha2)) UnitSpawner.Instance.SpawnUnit(1, Team.Player);
        if (Input.GetKeyDown(KeyCode.Alpha3)) UnitSpawner.Instance.SpawnUnit(2, Team.Player);

        // Abilities
        if (Input.GetKeyDown(KeyCode.Q)) UseRallyAbility();

        // UI
        if (Input.GetKeyDown(KeyCode.U)) ToggleUpgradePanel();
        if (Input.GetKeyDown(KeyCode.Escape)) GameManager.Instance.TogglePause();
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
