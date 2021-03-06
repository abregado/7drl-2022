using System;
using UnityEngine;

public class Body: MonoBehaviour {
    public bool isStunned => stunnedTurns > 0;
    public int stunnedTurns;

    public bool immuneToStun;
    public bool immuneToKill;

    public GameObject deathExplosion;
    
    public virtual void UpdateStates() {
        if (stunnedTurns > 0) {
            stunnedTurns -= 1;
        }
    }
    
    public virtual void Stun(int turns) {
        if (!immuneToStun) {
            AudioManager.Instance.PlayAudio(AudioManager.GameSfx.stunHit);
            stunnedTurns += turns;
            stunnedTurns = Math.Min(stunnedTurns, 2);
        }
    }

    public virtual void Kill() {
        if (!immuneToKill) {
            if (deathExplosion != null) {
                Transform explosionLocation = transform.Find("ExplosionLocation").transform;
                Instantiate(deathExplosion, explosionLocation.position, Quaternion.identity);
            } 
            gameObject.SetActive(false); 
        }
    }

    public virtual void Reset() {
        stunnedTurns = 0;
    }
}
