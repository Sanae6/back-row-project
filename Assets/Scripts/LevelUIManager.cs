using UnityEngine;

public class LevelUIManager : MonoBehaviour
{
    [SerializeField]
    private GameObject m_ResetLevelButton;

    [SerializeField]
    private GameObject m_RestartLevelButton;

    void Awake()
    {
        m_ResetLevelButton
            .GetComponentInChildren<PressableButton>()
            .ButtonPressed.AddListener(OnResetButtonPressed);

        m_RestartLevelButton
            .GetComponentInChildren<PressableButton>()
            .ButtonPressed.AddListener(OnRestartButtonPressed);
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
}
