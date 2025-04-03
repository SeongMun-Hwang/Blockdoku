using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BlockArray", menuName = "Scriptable Objects/BlockArray")]
public class BlockArray : ScriptableObject
{
    public List<string> shapeRows;
}
