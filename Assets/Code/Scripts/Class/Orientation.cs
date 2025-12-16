using UnityEngine;

public class Orientation 
{
    public Vector3 forward;
    public Vector3 up;
    public Vector3 right;
    public Orientation(Vector3 forward, Vector3 up, Vector3 right)
    {
        this.forward = forward.normalized;
        this.up = up.normalized;
        this.right = right.normalized;
    }
}
