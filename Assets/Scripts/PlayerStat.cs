using UnityEngine;
using Photon.Pun;
using JetBrains.Annotations;

public class PlayerStat : MonoBehaviourPun
{
    public float maxHp = 100.0f;
    public float currentHp;

    private void Start()
    {
        currentHp = maxHp;
    }

    [PunRPC]
    public void TakeDamage(float damageAmount)
    {
        currentHp -= damageAmount;

        //Debug.Log($"{photonView.Owner.NickName}님이 {damageAmount}의 피해를 입었습니다. 남은 체력: {currentHp}");
        if (photonView.IsMine)
        {
            Debug.Log($"<color=red> [피격!] 으악! 내가 {damageAmount} 데미지를 입었습니다. (남은 체력: {currentHp})</color>");
        }
        // Case 2: 남이 맞았을 때 (내가 쏜 걸 맞았든, 남끼리 싸우다 맞았든)
        else
        {
            Debug.Log($"[전투 정보] {photonView.Owner.NickName}님이 공격받음! (남은 체력: {currentHp})");
        }

        if (currentHp <= 0.0f)
        {
            Die();
        }
    }

    private void Die()
    {
        // 이미 격추된 상태라면 무시
        if(!gameObject.activeSelf) return;

        Debug.Log($"{photonView.Owner.NickName} 격추됨!");

        if(photonView.IsMine)
        {
            Debug.Log("Game Over!");
        }

        gameObject.SetActive(false);
    }
}
