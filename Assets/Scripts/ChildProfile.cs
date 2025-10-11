using UnityEngine;

[CreateAssetMenu(menuName = "Child/ChildProfile")]
public class ChildProfile : ScriptableObject
{
    public string childName;
    [TextArea] public string desire;        // “Wants to confess to Tiffany”
    [TextArea] public string insecurity;    // “Not athletic like Brandon”
    public string[] tags;                   // e.g., "love","athletic","envy"
}