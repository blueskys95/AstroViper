using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpPower = 5f;
    [SerializeField] private float groundCheckDistance = 1.1f; // 캡슐(높이 2) 기준: 절반(1) + 여유
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody rigid;
    private float inputX;
    private bool jumpRequested;
    private float stunTimer;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        rigid.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
    }

    // 입력은 Update에서 받는다 (GetKeyDown은 FixedUpdate에서 놓칠 수 있음)
    private void Update()
    {
        inputX = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
            jumpRequested = true;
    }

    // 물리 이동은 FixedUpdate에서 처리한다
    private void FixedUpdate()
    {
        if (stunTimer > 0f)
        {
            stunTimer -= Time.fixedDeltaTime;
            return;
        }

        Vector3 velocity = rigid.velocity; // Unity 6 이상: rigid.linearVelocity
        velocity.x = inputX * moveSpeed;
        rigid.velocity = velocity;

        if (jumpRequested)
        {
            rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            jumpRequested = false;
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
    }

    // 몬스터에게 맞았을 때 호출: 잠깐 조작을 막고 뒤로 밀려난다
    public void Knockback(Vector3 force, float stunTime = 0.3f)
    {
        rigid.velocity = Vector3.zero;
        rigid.AddForce(force, ForceMode.Impulse);
        stunTimer = stunTime;
        jumpRequested = false;
    }
}
