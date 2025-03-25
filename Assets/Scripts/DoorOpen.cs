using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    [SerializeField]
    private float m_DoorCloseDelay = 5.0f;

    [SerializeField]
    private ActivationPad m_ActivationPad;

    private Animator m_Animator;

    void Start()
    {
        m_ActivationPad.PadActivated.AddListener(OnPadActivated);
        m_Animator = GetComponent<Animator>();
    }

    void OnPadActivated()
    {
        m_Animator.SetBool("ShouldBeOpen", true);
        Invoke(nameof(CloseDoor), m_DoorCloseDelay);
    }

    void CloseDoor()
    {
        m_Animator.SetBool("ShouldBeOpen", false);
    }
}
