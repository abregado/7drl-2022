using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShooter : Enemy {
    private Transform _gun;
    private LineRenderer _line;    
    public float acceptableDist = 0.5f;
    public GameObject projectilePrefab;

    public bool validTarget;
    public Vector2 targetPoint;

    public override void Awake() {
        base.Awake();
        _line = GetComponentInChildren<LineRenderer>();
        _gun = transform.Find("Gun");
    }

    public override void UpdateVision(Vector2 target) {
        if (!_body.isStunned) {
            _line.SetPositions(new Vector3[] {_gun.position, _gun.position});

            Vector2 targetDirection = target - (Vector2) _gun.position;
            int mask = LayerMask.GetMask("Player", "MoveTarget", "Door");
            int wallMask = LayerMask.NameToLayer("MoveTarget");

            RaycastHit2D hit = Physics2D.Raycast(_gun.position, targetDirection, 5000f, mask);

            if (hit.collider) {
                if (hit.collider.gameObject.layer == wallMask && Vector2.Distance(hit.point, target) < acceptableDist) {
                    _line.SetPositions(new Vector3[] {_gun.position, target});
                    targetPoint = target;
                    validTarget = true;
                    SetIndicatorState(true);
                    return;
                }
            }
        }
        _line.SetPositions(new Vector3[] {_gun.position, _gun.position});
        validTarget = false;
    }

    public override void EndUpdate() {
        validTarget = false;
    }

    public override void ShootingPhase() {
        if (!_body.isStunned && validTarget) {
            Fire();
        }
    }
    
    public void Fire() {
        GameObject bullet = Instantiate(projectilePrefab, _gun.position, Quaternion.identity);
        AudioManager.Instance.PlayAudio(AudioManager.GameSfx.canonShoot);
        bullet.GetComponent<Projectile>().Setup(targetPoint);
    }

    public override void SetIndicatorState(bool state) {
        if (_body.isStunned || validTarget == false) {
            state = false;
        }
        _line.enabled = state;
    }

    public override void Reset() {
        base.Reset();
        validTarget = false;
        SetIndicatorState(false);
    }
}
