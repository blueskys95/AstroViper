using UnityEngine;

// 데모용: 낭떠러지로 떨어지면 시작 위치로 되돌린다
[RequireComponent(typeof(Rigidbody))]
public class FallRespawn : MonoBehaviour
{
    [SerializeField] private float killY = -8f;

    private Vector3 spawnPosition;
    private Rigidbody rigid;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        spawnPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (transform.position.y > killY) return;

        rigid.velocity = Vector3.zero;
        rigid.position = spawnPosition;
    }
}
