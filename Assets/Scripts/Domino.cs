using System;
using UnityEngine;

public class Domino : MonoBehaviour
{
    [HideInInspector]
    public float hue;

    private bool m_HasCollided = false;

    void OnCollisionEnter(Collision collision)
    {
        if (m_HasCollided)
            return;

        if (collision.gameObject.CompareTag("Domino"))
        {
            Color c = Color.HSVToRGB(hue, 1.0f, 1.0f);
            Renderer r = GetComponent<Renderer>();

            r.material.color = c;
            r.material.SetColor("_EmissionColor", c);
            r.material.EnableKeyword("_EMISSION");

            m_HasCollided = true;
        } 
    }
}