using Unity.VisualScripting;
using UnityEngine;

public class JumpTarget
{
    public JumpTarget(Vector3 position, bool jumpType)
    {
        this.position = position;
        this.jumpType = jumpType;
    }

    public Vector3 position;
    public bool jumpType;
}