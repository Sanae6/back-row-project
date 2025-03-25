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

    private List<DominoCurve> m_Curves = new();
    private List<GameObject> m_Singles = new();
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

            if (m_Curves.Count == 0 && m_Singles.Count == 0)
            {
                yield return new WaitForSeconds(seconds);
                continue;
            }

            m_LastValidState.Clear();

            for (int i = 0; i < m_Curves.Count; i++)
            {
                foreach (Transform child in m_Curves[i].transform)
                {
                    m_LastValidState.Add(
                        new PosRot()
                        {
                            Pos = child.transform.position,
                            Rot = child.transform.rotation,
                        }
                    );
                }
            }

            for (int i = 0; i < m_Singles.Count; i++)
            {
                m_LastValidState.Add(
                    new PosRot()
                    {
                        Pos = m_Singles[i].transform.position,
                        Rot = m_Singles[i].transform.rotation,
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

        GameObject curveObj = new GameObject("DominoCurve");
        curveObj.AddComponent<DominoCurve>();

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

                Instantiate(m_DominoPrefab, spawnPos, rotation, curveObj.transform);

                targetDistance += m_DominoDistance;
                yield return new WaitForSeconds(0.01f); // adjust as needed
            }
            accumulatedDistance += segment;
            i++;
        }
    }

    public void RegisterCurve(DominoCurve curve)
    {
        m_Curves.Add(curve);
    }

    public void RevertToLastValidState()
    {
        foreach (PosRot pr in m_LastValidState)
        {
            m_Singles.Add(Instantiate(m_DominoPrefab, pr.Pos, pr.Rot));
        }
    }

    public void ClearDominos()
    {
        Debug.Log(
            $"Domino manager clearing {m_Curves.Count} curves and {m_Singles.Count} singles."
        );

        StopAllCoroutines();

        for (int i = 0; i < m_Curves.Count; i++)
        {
            Destroy(m_Curves[i].gameObject);
        }

        for (int i = 0; i < m_Singles.Count; i++)
        {
            Destroy(m_Singles[i]);
        }

        m_Curves.Clear();
        m_Singles.Clear();

        StartCoroutine(WatchForValidDominoStateEvery(0.5f));
    }

    public void InstantiateSingle(Vector3 pos, Quaternion rot)
    {
        GameObject dom = Instantiate(m_DominoPrefab, pos, rot);
        m_Singles.Add(dom);
    }

    public void DeleteSingle(Domino dom)
    {
        m_Singles.Remove(dom.gameObject);
        Destroy(dom.gameObject);
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
