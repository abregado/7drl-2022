using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class MovementRaycaster : MonoBehaviour {
    private const float _acceptanceClickRange = 0.25f;
    
    public Camera myCamera;
    public float maxAimAngle = 30f;
    public Vector2 targetPoint = Vector2.negativeInfinity;

    private bool _registered;
    
    private LineRenderer _lineRenderer;
    private TurnDirector _director;
    private WallJumper _wallJumper;
    private MeshFilter _moveArcFilter;
    private RoomManager _roomManager;
    private Transform _clickHelper;

    private void Awake() {
        _lineRenderer = transform.Find("LineRenderer").GetComponent<LineRenderer>();
        _moveArcFilter = FindObjectOfType<MoveArcIndicator>().GetComponent<MeshFilter>();
        _wallJumper = GetComponent<WallJumper>();
        _director = GetComponent<TurnDirector>();
        _roomManager = FindObjectOfType<RoomManager>();
        _clickHelper = FindObjectOfType<ClickHelper>().transform;
    }

    public void PhaseUpdate()
    {
        var worldMousePos = myCamera.ScreenToWorldPoint(Input.mousePosition);

        if (!_registered && Input.GetMouseButtonUp(0) && IsTargetPointValid() && Vector2.Distance(targetPoint, worldMousePos) <= _acceptanceClickRange) {
            _director.NextPhase();
            return;
        }
        
        if (Input.GetMouseButtonUp(0)) {
            _registered = false;
            if (IsTargetPointValid()) {
                _clickHelper.transform.position = targetPoint;
            }
            else {
                _clickHelper.transform.position = new Vector3(1000f,1000f,0f);
                _roomManager.SetEnemyIndicatorState(false);
                ResetTargetPoint();
            }
            return;
        }

        if (Input.GetMouseButtonDown(0)) {
            _clickHelper.transform.position = new Vector3(1000f,1000f,0f);
            if (_registered == false && targetPoint != Vector2.negativeInfinity &&
                Vector2.Distance(targetPoint, worldMousePos) > _acceptanceClickRange) {
                _registered = true;
            }
        }

        if (Input.GetMouseButton(0)) {
            if (_registered) {
                UpdateMovementLine(worldMousePos);
            }
        }

        
        
        if (Input.GetMouseButtonUp(1)) {
            ResetTargetPoint();
            _roomManager.SetEnemyIndicatorState(false);
            _registered = false;
            return;
        }
        
        
        
//---------------------------------------------------------------
        // if (Input.GetMouseButtonUp(0)) {
        //     //move on if the player clicks in the same place as an existing, valid location.
        //     if (_registered) {
        //         if (IsTargetPointValid() && Vector2.Distance(targetPoint, worldMousePos) < _acceptanceClickRange) {
        //             
        //             _director.NextPhase();
        //             return;
        //         }
        //     }
        //     else if (IsTargetPointValid()) {
        //         _registered = true;
        //         _roomManager.SetEnemyIndicatorState(true);
        //         _clickHelper.position = targetPoint;
        //         return;
        //     }
        // }
        //
        // if (Input.GetMouseButtonDown(0)){
        //     if (Vector2.Distance(targetPoint, worldMousePos) > _acceptanceClickRange) {
        //         _registered = false;
        //         _clickHelper.position = new Vector2(1000f, 1000f);
        //         return;
        //     }
        //
        //     if (IsTargetPointValid()) {
        //         _roomManager.SetEnemyIndicatorState(false);
        //         return;
        //     }
        //     
        // }
        //
        // if (Input.GetMouseButton(0)) {
        //     //shoot a ray at the mouse
        //     
        //     
        //     return;
        // }
        //
        // if (Input.GetMouseButtonUp(1)) {
        //     ResetTargetPoint();
        //     _roomManager.SetEnemyIndicatorState(false);
        //     return;
        // }
    }

    public void UpdateMovementLine(Vector2 worldMousePos) {
        Vector2 pos = transform.position;
        Vector2 targetDirection = worldMousePos - pos;

        int mask = LayerMask.GetMask("MoveTarget","Enemy", "Door");
        int wallMask = LayerMask.NameToLayer("MoveTarget");

        RaycastHit2D hit = Physics2D.Raycast(pos, targetDirection, 5000f, mask);

        if (hit.collider) {
            _lineRenderer.SetPositions(new Vector3[] {pos, hit.point});
            if (hit.collider.gameObject.layer != wallMask || IsWithinViewCone(hit.point) == false) {
                _lineRenderer.endColor = Color.red;
                targetPoint = Vector2.negativeInfinity;
            }
            else {
                _lineRenderer.endColor = Color.green;
                targetPoint = hit.point;
                _roomManager.UpdateEnemyVision(targetPoint);
            }
        }
        else {
            targetPoint = Vector2.negativeInfinity;
        }
    }
    
    public bool IsTargetPointValid() {
        return targetPoint != Vector2.negativeInfinity && IsWithinViewCone(targetPoint);
    }

    public void ResetTargetPoint() {
        targetPoint = Vector2.negativeInfinity;
        _lineRenderer.SetPositions(new Vector3[]{transform.position,transform.position});
    }

    public Vector2 GetNextPosition() {
        Vector2 nearPoint =  targetPoint - (Vector2) transform.position;
        nearPoint *= 0.999f;
        return nearPoint + (Vector2) transform.position;
    }
    
    public bool IsWithinViewCone(Vector2 point) {
        
        Vector2 directionToPoint = point - (Vector2) transform.position;
        float angle = Vector2.SignedAngle(_wallJumper.currentFacing, directionToPoint);
        
        return Math.Abs(angle) < maxAimAngle;
    }

    public void DrawMoveArcTriangle() {
        float depth = 5000f;
        float currentRotation = Vector2.SignedAngle(Vector2.up,_wallJumper.currentFacing);
        float maxRot = currentRotation + maxAimAngle;
        if (maxRot < 0f) {
            maxRot += 360f;
        }
        float minRot = currentRotation - maxAimAngle;
        if (minRot < 0f) {
            minRot += 360f;
        }
        maxRot *= Mathf.Deg2Rad;
        minRot *= Mathf.Deg2Rad;
        Vector2 maxPoint1 = new Vector2(depth * Mathf.Sin(maxRot), -depth * Mathf.Cos(maxRot));
        Vector2 maxPoint2 = new Vector2(depth * Mathf.Sin(minRot), -depth * Mathf.Cos(minRot));

        Mesh triangle = new Mesh();
        triangle.vertices = new Vector3[] {
            transform.position,
            transform.position - (Vector3) maxPoint1,
            transform.position - (Vector3) maxPoint2,
        };
        triangle.uv = new Vector2[] {
            Vector3.zero,
            maxPoint1,
            maxPoint2,
        };
        triangle.triangles = new int[] {0, 1, 2};
        _moveArcFilter.mesh = triangle;
    }
    
    public Vector2 GetClosestPointOnMoveLine() {
        Vector2 mStart = transform.position;
        Vector2 mEnd = GetNextPosition();
        var mPos = (Vector2) myCamera.ScreenToWorldPoint(Input.mousePosition);

        Vector2 a_to_p = mPos - mStart;
        Vector2 a_to_b = mEnd - mStart;

        float dist = Vector2.Dot(a_to_p, a_to_b) / a_to_b.sqrMagnitude;

        dist = Math.Clamp(dist, 0.001f, 0.999f);
        Vector2 closest = mStart+a_to_b*dist;
        return closest;
    }

    public float GetPercentageOfLine(Vector2 lStart,Vector2 lEnd1,Vector2 lEnd2) {
        float mag1 = (lEnd1 - lStart).magnitude;
        float mag2 = (lEnd2 - lStart).magnitude;

        return mag2 / mag1;
    }

    public Vector2 LinePercentToPos(Vector2 lStart, Vector2 lEnd, float percent) {
        Vector2 vect = lEnd - lStart;
        return (vect.normalized * percent) + lStart;
    }

    public void SetMoveArcState(bool state) {
        _moveArcFilter.GetComponent<MeshRenderer>().enabled = state;
    }

    public void SetMoveLineState(bool state) {
        _lineRenderer.enabled = state;
    }
}
