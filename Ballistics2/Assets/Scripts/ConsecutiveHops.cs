using System;
using System.Collections.Generic;
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
    private float timeInJump = 0;

    [SerializeField]
    private float targetConfirmRange = 1;
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
            UI.instance.SetInfoText("No Jumps Remaining");
            return;
        }

        if (!TestJump(hopTracker.Peek().position, activePosition, activeJumpType))
        {
            Debug.Log("Jump is not possible");
            UI.instance.SetInfoText("Jump is not possible");
            return;
        }

        AddJump(activePosition, activeJumpType);
        targetMarkers.Add(activeTargetMarker);
        activeTargetMarker = Instantiate(targetMarkerVisual, activePosition, Quaternion.identity);
        jumpCount--;
        activePosition = Vector3.one * -1000;
        UI.instance.UpdateJumpCount(jumpCount);
        UI.instance.SetInfoText("");
        Debug.Log(hops.Count);
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
        Nullable<Vector3> aimVector = fs.Calculate(startPosition, targetPosition, launchForce - 3, Physics.gravity);
        return aimVector.HasValue;
    }

    private void AddJump(Vector3 position, bool jumpType)
    {
        FiringSolution fs = new FiringSolution();
        fs.useMaxTime = jumpType;
        
        Nullable<Vector3> aimVector = fs.Calculate(hopTracker.Peek().position, position, launchForce - 3, Physics.gravity);
        if (aimVector.HasValue)
        {
            hops.Enqueue(new JumpTarget(position, jumpType));
            hopTracker.Push(new JumpTarget(position, jumpType));
        }
    }

    private void Start()
    {
        hopTracker.Clear();
        hops.Clear();
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

        if (timeInJump >= 0)
        {
            timeInJump -= Time.deltaTime;
        }
        else if (jumpSequenceActive)
        {
            CheckForNextJump();
        }
    }

    private void CheckForNextJump()
    {
        if (jumpSequenceActive && (transform.position - currentTarget.position).magnitude < targetConfirmRange)
        {
            if (hops.Count > 0)
            {
                DoJump();
            }
            else
            {
                ResetJumps();
            }
        }
    }

    private void DoJump()
    {
        Debug.Log("DoJump Called");
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
        Destroy(activeTargetMarker);
        activeTargetMarker = Instantiate(targetMarkerVisual, activePosition, Quaternion.identity);
        hopTracker.Push(new JumpTarget(playerRb.transform.position, true));
        hops.Enqueue(new JumpTarget(playerRb.transform.position, true));

        return;
    }
}
