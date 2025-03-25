using System;
using UnityEngine;

public class Domino : MonoBehaviour
{
    private bool m_HasCollided = false;

    // A domino is valid if it was instantiated in the starting area,
    // or if it has collided with a valid domino.
    //
    // This idea is that we have a chain of validity going down
    // the cascade. The goal domino is only validly toppled if toppled
    // by a valid domino.
    [HideInInspector]
    public bool isValid = false;

    void Start()
    {
        if (LevelManager.Instance.PositionIsInStartingArea(transform.position))
        {
            gameObject.layer = 0;
            isValid = true;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (m_HasCollided)
            return;

        if (collision.gameObject.CompareTag("Domino"))
        {
            bool otherIsValid = collision.gameObject.GetComponent<Domino>().isValid;

            if (!otherIsValid && !isValid)
            {
                LevelManager.Instance.RegisterInvalidTopple(collision.GetContact(0).point);
                GetComponent<Renderer>().material.color = Color.red;
                return;
            }

            OnValidCollision();
        }
    }

    public void OnValidCollision()
    {
        Color c = Color.HSVToRGB(DominoManager.Instance.GetNextHue(), 1.0f, 1.0f);
        Renderer r = GetComponent<Renderer>();

        r.material.color = c;
        r.material.SetColor("_EmissionColor", c);
        r.material.EnableKeyword("_EMISSION");

        isValid = true;
        m_HasCollided = true;
    }

    public void ResetValidity()
    {
        isValid = false;
        m_HasCollided = false;
    }
}
