using UnityEngine;

public class LevelUIManager : MonoBehaviour
{
    [SerializeField] private GameObject m_ResetLevelButton;

    [SerializeField] private GameObject m_RestartLevelButton;
    [SerializeField] private GameObject m_MenuButton;

    void Awake()
    {
        if (m_ResetLevelButton)
            m_ResetLevelButton
                .GetComponentInChildren<PressableButton>()
                .ButtonPressed.AddListener(OnResetButtonPressed);

        if (m_RestartLevelButton)
            m_RestartLevelButton
                .GetComponentInChildren<PressableButton>()
                .ButtonPressed.AddListener(OnRestartButtonPressed);
        
        if (m_MenuButton)
            m_MenuButton
                .GetComponentInChildren<PressableButton>()
                .ButtonPressed.AddListener(OnMenuButtonPressed);
    }

    private void OnResetButtonPressed()
    {
        LevelManager.Instance.ResetLevel();
    }

    private void OnRestartButtonPressed()
    {
        LevelManager.Instance.ResetLevel();
        DominoManager.Instance.RevertToLastValidState();
    }

    private void OnMenuButtonPressed()
    {
        LevelSystem.Instance.SwitchToMainMenu();
    }
}