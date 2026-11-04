using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    //private CharacterController characterController;
    Rigidbody rb;
    PhotonView view;

    //private float gravity = -9.81f;
    //private Vector3 velocity;
    public float acceleration = 20.0f;
    public float rotationSpeed = 10.0f;

    private Vector3 moveInput;
    //public float speed;

    public int playerActorNumber;

    private void Start()
    {
        //characterController = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        view = GetComponent<PhotonView>();

        rb.useGravity = false;

        rb.constraints = RigidbodyConstraints.FreezeRotationY |
                         RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ;
    }
    private void Update()
    {
        if (!view.IsMine) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        moveInput = new Vector3(h, 0, v).normalized;
    }

    private void FixedUpdate()
    {
        if (!view.IsMine) return;

        if (moveInput.magnitude >= 0.1f)
        {
            rb.AddForce(moveInput * acceleration);

            Quaternion targetRotation = Quaternion.LookRotation(moveInput);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            //float targetAngle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg;
            //transform.rotation = Quaternion.Euler(0, targetAngle, 0);

            //characterController.Move(movement * speed * Time.deltaTime);   
        }

        //transform.Translate(movement);
    }
    public void Initialize(int actorNumber)
    {
        playerActorNumber = actorNumber;

        Renderer myRenderer = GetComponent<Renderer>();
        if (myRenderer != null)
        {
            if (actorNumber == 1)
            {
                myRenderer.material.color = Color.red;
            }
            else
            {
                myRenderer.material.color = Color.blue;
            }
        }

        Debug.Log($"플레이어 초기화 완료. ActorNumber: {actorNumber}");
    }
}
 