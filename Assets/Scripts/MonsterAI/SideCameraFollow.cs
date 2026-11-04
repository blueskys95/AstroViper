using UnityEngine;

// 횡스크롤 카메라: 대상의 x(와 살짝 y)를 부드럽게 따라간다
public class SideCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -12f);
    [SerializeField] private float smooth = 5f;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 goal = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, goal, Time.deltaTime * smooth);
    }
}
