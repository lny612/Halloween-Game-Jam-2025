using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ChildProfileContainer", menuName = "Child/ChildProfile")]
public class ChildProfileContainer : ScriptableObject
{
    public List<ChildProfile> childProfileList;
}
[System.Serializable]
public class ChildProfile
{
    public string childName;
    [TextArea] public string greeting;        // “Wants to confess to Tiffany”
    [TextArea] public string desire;        // “Wants to confess to Tiffany”
    [TextArea] public string insecurity;    // “Not athletic like Brandon”
    [TextArea] public string farewell;    // “Not athletic like Brandon”
    public CandyName matchingCandy;
    
}