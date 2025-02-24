using System;
using UnityEngine;


public class RotationHelper
{


    //Rotate a xyz matrix by xRotation, yRotation, zRotation, where each rotation is either 0, 1, 2 or 3, standing for 0, 90, 180, or 270 degrees
    //Return the rotated matrix
    public static int[][][] RotateMatrix (int[][][] matrix, int xRotation, int yRotation, int zRotation){
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
        //fill new matrix-array
        for (int x = 0; x < matrix.Length; x++)
        {
            for (int y = 0; y < matrix[x].Length; y++)
            {
                for (int z = 0; z < matrix[x][y].Length; z++)
                {
                    (int newX, int newY, int newZ) = RotateIndices(x, y, z, xRotation, yRotation, zRotation, newXSize, newYSize, newZSize);
                    Debug.Log("matrix size: " + matrix.Length + " " + matrix[x].Length + " " + matrix[x][y].Length +" and rotatedMatrixSize: " + newXSize + " " + newYSize + " " + newZSize + " and old coords:"+x+" "+y+" "+z+" and new Coords: " + newX + " " + newY + " " + newZ);
                    rotatedMatrix[newX][newY][newZ] = matrix[x][y][z];
                }
            }
        }
        return rotatedMatrix; 
    }
    //xRotation, yRotation, zRotation are either 0, 1, 2 or 3 and determine how often the matrix is rotated by 90 degrees
    //For example, if xRotation = 1, the matrix is rotated by 90 degrees around the x-axis
    // Rotate the size of a matrix int[][][], so if the matrix is (1,3,1) and is rotated once on the x-axis, the new size is (1,1,3)
    private static (int,int,int) RotateDimensionSize(int x, int y, int z, int xRotation, int yRotation, int zRotation){
        int newX = x;
        int newY = y;
        int newZ = z;
        if (xRotation % 2 == 1)
        {
            (newX, newY, newZ) = (z, y, x);
        }
        if (yRotation % 2 == 1)
        {
            (newX, newY, newZ) = (x, z, y);
        }
        if (zRotation % 2 == 1)
        {
            (newX, newY, newZ) = (y, x, z);
        }
        return (newX, newY, newZ);
    }

    private static (int,int,int) RotateIndices(int x, int y, int z, int xRotation, int yRotation, int zRotation, int xLength, int yLength, int zLength)
    {
        int newX = x;
        int newY = y;
        int newZ = z;
        if (xRotation == 1)
        {
            (newX, newY, newZ) = (x, zLength - 1 - z, y);
            (xLength, yLength, zLength) = RotateDimensionSize(xLength, yLength, zLength, 1, 0, 0);
        }
        else if (xRotation == 2)
        {
            (newX, newY, newZ) = (x, yLength - 1 - y, zLength - 1 - z);
            (xLength, yLength, zLength) = RotateDimensionSize(xLength, yLength, zLength, 2, 0, 0);
        }
        else if (xRotation == 3)
        {
            (newX, newY, newZ) = (x, z, xLength - 1 - y);
            (xLength, yLength, zLength) = RotateDimensionSize(xLength, yLength, zLength, 3, 0, 0);
        }
        if (yRotation == 1)
        {
            (newX, newY, newZ) = (zLength - 1 - z, y, x);
            (xLength, yLength, zLength) = RotateDimensionSize(xLength, yLength, zLength, 0, 1, 0);
        }
        else if (yRotation == 2)
        {
            (newX, newY, newZ) = (xLength - 1 - x, y, zLength - 1 - z);
            (xLength, yLength, zLength) = RotateDimensionSize(xLength, yLength, zLength, 0, 2, 0);
        }
        else if (yRotation == 3)
        {
            (newX, newY, newZ) = (z, y, xLength - 1 - x);
            (xLength, yLength, zLength) = RotateDimensionSize(xLength, yLength, zLength, 0, 3, 0);
        }
        if (zRotation == 1)
        {
            (newX, newY, newZ) = (y, x, zLength - 1 - z);
            RotateDimensionSize(xLength, yLength, zLength, 0, 0, 1);
        }
        else if (zRotation == 2)
        {
            (newX, newY, newZ) = (xLength - 1 - x, yLength - 1 - y, z);
            RotateDimensionSize(xLength, yLength, zLength, 0, 0, 2);
        }
        else if (zRotation == 3)
        {
            (newX, newY, newZ) = (yLength - 1 - y, x, z);
            RotateDimensionSize(xLength, yLength, zLength, 0, 0, 3);
        }
        return (newX, newY, newZ);
    }
}
