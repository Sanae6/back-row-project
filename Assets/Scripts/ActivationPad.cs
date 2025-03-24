using UnityEngine;
using UnityEngine.Events;

public class ActivationPad : MonoBehaviour
{
    [HideInInspector]
    public UnityEvent PadActivated;

    private bool m_HasActivated = false;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Activation Pad colliding!!");
        if (m_HasActivated)
            return;

        if (other.gameObject.CompareTag("Domino"))
        {
            bool otherIsValid = other.gameObject.GetComponent<Domino>().isValid;

            if (otherIsValid)
            {
                m_HasActivated = true;
                PadActivated.Invoke();
            }
        }
    }

    public void Reset()
    {
        m_HasActivated = false;
    }
}
