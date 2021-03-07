
using UnityEngine;

[CreateAssetMenu]
[System.Serializable]
public class Tile : ScriptableObject
{
    public int index;
    public int[,,] objs;
    public GameObject tileObj;
    public int freq = 1;
}
