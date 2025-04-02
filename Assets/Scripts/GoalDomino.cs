using System;
using Unity.VisualScripting;
using UnityEngine;

public class GoalDomino : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Quaternion m_StartingRotation;
    private Vector3 m_StartingPosition;

    [SerializeField]
    private float m_ToppleThreshold = 45;

    void Start()
    {
        LevelManager.Instance.RegisterGoalDomino(this);
        m_StartingRotation = transform.localRotation;
        m_StartingPosition = transform.localPosition;
    }

    private bool m_HasToppled = false;

    private bool m_ValidTopple = false; // Assumed to be false until we have collided with a valid domino

    void FixedUpdate()
    {
        if (m_HasToppled || !m_ValidTopple)
            return;

        if (Quaternion.Angle(m_StartingRotation, transform.localRotation) > m_ToppleThreshold)
        {
            LevelManager.Instance.RegisterGoalToppled(this);
            m_HasToppled = true;
        }
    }

    private bool m_HasCollided = false;

    void OnCollisionEnter(Collision collision)
    {
        if (m_HasCollided)
            return;

        if (collision.gameObject.CompareTag("Domino"))
        {
            m_HasCollided = true;
            bool otherIsValid = collision.gameObject.GetComponent<Domino>().isValid;

            if (!otherIsValid)
            {
                LevelManager.Instance.RegisterInvalidTopple(collision.GetContact(0).point);
                return;
            }
            m_ValidTopple = true;
        }
        // Handle edge cases where gold dominos are toppled by other gold dominos
        else if (collision.gameObject.CompareTag("GoalDomino"))
        {
            m_HasCollided = true;
            m_ValidTopple = true;
        }
    }

    public void Reset()
    {
        m_HasToppled = false;
        m_ValidTopple = false;
        m_HasCollided = false;

        transform.SetLocalPositionAndRotation(m_StartingPosition, m_StartingRotation);
    }
}
