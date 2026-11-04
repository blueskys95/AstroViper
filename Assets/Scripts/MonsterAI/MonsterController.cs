using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MonsterController : MonoBehaviour
{
    public enum State { Patrol, Alert, Attack }

    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Move")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float edgeCheckOffset = 0.5f;   // 몸 중심에서 진행 방향으로 얼마나 앞에서 레이를 쏠지
    [SerializeField] private float edgeCheckDistance = 1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Range")]
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float lookoutRange = 3f;

    [Header("Attack")]
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float knockbackPower = 5f;

    private Rigidbody rigid;
    private State state = State.Patrol;
    private int facing = 1;      // 1: 오른쪽, -1: 왼쪽
    private float attackTimer;

    // 상태 표시용(MonsterStateIndicator)에서 읽는다
    public State CurrentState => state;
    public event System.Action Attacked;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        rigid.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
    }

    // 상태 판단과 타이머는 Update에서
    private void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        State next = distance < attackRange ? State.Attack
                   : distance < lookoutRange ? State.Alert
                   : State.Patrol;

        if (next != state)
        {
            Debug.Log($"[Monster] {state} -> {next}"); // 상태가 바뀔 때만 로그
            state = next;
            attackTimer = 0f;
        }

        if (state == State.Attack)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackCooldown)
            {
                Attack();
                attackTimer = 0f;
            }
        }
    }

    // 이동은 FixedUpdate에서
    private void FixedUpdate()
    {
        Vector3 velocity = rigid.velocity; // Unity 6 이상: rigid.linearVelocity

        switch (state)
        {
            case State.Patrol:
                if (!IsGroundAhead())
                    Turn(-facing);
                velocity.x = facing * moveSpeed;
                break;

            case State.Alert:
            case State.Attack:
                FacePlayer();
                velocity.x = 0f;
                break;
        }

        rigid.velocity = velocity;
    }

    // 진행 방향 앞쪽에서 아래로 레이를 쏴서, 바닥이 없으면 낭떠러지
    private bool IsGroundAhead()
    {
        Vector3 origin = transform.position + Vector3.right * facing * edgeCheckOffset;
        Debug.DrawRay(origin, Vector3.down * edgeCheckDistance, Color.green);
        return Physics.Raycast(origin, Vector3.down, edgeCheckDistance, groundLayer);
    }

    private void Turn(int newFacing)
    {
        facing = newFacing;
        transform.rotation = Quaternion.Euler(0f, facing > 0 ? 0f : 180f, 0f);
    }

    private void FacePlayer()
    {
        int dir = player.position.x > transform.position.x ? 1 : -1;
        if (dir != facing)
            Turn(dir);
    }

    private void Attack()
    {
        Debug.Log("[Monster] 공격!");
        Attacked?.Invoke();
        if (player.TryGetComponent(out PlayerMove playerMove))
        {
            Vector3 force = new Vector3(facing, 0.5f, 0f).normalized * knockbackPower;
            playerMove.Knockback(force);
        }
    }

    // Scene 뷰에서 공격/경계 범위를 눈으로 확인
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lookoutRange);
    }
}
