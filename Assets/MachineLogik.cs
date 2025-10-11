using UnityEngine.InputSystem;
using UnityEngine;


public class MachineLogik : MonoBehaviour
{
    Rigidbody2D rb;

    float rotation = 0f;

    float moveForward = 0f;

    float currentSpeed = 0f;

    [SerializeField] float maxSpeed = 100f;

    [SerializeField] float acceleration = 5.0f; //ускарение 

    [SerializeField] float deceleration = 5.0f; //замедление 

    [SerializeField] float rotateSpeed = 1.0f;

    public void Move(InputAction.CallbackContext context)
    {

        if (context.performed)
            moveForward = context.ReadValue<float>();

        if (context.canceled)
            moveForward = 0f;

        Debug.Log(moveForward);
    }

    private void FixedUpdate()
    {


        if (rb.linearVelocity != Vector2.zero)
        {


            if (rotation != 0)
            {

                float newRotation = rb.rotation - rotation * rotateSpeed;

                rb.MoveRotation(newRotation);

            }
        }




        if (moveForward != 0)
        {

            currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed * moveForward, acceleration * Time.fixedDeltaTime);

        }

        else
        {

            if (deceleration == 0)
                currentSpeed = 0f;

            else
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.fixedDeltaTime);

        }
        rb.linearVelocity = transform.up * currentSpeed;


        Debug.Log(moveForward);

    }

    public void Rotation(InputAction.CallbackContext context)
    {

        if (context.performed)
            rotation = context.ReadValue<float>();

        if (context.canceled)
            rotation = 0f;

        Debug.Log(rotation);
    }


    private void Start()
    {

        rb = GetComponent<Rigidbody2D>();



    }
}
