using System.Collections.Generic;
using System.Numerics;

[System.Serializable]
public class LevelCollection
{
    public List<LevelData> levels;
}
[System.Serializable]
public class LevelData
{
    public float voxelSize;
    public Fillable fillable;
    public List<Grabbable> grabbables; // 3D array (1 = active, 0 = inactive)
}

[System.Serializable]
public class Fillable {

    // Both size and position are relative to voxelSize
    public int[] size; // [x, y, z]
    public float[] position; // [x, y, z]
}

[System.Serializable]
public class Grabbable
{
    // position is relative to voxelSize
    public float[] position; // [x, y, z]
    public int[] voxels;
    /*
        "voxels": [
            Bottom Front,  Middle Front,  Top Front,
            Bottom Middle,  Middle Middle,  Top Middle,
            Bottom Back,  Middle Back,  Top Back
        ],
    */

}


