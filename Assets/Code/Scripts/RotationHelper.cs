using System;
using UnityEngine;


public class RotationHelper
{


    // OLD, unused
    // Used to rotate the voxel matrix of a grabbable object
    //Rotate a xyz matrix by xRotation, yRotation, zRotation, where each rotation is either 0, 1, 2 or 3, standing for 0, 90, 180, or 270 degrees
    //Return the rotated matrix
    public static int[][][] RotateMatrix (int[][][] matrix, int xRotation, int yRotation, int zRotation){
        Debug.Log ("Rotating matrix by " + xRotation + " " + yRotation + " " + zRotation);
        (int newXSize, int newYSize, int newZSize) = RotateDimensionSize(matrix.Length, matrix[0].Length, matrix[0][0].Length, xRotation, yRotation, zRotation);
        int[][][] rotatedMatrix = new int[newXSize][][];
        // Create new rotated matrix-array
        for (int x = 0; x < newXSize; x++)
        {
            rotatedMatrix[x] = new int[newYSize][];
            for (int y = 0; y < newYSize; y++)
            {
                rotatedMatrix[x][y] = new int[newZSize];
            }
        }
        Debug.Log ("Previous Matrix Size: " + matrix.Length + " " + matrix[0].Length + " " + matrix[0][0].Length + " and new Matrix Size: " + newXSize + " " + newYSize + " " + newZSize);
        //fill new matrix-array
        for (int x = 0; x < matrix.Length; x++)
        {
            for (int y = 0; y < matrix[x].Length; y++)
            {
                for (int z = 0; z < matrix[x][y].Length; z++)
                {
                    (int newX, int newY, int newZ) = RotateIndices(x, y, z, xRotation, yRotation, zRotation, newXSize, newYSize, newZSize);
                    try{
                       rotatedMatrix[newX][newY][newZ] = matrix[x][y][z];
                    }
                    catch (IndexOutOfRangeException e){
                        //TODO: fix bug here
                        Debug.Log("matrix size: " + matrix.Length + " " + matrix[x].Length + " " + matrix[x][y].Length +" and rotatedMatrixSize: " + newXSize + " " + newYSize + " " + newZSize + " and old coords:"+x+" "+y+" "+z+" and new Coords: " + newX + " " + newY + " " + newZ);
                        Debug.Log("Index out of range: " + e);
                        
                    }
                }
            }
        }
        return rotatedMatrix; 
    }
    
    //xRotation, yRotation, zRotation are either 0, 1, 2 or 3 and determine how often the matrix is rotated by 90 degrees
    //For example, if xRotation = 1, the matrix is rotated by 90 degrees around the x-axis
    // Rotate the size of a matrix int[][][], so if the matrix is (1,3,1) and is rotated once on the x-axis, the new size is (1,1,3)
    public static (int,int,int) RotateDimensionSize(int x, int y, int z, int xRotation, int yRotation, int zRotation){
        int newX = x;
        int newY = y;
        int newZ = z;
        if (xRotation % 2 == 1)
        {
            (newY, newZ) = (newZ, newY);
        }
        if (yRotation % 2 == 1)
        {
            (newX, newZ) = (newZ, newX);
        }
        if (zRotation % 2 == 1)
        {
            (newX, newY) = (newY, newX);
        }
        return (newX, newY, newZ);
    }

