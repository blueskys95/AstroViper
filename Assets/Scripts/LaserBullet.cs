using UnityEngine;
using Photon.Pun;

public class LaserBullet : MonoBehaviourPun
{
    public float speed = 100.0f;
    public float lifeTime = 2.0f;
    public float damage = 10.0f;

    private void Start()
    {
        if(photonView.IsMine)
        {
            Invoke("DestroyBullet", lifeTime);
        }
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;

        if(other.CompareTag("Player"))
        {
            Debug.Log("플레이어가 총알에 맞았습니다.");
            PhotonView targetPv = other.GetComponent<PhotonView>();

            if(targetPv != null && !targetPv.IsMine)
            {
                targetPv.RPC("TakeDamage", RpcTarget.All, damage);

                PhotonNetwork.Destroy(gameObject);
            }
        }
    }

    private void DestroyBullet()
    {
        PhotonNetwork.Destroy(gameObject);
    }
}
