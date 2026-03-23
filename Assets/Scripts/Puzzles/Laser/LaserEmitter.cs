using UnityEngine;
using System.Collections.Generic;

namespace ScapeRoom.Puzzles
{
    [RequireComponent(typeof(LineRenderer))]
    public class LaserEmitter : MonoBehaviour
    {
        [Header("Laser Settings")]
        [SerializeField] private int _maxBounces = 5;
        [SerializeField] private float _maxDistance = 100f;
        [SerializeField] private LayerMask _laserCollisionMask = ~0;
        
        private LineRenderer _lineRenderer;
        private List<Vector3> _laserPoints = new List<Vector3>();
        private LaserSensor _lastHitSensor;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            
            _lineRenderer.startWidth = 0.05f;
            _lineRenderer.endWidth = 0.05f;
            _lineRenderer.useWorldSpace = true;
        }

        private void Update()
        {
            UpdateLaser();
        }

        // Raycasting and reflections
        private void UpdateLaser()
        {
            _laserPoints.Clear();
            _laserPoints.Add(transform.position);

            Vector3 currentOrigin = transform.position;
            Vector3 currentDirection = transform.forward;
            
            LaserSensor hitSensor = null;

            for (int i = 0; i < _maxBounces; i++)
            {
                if (Physics.Raycast(currentOrigin, currentDirection, out RaycastHit hit, _maxDistance, _laserCollisionMask))
                {
                    _laserPoints.Add(hit.point);

                    LaserMirror mirror = hit.collider.GetComponent<LaserMirror>();
                    if (mirror != null)
                    {
                        currentDirection = Vector3.Reflect(currentDirection, hit.normal);
                        currentOrigin = hit.point + currentDirection * 0.01f;
                    }
                    else
                    {
                        hitSensor = hit.collider.GetComponent<LaserSensor>();
                        break; 
                    }
                }
                else
                {
                    _laserPoints.Add(currentOrigin + currentDirection * _maxDistance);
                    break;
                }
            }

            _lineRenderer.positionCount = _laserPoints.Count;
            _lineRenderer.SetPositions(_laserPoints.ToArray());

            ManageSensorActivation(hitSensor);
        }

        // Manage active target
        private void ManageSensorActivation(LaserSensor currentHitSensor)
        {
            if (_lastHitSensor != currentHitSensor)
            {
                if (_lastHitSensor != null) _lastHitSensor.OnLaserLost();
                if (currentHitSensor != null) currentHitSensor.OnLaserHit();
                
                _lastHitSensor = currentHitSensor;
            }

            if (_lastHitSensor != null)
            {
                _lastHitSensor.OnLaserStay(Time.deltaTime);
            }
        }
    }
}