    private static (int,int,int) RotateIndices(int x, int y, int z, int xRotation, int yRotation, int zRotation, int xLength, int yLength, int zLength)
    {
        int newX = x;
        int newY = y;
        int newZ = z;
        
        //Rotation around the x-axis, if i had an empty 3x3x3 grid with a 1 at (2,0,0), rotating it once would result with the 1 being at (2,2,0), once again at (2,2,2), and finally at (2,0,2)
        for (int ix = 0; ix < xRotation; ix++)
        {
            (newY, newZ) = (zLength - 1 - newZ, newY); //(x,y,z) = (x,!z,y)
            (xLength, yLength, zLength) = RotateDimensionSize(xLength, yLength, zLength, 1, 0, 0);
            
        }
        //Rotation around the y-axis, if i had an empty 3x3x3 grid with a 1 at (2,0,0), rotating it once would result with the 1 being at (0,0,0), once again at (0,0,2), and finally at (2,0,2)
        for (int iy = 0; iy < yRotation; iy++)
        {
            (newX, newZ) = (newZ, xLength - 1 - newX); //(x,y,z) = (z,y,!x)
            (xLength, yLength, zLength) = RotateDimensionSize(xLength, yLength, zLength, 0, 1, 0);
        }
        //Rotation around the z-axis, if i had an empty 3x3x3 grid with a 1 at (2,0,0), rotating it once would result with the 1 being at (2,2,0), once again at (0,2,0), and finally at (0,0,0)
        for (int iz = 0; iz < zRotation; iz++)
        {
            (newX, newY) = (yLength - 1 - newY, newX); //(x,y,z) = (!y,x,z)
            (xLength, yLength, zLength) = RotateDimensionSize(xLength, yLength, zLength, 0, 0, 1);
        }
        //if(x == 0 && y == 0 && z == 0){
            Debug.Log("Rotating indices: " + x + " " + y + " " + z + " by " + xRotation + " " + yRotation + " " + zRotation + " to " + newX + " " + newY + " " + newZ);
        //}
        return (newX, newY, newZ);
    }

