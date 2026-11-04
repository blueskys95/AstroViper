using UnityEngine;

// 데모(GIF 촬영)용: 몬스터 상태를 몸 색상과 머리 위 텍스트로 보여준다
public class MonsterStateIndicator : MonoBehaviour
{
    [SerializeField] private MonsterController monster;
    [SerializeField] private Renderer body;
    [SerializeField] private TextMesh label;
    [SerializeField] private Vector3 labelOffset = new Vector3(0f, 1.4f, 0f);

    [Header("Colors")]
    [SerializeField] private Color patrolColor = new Color(0.3f, 0.75f, 0.35f);
    [SerializeField] private Color alertColor = new Color(1f, 0.8f, 0.2f);
    [SerializeField] private Color attackColor = new Color(0.9f, 0.25f, 0.2f);
    [SerializeField] private Color hitFlashColor = Color.white;
    [SerializeField] private float hitFlashTime = 0.12f;

    private float flashTimer;

    private void OnEnable() => monster.Attacked += OnAttacked;
    private void OnDisable() => monster.Attacked -= OnAttacked;

    private void OnAttacked() => flashTimer = hitFlashTime;

    private void LateUpdate()
    {
        MonsterController.State state = monster.CurrentState;

        Color color = state == MonsterController.State.Attack ? attackColor
                    : state == MonsterController.State.Alert ? alertColor
                    : patrolColor;

        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            color = hitFlashColor;
        }

        body.material.color = color;

        // 라벨은 몬스터가 뒤돌아도 뒤집히지 않도록 자식이 아닌 별도 오브젝트로 따라다닌다
        label.transform.position = monster.transform.position + labelOffset;
        label.text = state.ToString();
        label.color = color;
    }
}
