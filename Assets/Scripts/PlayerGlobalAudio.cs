using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGlobalAudio : MonoBehaviour
{
    [SerializeField]
    private List<AudioClip> m_Songs = new();

    private AudioSource m_AudioSource;

    private int m_CurrentSongIndex = 0;
    void Awake()
    {
        m_AudioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        StartCoroutine(SongLoop());
    }

    IEnumerator SongLoop()
    {
        while (true)
        {
            // Fade music maybe??
            m_AudioSource.clip = m_Songs[m_CurrentSongIndex];
            m_AudioSource.Play();

            yield return new WaitForSeconds(m_Songs[m_CurrentSongIndex].length);

            m_CurrentSongIndex++;
            m_CurrentSongIndex %= m_Songs.Count;
        }
    }
}
