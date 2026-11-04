using UnityEngine;
using Photon.Pun;

public class PlayerShoot : MonoBehaviourPun
{
    public string bulletPrefabName = "LaserBullet";
    public Transform firePoint;
    public float fireRate = 0.5f;

    private float nextFireTime = 0.0f;

    private void Update()
    {
        if (!photonView.IsMine) return;

        if(Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireRate;
        }
    }
    private void Fire()
    {
        GameObject bullet = PhotonNetwork.Instantiate(bulletPrefabName, firePoint.position, firePoint.rotation);
    
        //bullet.GetComponent<LaserBullet>().shooter = this.gameObject;
    }
}
