using UnityEngine;

[CreateAssetMenu(fileName = "TutorialData", menuName = "Scriptable Objects/TutorialData")]
public class TutorialData : ScriptableObject
{
    public TriggerEvent StartTrigger;
    public string Text;
    public TriggerEvent EndTrigger;
}
