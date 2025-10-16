using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EndingDataContainer", menuName = "Scriptable Objects/EndingDataContainer")]
public class EndingDataContainer : ScriptableObject
{
    public List<EndingScripts> endingList;
}
[System.Serializable]
public class EndingScripts
{
    public CandyName candyName;

    [Header("Correct")]
    [TextArea] public string correctHeadline;
    [TextArea] public string correctEndingText;
    [TextArea] public string correctComment;
    public Sprite correctImage;

    [Header("Wrong")]
    [TextArea] public string wrongHeadline;
    [TextArea] public string wrongEndingText;
    [TextArea] public string wrongComment;
    public Sprite wrongImage;
}