using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [Serializable] public class Row
    {
        public GameObject[] col;
    }
    public Row[] grid;

}
