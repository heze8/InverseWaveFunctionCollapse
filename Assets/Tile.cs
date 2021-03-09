
using UnityEngine;

[CreateAssetMenu]
[System.Serializable]
public class Tile : ScriptableObject
{
    public int index;
    public int[,,] objs;
    public int direction;
    public GameObject tileObj;
    public int freq = 1;
}
