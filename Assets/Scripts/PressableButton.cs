using System;
using UnityEngine;
using UnityEngine.Events;

public class PressableButton : MonoBehaviour
{
    [SerializeField] private AudioClip m_PressedSound;

    [SerializeField] private float m_PushActivationThreshold = 0.04f;

    private ConfigurableJoint m_ConfigurableJoint;

    private Vector3 m_OriginalPosition;

    [HideInInspector] public UnityEvent ButtonPressed;

    private void Awake()
    {
        OnEnable();
    }

    void OnEnable()
    {
        m_OriginalPosition = transform.localPosition;

        m_ConfigurableJoint = GetComponent<ConfigurableJoint>();
        if (m_ConfigurableJoint == null)
            Debug.LogError("Pressable button instantiated without configurable joint.");
    }

    private bool m_Pressed = false;
    void Update()
    {
        float dist = Vector3.Distance(m_OriginalPosition, transform.localPosition);

        if (!m_Pressed && dist > m_PushActivationThreshold)
        {
            m_Pressed = true;
            ButtonPressed.Invoke();
            if (m_PressedSound != null)
            {
                AudioSource.PlayClipAtPoint(m_PressedSound, transform.position);
            }
        }

        if (transform.localPosition.y > m_OriginalPosition.y)
        {
            transform.localPosition = m_OriginalPosition;
        }

        if (dist < 0.005f)
        {
            m_Pressed = false;
        }
    }
}
