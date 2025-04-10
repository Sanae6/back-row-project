using System.Collections.Generic;
using UnityEngine;

public class Domino : MonoBehaviour
{
    private bool m_HasCollided = false;

    [SerializeField]
    private List<AudioClip> m_InstantiateSounds = new();

    [SerializeField]
    private AudioClip m_ToppleSound;

    [SerializeField]
    private Renderer m_Renderer;

    // or if it has collided with a valid domino.
    //
    // This idea is that we have a chain of validity going down
    // the cascade. The goal domino is only validly toppled if toppled
    // by a valid domino.
    [HideInInspector]
    public bool isValid = false;
    private bool m_IsCannonBall = false;

    void Start()
    {
        if (LevelManager.Instance.PositionIsInStartingArea(transform.position))
        {
            gameObject.layer = 0;
            isValid = true;
        }

        if (m_InstantiateSounds.Count != 0)
        {
            AudioSource.PlayClipAtPoint(m_InstantiateSounds[Random.Range(0, m_InstantiateSounds.Count)], transform.position);
        }

        m_IsCannonBall = GetComponent<Domino>() == null; // TODO
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            if (!m_IsCannonBall) // TODO: better solution for this
                gameObject.transform.parent.parent = collision.gameObject.transform.parent;
        }

        if (m_HasCollided)
            return;

        if (collision.gameObject.CompareTag("Domino"))
        {
            bool otherIsValid = collision.gameObject.GetComponent<Domino>().isValid;

            if (!otherIsValid && !isValid)
            {
                LevelManager.Instance.RegisterInvalidTopple(collision.GetContact(0).point);
                if (m_Renderer)
                    m_Renderer.material.color = Color.red;
                return;
            }

            OnValidCollision();
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            if (m_IsCannonBall)
                return;

            gameObject.transform.parent.parent = DominoManager.Instance.gameObject.transform;
        }
    }

    public void OnValidCollision()
    {
        if (LevelManager.Instance.GetLevelState() == LevelState.Valid)
            LevelManager.Instance.RegisterBeginTopple();

        Color c = Color.HSVToRGB(DominoManager.Instance.GetNextHue(), 1.0f, 1.0f);

        if (m_Renderer)
        {
            m_Renderer.material.color = c;
            m_Renderer.material.SetColor("_EmissionColor", c);
            m_Renderer.material.EnableKeyword("_EMISSION");
        }

        isValid = true;
        m_HasCollided = true;

        if (m_ToppleSound)
        {
            AudioSource.PlayClipAtPoint(m_ToppleSound, transform.position);
        }
        else
        {
            Debug.LogWarning("Domino has no topple sound assigned.");
        }
    }

    public void ResetValidity()
    {
        isValid = false;
        m_HasCollided = false;
    }
}
