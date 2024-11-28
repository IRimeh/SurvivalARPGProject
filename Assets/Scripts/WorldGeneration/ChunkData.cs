using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ChunkData
{
    public Vector2Int Position;
    public int Height;

    public int NeighborForwardHeight;
    public int NeighborRightHeight;
    public int NeighborBackHeight;
    public int NeighborLeftHeight;

    public List<ObjectSaveData> Objects;
}
