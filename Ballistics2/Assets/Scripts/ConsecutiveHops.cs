using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



public class ConsecutiveHops : MonoBehaviour
{
    private Queue<JumpTarget> hops = new Queue<JumpTarget>();
    private Stack<JumpTarget> hopTracker = new Stack<JumpTarget>();
    private JumpTarget currentTarget;

    private List<GameObject> targetMarkers = new List<GameObject>();
    private GameObject activeTargetMarker;
    private Vector3 activePosition = new Vector3(-100, -100, -100);
    private bool activeJumpType = true;
    private bool jumpSequenceActive = false;
    private int jumpCount = 5;

    [SerializeField]
    private float gravityModifier = 5;
    [SerializeField]
    private float launchForce = 10;
    [SerializeField]
    private int maxJumps = 5;
    [SerializeField]
    private Rigidbody playerRb;
    [SerializeField]
    private GameObject targetMarkerVisual;

    public void setTempJump(bool type)
    {
        activeJumpType = type;
    }

    public void RemoveJump()
    {
        if (hops.Count <= 1) return;
        hopTracker.Pop();
        hops.Dequeue();
        Destroy(targetMarkers[targetMarkers.Count - 1]);
        targetMarkers.Remove(targetMarkers[targetMarkers.Count - 1]);
        Mathf.Min(maxJumps, jumpCount++);
        UI.instance.UpdateJumpCount(jumpCount);
    }

    public void ConfirmJumpTarget()
    {
        if (jumpCount <= 0)
        {
            Debug.Log("No Jumps Remaining");
            return;
        }

        if (!TestJump(hopTracker.Peek().position, activePosition, activeJumpType))
        {
            Debug.Log("Jump is not possible");
            return;
        }

        AddJump(activePosition, activeJumpType);
        targetMarkers.Add(activeTargetMarker);
        activeTargetMarker = Instantiate(targetMarkerVisual, activePosition, Quaternion.identity);
        jumpCount--;
        activePosition = Vector3.one * -1000;
        UI.instance.UpdateJumpCount(jumpCount);
    }

    public void StartJumpSequence()
    {
        if (hops.Count <= 1) return;

        hops.Dequeue();
        DoJump();
        jumpSequenceActive = true;
    }

    private bool TestJump(Vector3 startPosition, Vector3 targetPosition, bool jumpType)
    {
        FiringSolution fs = new FiringSolution();
        fs.useMaxTime = jumpType;
        Nullable<Vector3> aimVector = fs.Calculate(startPosition, targetPosition, launchForce, Physics.gravity);
        return aimVector.HasValue;
    }

    private void AddJump(Vector3 position, bool jumpType)
    {
        FiringSolution fs = new FiringSolution();
        fs.useMaxTime = jumpType;
        
        Nullable<Vector3> aimVector = fs.Calculate(hops.Peek().position, position, launchForce, Physics.gravity);
        if (aimVector.HasValue)
        {
            hops.Enqueue(new JumpTarget(position, jumpType));
            hopTracker.Push(new JumpTarget(position, jumpType));
        }
    }

    private void Start()
    {
        hops.Enqueue(new JumpTarget(playerRb.transform.position, true));
        hopTracker.Push(new JumpTarget(playerRb.transform.position, true));
        jumpCount = maxJumps;
        UI.instance.UpdateJumpCount(jumpCount);
        activeTargetMarker = Instantiate(targetMarkerVisual, activePosition, Quaternion.identity);
        targetMarkers.Add(activeTargetMarker);
        Physics.gravity = Physics.gravity * gravityModifier;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            bool hasHit = Physics.Raycast(ray, out hit);

            Debug.Log(hasHit);

            if (hasHit && hit.collider.gameObject.CompareTag("Ground") && jumpCount >= 1)
            {
                activePosition = hit.point;
                activeTargetMarker.transform.position = activePosition;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collision Occurred");
        if (currentTarget == null)
            return;

        if (other.transform.tag == "Target" && jumpSequenceActive)
        {
            if (hops.Count > 0)
            {
                DoJump();
            }
            else
            {
                Debug.Log("Time to reset");
                ResetJumps();
            }
        }
    }

    private void DoJump()
    {

        currentTarget = hops.Peek();

        FiringSolution fs = new FiringSolution();
        fs.useMaxTime = currentTarget.jumpType;
        Nullable<Vector3> aimVector = fs.Calculate(playerRb.transform.position, currentTarget.position, launchForce, Physics.gravity);
        if (aimVector.HasValue)
        {
            Debug.Log("Gotta Blast");
            playerRb.linearVelocity = Vector3.zero;
            playerRb.AddForce(aimVector.Value.normalized * launchForce, ForceMode.VelocityChange);

            hops.Dequeue();
        }
    }

    public void ResetJumps()
    {
        jumpCount = maxJumps;
        UI.instance.UpdateJumpCount(jumpCount);
        jumpSequenceActive = false;
        hops.Clear();
        hopTracker.Clear();
        foreach (GameObject marker in targetMarkers)
        {
            Destroy(marker);    
        }
        targetMarkers.Clear();
        activeTargetMarker = Instantiate(targetMarkerVisual, activePosition, Quaternion.identity);
        hopTracker.Push(new JumpTarget(playerRb.transform.position, true));
        hops.Enqueue(new JumpTarget(playerRb.transform.position, true));

        return;
    }
}
