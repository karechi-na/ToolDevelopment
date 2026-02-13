using UnityEngine;
using UnityEngine.InputSystem;

public class BoxController : MonoBehaviour
{
    private Rigidbody rb;

   
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnJump(InputValue val)
    {
        if (val.isPressed)
        {
            rb.AddForce(Vector3.up * 5.0f, ForceMode.Impulse);
        }
    }

    private void OnDash(InputValue val)
    {
        if (val.isPressed)
        {
            Debug.Log("Dash!!");
        }
    }
}
