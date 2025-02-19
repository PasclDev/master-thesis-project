using System.Collections.Generic;
using System.Numerics;

[System.Serializable]
public class LevelData
{
    public int[] gridSize; // [x, y, z]
    public int[]  voxels; // 3D array (1 = active, 0 = inactive)
}

[System.Serializable]
public class LevelCollection
{
    public List<LevelData> levels;
}
