using UnityEngine.InputSystem;
using UnityEngine;
using Unity.Netcode;


public class MachineLogik : NetworkBehaviour
{
    Rigidbody2D rb;

    float rotation = 0f;

    float moveForward = 0f;

    bool breakInput = false;

    [SerializeField] float maxSpeed = 100f;

    [SerializeField] float acceleration = 5.0f; //ускарение 

    [SerializeField] float deceleration = 5.0f; //замедление 

    [SerializeField] float rotateSpeed = 1.0f;

    [SerializeField, Range(0,1)] float driftFactor = 0.85f;

    public void Move(InputAction.CallbackContext context)
    {

        if (context.performed)
            moveForward = context.ReadValue<float>();

        if (context.canceled)
            moveForward = 0f;

       
    }

    private void FixedUpdate()
    {

        if (!IsOwner)
            return;

        KillOrthogonalVelocity();

        ApplyRotation();

        ApplyForce();

    }

    public void Rotation(InputAction.CallbackContext context)
    {

        if (context.performed)
            rotation = context.ReadValue<float>();

        if (context.canceled)
            rotation = 0f;

    }


    private void Start()
    {

        rb = GetComponent<Rigidbody2D>();



    }

    void ApplyForce()
    {

        float curentSpeed = Vector2.Dot(rb.linearVelocity, transform.up);

        Vector2 engineForce = Vector2.zero;

        if (moveForward > 0 && curentSpeed > maxSpeed)
            return;

        if (moveForward < 0 && curentSpeed < -maxSpeed * 0.5)
            return;

        if (moveForward != 0)
        {

            engineForce = transform.up * moveForward * acceleration;

            if(moveForward < 0 && curentSpeed > 0)
                engineForce = -transform.up * deceleration;

        }

        if (breakInput)
        {

            if(rb.linearVelocity.magnitude > 0.1f)
            {

                Vector2 breakForce = -rb.linearVelocity.normalized * deceleration;

                rb.AddForce(breakForce , ForceMode2D.Force);

            }


        }else
            rb.AddForce(engineForce, ForceMode2D.Force);


    }
    void ApplyRotation()
    {

        float speedFactor = Mathf.Clamp01(rb.linearVelocity.magnitude / maxSpeed);

        float rotationForce = rotation * rotateSpeed * speedFactor * Time.deltaTime;

        rb.MoveRotation(rb.rotation - rotationForce);

    }

    public void Break(InputAction.CallbackContext context)
    {

        if (context.performed)
            breakInput = true;

        if (context.canceled)
            breakInput = false;

    }
    void KillOrthogonalVelocity()
    {
        Vector2 forwardVelocity = transform.up * Vector2.Dot(rb.linearVelocity, transform.up);

        Vector2 rightVelocity = transform.right * Vector2.Dot(rb.linearVelocity, transform.right);

        rb.linearVelocity = forwardVelocity + rightVelocity * driftFactor;
    }
}
