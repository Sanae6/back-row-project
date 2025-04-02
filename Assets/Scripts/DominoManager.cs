using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DominoManager : MonoBehaviour
{
    [HideInInspector]
    public static DominoManager Instance;

    [SerializeField]
    private GameObject m_DominoPrefab;

    [SerializeField]
    private float m_DominoDistance;

    [SerializeField]
    private float m_ColorGradientStep;

    private List<Domino> m_Dominos = new();
    private List<PosRot> m_LastValidState = new();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        StartCoroutine(WatchForValidDominoStateEvery(0.5f));
    }

    public void SpawnCurve(List<Vector3> curve)
    {
        StartCoroutine(SpawnDominosAlongCurve(curve));
    }

    IEnumerator WatchForValidDominoStateEvery(float seconds)
    {
        while (true)
        {
            if (LevelManager.Instance.GetLevelState() != LevelState.Valid)
            {
                yield return new WaitForSeconds(seconds);
                continue;
            }

            if (m_Dominos.Count == 0)
            {
                yield return new WaitForSeconds(seconds);
                continue;
            }

            m_LastValidState.Clear();

            for (int i = 0; i < m_Dominos.Count; i++)
            {
                if (m_Dominos[i] == null)
                {
                    Debug.LogError("[BIG ERROR] Domino manager: Instance of domino in m_Dominos is null... Clearing state....");
                    ClearDominos();
                    continue;
                }

                m_LastValidState.Add(
                    new PosRot()
                    {
                        Pos = m_Dominos[i].transform.position - new Vector3(0, 0.05f, 0),
                        Rot = m_Dominos[i].transform.parent.localRotation,
                    }
                );
            }

            yield return new WaitForSeconds(seconds);
        }
    }

    IEnumerator SpawnDominosAlongCurve(List<Vector3> curvePoints)
    {
        if (curvePoints == null || curvePoints.Count < 2)
            yield break;

        float accumulatedDistance = 0f;
        float targetDistance = m_DominoDistance;
        int i = 1;
        while (i < curvePoints.Count)
        {
            float segment = Vector3.Distance(curvePoints[i - 1], curvePoints[i]);
            while (accumulatedDistance + segment >= targetDistance)
            {
                float t = (targetDistance - accumulatedDistance) / segment;
                Vector3 spawnPos = Vector3.Lerp(curvePoints[i - 1], curvePoints[i], t);

                // use the segment's direction as an approximation for the tangent

                Vector3 tangent;

                if (i == 0)
                    tangent = (curvePoints[1] - curvePoints[0]).normalized;
                else if (i == curvePoints.Count - 1)
                    tangent = (curvePoints[i] - curvePoints[i - 1]).normalized;
                else
                    tangent = (curvePoints[i + 1] - curvePoints[i - 1]).normalized;

                Quaternion rotation = Quaternion.LookRotation(tangent);

                Domino dom = Instantiate(m_DominoPrefab, spawnPos, rotation).GetComponentInChildren<Domino>();

                if (dom == null)
                    Debug.LogError("Domino manager instantiated domino but unable to get Domino component...");

                m_Dominos.Add(dom);

                targetDistance += m_DominoDistance;
                yield return new WaitForSeconds(0.01f); // adjust as needed
            }
            accumulatedDistance += segment;
            i++;
        }
    }

    public void RevertToLastValidState()
    {
        foreach (PosRot pr in m_LastValidState)
        {
            m_Dominos.Add(Instantiate(m_DominoPrefab, pr.Pos, pr.Rot, transform).GetComponentInChildren<Domino>());
        }
    }

    public void ClearDominos()
    {
        Debug.Log($"Domino manager clearing {m_Dominos.Count} dominos.");

        StopAllCoroutines();

        for (int i = 0; i < m_Dominos.Count; i++)
        {
            if (m_Dominos[i] != null)
                Destroy(m_Dominos[i].transform.parent.gameObject);
        }

        m_Dominos.Clear();

        StartCoroutine(WatchForValidDominoStateEvery(0.5f));
    }

    public void InstantiateSingle(Vector3 pos, Quaternion rot)
    {
        GameObject go = Instantiate(m_DominoPrefab, pos, rot, transform);
        Domino domino = go.GetComponentInChildren<Domino>();
        if (domino != null)
        {
            m_Dominos.Add(domino);
        }
        else
        {
            Debug.LogError("Unable to get domino component on instantiated single");
        }
    }

    public void RegisterSingle(Domino domino)
    {
        if (!m_Dominos.Contains(domino))
        {
            m_Dominos.Add(domino);
        }
        else
        {
            Debug.LogWarning(
                "Attempt to register single domino but already registered. Ignoring..."
            );
        }
    }

    public void DeleteSingle(Domino dom)
    {
        m_Dominos.Remove(dom);
        Destroy(dom.gameObject.transform.parent.gameObject);
    }

    private float m_CurrentColorHue = 0;

    public float GetNextHue()
    {
        m_CurrentColorHue += m_ColorGradientStep;
        m_CurrentColorHue %= 1;
        return m_CurrentColorHue;
    }

    private struct PosRot
    {
        public Vector3 Pos;
        public Quaternion Rot;
    }
}
