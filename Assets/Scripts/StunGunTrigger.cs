using UnityEngine;

public class StunGunTrigger: AbilityTrigger {
    public GameObject bulletPrefab;
    protected override void TriggerMe() {
        GameObject bullet = Instantiate(bulletPrefab,transform.position,Quaternion.identity);
        Projectile projectile = bullet.GetComponent<Projectile>();
        AudioManager.Instance.PlayAudio(AudioManager.GameSfx.stunShoot);
        projectile.Setup(_setting);
        Destroy(gameObject);
    }
}
