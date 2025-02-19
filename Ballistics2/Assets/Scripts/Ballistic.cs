using System;
using UnityEngine;

public class Ballistic : MonoBehaviour
{
    public float myTimeScale = 1.0f;
    public GameObject target;
    public float launchForce = 10f;
    public bool useMaxTime;
    Rigidbody rb;
    Vector3 startPos;

    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
        Time.timeScale = myTimeScale; // allow for slowing time to see what's happening
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            FiringSolution fs = new FiringSolution();
            fs.useMaxTime = useMaxTime;
            Nullable<Vector3> aimVector = fs.Calculate(transform.position, target.transform.position, launchForce, Physics.gravity);
            if (aimVector.HasValue)
            {
                rb.linearVelocity = Vector3.zero;
                rb.AddForce(aimVector.Value.normalized * launchForce, ForceMode.VelocityChange);
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            // reset
            rb.isKinematic = true;
            transform.position = startPos;
            rb.isKinematic = false;
        }
    }
}