    // Rotate a matrix of voxels by the orientation of the object
    public static int[][][] RotateMatrix(int[][][] matrix, Vector3 up, Vector3 right, Vector3 forward)
    {
        int sizeX = matrix.Length;
        int sizeY = matrix[0].Length;
        int sizeZ = matrix[0][0].Length;

        // Determine new dimensions based on axis swaps
        int newSizeX = Mathf.Abs(Vector3.Dot(Vector3.right, up)) > 0 ? sizeY : (Mathf.Abs(Vector3.Dot(Vector3.forward, up)) > 0 ? sizeZ : sizeX);
        int newSizeY = Mathf.Abs(Vector3.Dot(Vector3.up, up)) > 0 ? sizeY : (Mathf.Abs(Vector3.Dot(Vector3.forward, up)) > 0 ? sizeX : sizeZ);
        int newSizeZ = Mathf.Abs(Vector3.Dot(Vector3.forward, up)) > 0 ? sizeY : (Mathf.Abs(Vector3.Dot(Vector3.up, up)) > 0 ? sizeZ : sizeX);

        Debug.Log("Rotate Matrix, previous size: " + sizeX + " " + sizeY + " " + sizeZ + " and new size: " + newSizeX + " " + newSizeY + " " + newSizeZ+ " with up: " + up + " right: " + right + " forward: " + forward);
        
        // Create the new rotated matrix with the correct dimensions
        int[][][] rotatedMatrix = new int[newSizeX][][];
        for (int x = 0; x < newSizeX; x++)
        {
            rotatedMatrix[x] = new int[newSizeY][];
            for (int y = 0; y < newSizeY; y++)
            {
                rotatedMatrix[x][y] = new int[newSizeZ];
            }
        }

        // Rotate each voxel correctly
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    Vector3 oldPos = new Vector3(x, y, z);
                    Vector3 newPos = RotateVoxelPosition(oldPos, up, right, forward, sizeX, sizeY, sizeZ);

                    rotatedMatrix[(int)newPos.x][(int)newPos.y][(int)newPos.z] = matrix[x][y][z];
                }
            }
        }

        return rotatedMatrix;
    }

    private static Vector3 RotateVoxelPosition(Vector3 pos, Vector3 up, Vector3 right, Vector3 forward, int xLength, int yLength, int zLength)
    {
        int newX = 0, newY = 0, newZ = 0;

        // Determine the new position based on orientation
        //Default forward is z. up is y, right is x
        //do not rotate before, changes usage of _length, without its _length of the pos._, otherwise its the _length of the new variable (xLength, yLength, zLength) = RotateDimensionSize(xLength, yLength, zLength, up, right, forward);
        if (up == Vector3.up) //only rotation around y-axis
        {
            if(forward == Vector3.forward){ // Rotation in degrees: (0, 0, 0)
                (newX, newY, newZ) = ((int)pos.x, (int)pos.y, (int)pos.z); //checked
            }
            else if(forward == Vector3.back){ // Rotation in degrees: (0, 180, 0)
                (newX, newY, newZ) = (xLength - 1 - (int)pos.x, (int)pos.y, zLength - 1 - (int)pos.z); // (!x,y,!z)
                //(newX, newY, newZ) = RotateIndices((int)pos.x, (int)pos.y, (int)pos.z, 0, 2, 0, xLength, yLength, zLength);
            }
            else if(forward == Vector3.right){ // Rotation in degrees: (0, 90, 0)
                (newX, newY, newZ) = ((int)pos.z, (int)pos.y, xLength - 1 - (int)pos.x); // (z,y,!x) 
                //(newX, newY, newZ) = RotateIndices((int)pos.x, (int)pos.y, (int)pos.z, 0, 1, 0, xLength, yLength, zLength);

            }
            else if(forward == Vector3.left){ // Rotation in degrees: (0, 270, 0)
                (newX, newY, newZ) = (zLength - 1 - (int)pos.z, (int)pos.y, (int)pos.x); // (!z,y,x)
                //(newX, newY, newZ) = RotateIndices((int)pos.x, (int)pos.y, (int)pos.z, 0, 3, 0, xLength, yLength, zLength);
            }
        }
        else if (up == Vector3.down) // Base-Rotation of 180 degree on the x-axis or 180 degree on the z-axis
        {
            if(forward == Vector3.forward){ // Rotation in degrees: (180, 180, 0) or (0, 0, 180)
                (newX, newY, newZ) = (xLength - 1 - (int)pos.x, yLength - 1 - (int)pos.y, (int)pos.z); // checked
                //(newX, newY, newZ) = RotateIndices((int)pos.x, (int)pos.y, (int)pos.z, 0, 0, 2, xLength, yLength, zLength);
            }
            else if(forward == Vector3.back){ // Rotation in degrees: (180, 0, 0) or (0, 180, 180)
                (newX, newY, newZ) = ((int)pos.x, yLength - 1 - (int)pos.y, zLength - 1 - (int)pos.z);  // checked
                //(newX, newY, newZ) = RotateIndices((int)pos.x, (int)pos.y, (int)pos.z, 2, 0, 0, xLength, yLength, zLength);
            }
            else if(forward == Vector3.right){ // Rotation in degrees: (180, 270, 0) or (0, 90, 180)
                (newX, newY, newZ) = ((int)pos.z, yLength - 1 - (int)pos.y, xLength - 1 - (int)pos.x); // checked
                //(newX, newY, newZ) = RotateIndices((int)pos.x, (int)pos.y, (int)pos.z, 0, 1, 2, xLength, yLength, zLength);
            }
            else if(forward == Vector3.left){ // Rotation in degrees: (180, 90, 0) or (0, 270, 180)
                (newX, newY, newZ) = (zLength - 1 - (int)pos.z, yLength - 1 - (int)pos.y, (int)pos.x); // checked
                //(newX, newY, newZ) = RotateIndices((int)pos.x, (int)pos.y, (int)pos.z, 2, 1, 0, xLength, yLength, zLength);
            }
        }
        else if (up == Vector3.right) //Base rotation of 270 of degrees on the z axis
        {
            if(forward == Vector3.forward){ // Rotation in degrees: (0, 0, 270)
                // if original lengths are (4x3x3) and the up is right and forward forward, the new lengths are (3x4x3))
                //in a 4x3x3 grid, the voxel at (3,2,0) would be at (2,0,0) after the rotation
                (newX, newY, newZ) = ((int)pos.y, xLength - 1 - (int)pos.x, (int)pos.z); // checked
                //(newX, newY, newZ) = RotateIndices((int)pos.x, (int)pos.y, (int)pos.z, 0, 0, 3, xLength, yLength, zLength);
            }
            else if(forward == Vector3.back){ // Rotation in degrees: (180, 0, 270) or (0, 180, 90)
                (newX, newY, newZ) = ((int)pos.y, (int)pos.x, zLength - 1 - (int)pos.z); // checked 
                //(newX, newY, newZ) = (yLength - 1 - (int)pos.y, xLength - 1 - (int)pos.x, zLength - 1 - (int)pos.z);
                //(newX, newY, newZ) = RotateIndices((int)pos.x, (int)pos.y, (int)pos.z, 2, 0, 3, xLength, yLength, zLength);
            }
            else if(forward == Vector3.up){ // Rotation in degrees: (270, 0, 270) [270, 270, 0 may look the same, but doesn't work!]
                (newX, newY, newZ) = ((int)pos.y, (int)pos.z, (int)pos.x); // (y,z,x)
                //(newX, newY, newZ) = RotateIndices((int)pos.x, (int)pos.y, (int)pos.z, 3, 0, 3, xLength, yLength, zLength);
            }
            else if(forward == Vector3.down){ // Rotation in degrees: (90, 90, 0) [90, 0, 270 may look the same, but doesn't work!]
                (newX, newY, newZ) = ((int)pos.y, zLength - 1 - (int)pos.z, xLength - 1 - (int)pos.x);
                //(newX, newY, newZ) = RotateIndices((int)pos.x, (int)pos.y, (int)pos.z, 1, 0, 3, xLength, yLength, zLength);
            }
        }
        else if (up == Vector3.left)
        {
            if (forward == Vector3.forward) // Rotation in degrees: (0, 0, 90)
            {
                //(newX, newY, newZ) = (yLength - 1 - (int)pos.y, (int)pos.x, (int)pos.z);
                (newX, newY, newZ) = RotateIndices((int)pos.x, (int)pos.y, (int)pos.z, 0, 0, 1, xLength, yLength, zLength);
            }
            else if (forward == Vector3.back) // Rotation in degrees: (180, 0, 90)
            {
                (newX, newY, newZ) = (yLength - 1 - (int)pos.y, xLength - 1 - (int)pos.x, zLength - 1 - (int)pos.z);
                //(newX, newY, newZ) = RotateIndices((int)pos.x, (int)pos.y, (int)pos.z, 2, 0, 1, xLength, yLength, zLength);
                
            }
            else if (forward == Vector3.up) // Rotation in degrees: (270, 0, 90) or (90, 270, 0)
            {
                (newX, newY, newZ) = RotateIndices((int)pos.x, (int)pos.y, (int)pos.z, 3, 1, 0, xLength, yLength, zLength);
            }
            else if (forward == Vector3.down) // Rotation in degrees: (90, 0, 90)
            {
                (newX, newY, newZ) = RotateIndices((int)pos.x, (int)pos.y, (int)pos.z, 1, 1, 0, xLength, yLength, zLength);
            }
            
        }
        else if (up == Vector3.forward)
        {
            if (forward == Vector3.down){ // Rotation in degrees: (90, 0, 0) [creates gimbal lock with y and z]
                (newX, newY, newZ) = RotateIndices((int)pos.x, (int)pos.y, (int)pos.z, 1, 0, 0, xLength, yLength, zLength);
            }
            else if(forward == Vector3.up){ // Rotation in degrees: (270, 180, 0)
                (newX, newY, newZ) = RotateIndices((int)pos.x, (int)pos.y, (int)pos.z, 3, 2, 0, xLength, yLength, zLength);
            }
            else if(forward == Vector3.left){ // Rotation in degrees: (0, 270, 270)
                (newX, newY, newZ) = (zLength - 1 - (int)pos.z, xLength - 1 - (int)pos.x, (int)pos.y); //checked
                //(newX, newY, newZ) = RotateIndices((int)pos.x, (int)pos.y, (int)pos.z, 0, 3, 3, xLength, yLength, zLength);
            }
            else if(forward == Vector3.right){ // Rotation in degrees: (180, 270, 270) (0, 90, 90) 
                (newX, newY, newZ) = RotateIndices((int)pos.x, (int)pos.y, (int)pos.z, 1, 0, 1, xLength, yLength, zLength); // the input differs from the rotation in degrees, because the rotation axis of z changes when rotating around the y-axis
            }
        }
        else if (up == Vector3.back)
        {
            if (forward == Vector3.down){ // Rotation in degrees: (90, 180, 0)
                (newX, newY, newZ) = RotateIndices((int)pos.x, (int)pos.y, (int)pos.z, 1, 2, 0, xLength, yLength, zLength);
            }
            else if(forward == Vector3.up){ // Rotation in degrees: (270, 0, 0) [creates gimbal lock with y and z]
                (newX, newY, newZ) = RotateIndices((int)pos.x, (int)pos.y, (int)pos.z, 3, 0, 0, xLength, yLength, zLength);
            }
            else if(forward == Vector3.left){ // Rotation in degrees: (0, 270, 90)
                (newX, newY, newZ) = RotateIndices((int)pos.x, (int)pos.y, (int)pos.z, 3, 0, 1, xLength, yLength, zLength); //input differs, we want to rotate around x 270, then around z 90, the normal transform.rotation doesnt handle rotation seperately
            }
            else if(forward == Vector3.right){ // Rotation in degrees: (90, 90, 0)
                (newX, newY, newZ) = ((int)pos.z, xLength - 1 - (int)pos.x, yLength - 1 - (int)pos.y);
                //(newX, newY, newZ) = RotateIndices((int)pos.x, (int)pos.y, (int)pos.z, 1, 1, 0, xLength, yLength, zLength);
            }
        }    
        return new Vector3(newX, newY, newZ);
        
    }
    public static (int,int,int) RotateDimensionSize(int x, int y, int z, Vector3 up, Vector3 right, Vector3 forward)
    {
        int newX = 0, newY = 0, newZ = 0;

        // Determine the new dimensions based on orientation
        if (up == Vector3.up || up == Vector3.down)
        {
            newX = x;
            newY = y;
            newZ = z;
        }
        else if (up == Vector3.right || up == Vector3.left)
        {
            newX = y;
            newY = x;
            newZ = z;
        }
        else if (up == Vector3.forward || up == Vector3.back)
        {
            newX = x;
            newY = z;
            newZ = y;
        }

        // Handle forward axis flips
        if (forward == Vector3.left){
            newX = x;
            newY = y;
            newZ = z;
        }
        else if (forward == Vector3.up){
            newX = x;
            newY = y;
            newZ = z;
        }
        else if (forward == Vector3.back){
            newX = x;
            newY = y;
            newZ = z;
        }

        return (newX, newY, newZ);
    }
    public static (bool, Vector3, Vector3, Vector3) IsValidRotation(Transform grabbable, float rotationTolerancePercentage)
    {
        // Get the object's transform axes directly
        Vector3 right = grabbable.transform.right;     // Local X
        Vector3 up = grabbable.transform.up;           // Local Y
        Vector3 forward = grabbable.transform.forward; // Local Z

        // Define possible world-aligned axes
        Vector3[] validAxes = { 
            Vector3.right, Vector3.left, 
            Vector3.up, Vector3.down, 
            Vector3.forward, Vector3.back 
        };

        // Find the closest world-aligned vectors for each local axis
        Vector3 closestRight = FindClosestAxis(right, validAxes);
        Vector3 closestUp = FindClosestAxis(up, validAxes);
        Vector3 closestForward = FindClosestAxis(forward, validAxes);

        // Check if the object is aligned with world axes
        bool isValid = 
            IsAxisAligned(right, rotationTolerancePercentage) &&
            IsAxisAligned(up, rotationTolerancePercentage) &&
            IsAxisAligned(forward, rotationTolerancePercentage) &&
            closestRight != closestUp && closestUp != closestForward && closestRight != closestForward; // Ensure perpendicularity

        // Return validity and the full rotation axes
        
        return (isValid, closestUp, closestRight, closestForward);
    }

    // Checks if a vector is close to a valid axis (±1,0,0), (0,±1,0), (0,0,±1)
    static bool IsAxisAligned(Vector3 v, float rotationTolerancePercentage)
    {
        float tolerance = rotationTolerancePercentage;
        
        return (Mathf.Abs(Mathf.Abs(v.x) - 1) <= tolerance && Mathf.Abs(v.y) <= tolerance && Mathf.Abs(v.z) <= tolerance) ||
            (Mathf.Abs(Mathf.Abs(v.y) - 1) <= tolerance && Mathf.Abs(v.x) <= tolerance && Mathf.Abs(v.z) <= tolerance) ||
            (Mathf.Abs(Mathf.Abs(v.z) - 1) <= tolerance && Mathf.Abs(v.x) <= tolerance && Mathf.Abs(v.y) <= tolerance);
    }

    // Finds the closest world-aligned axis to a given vector
    static Vector3 FindClosestAxis(Vector3 axis, Vector3[] validAxes)
    {
        Vector3 closest = validAxes[0];
        float maxDot = -1f;

        foreach (var valid in validAxes)
        {
            float dot = Vector3.Dot(axis, valid);
            if (dot > maxDot) // Find best match
            {
                maxDot = dot;
                closest = valid;
            }
        }

        return closest;
    }
}