using UnityEngine;
using UnityEngine.Events;

public class ActivationPad : MonoBehaviour
{
    [HideInInspector]
    public UnityEvent PadActivated;

    private bool m_HasActivated = false;

    void Start()
    {
        LevelManager.Instance.RegisterActivationPad(this);
    }

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
                Debug.Log("Activation pad triggered by valid domino");
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
