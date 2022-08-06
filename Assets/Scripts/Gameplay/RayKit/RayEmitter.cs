using Gameplay.RayKit.Options;
using Unity.VisualScripting;
using UnityEngine;

namespace Gameplay.RayKit
{
    [RequireComponent(typeof(LineRenderer))]
    public class RayEmitter : MonoBehaviour
    {
        private LineRenderer   _lineRenderer;
        private Ray            _ray;
        private RaycastHit     _hit;
        private Vector3        _direction;
        public  EmitterOptions rayEmitter;
        public  HitOptions     hitOptions;

        private void Start()
        {
            _lineRenderer = GetComponent<LineRenderer>();
        }
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, transform.forward * rayEmitter.maxLength);
        }

        public void Update()
        {
            var localTransform = transform;
            // first ray
            _ray = new Ray(localTransform.position, localTransform.forward);
            // reset line renderer every frame otherwise it will leave tails
            _lineRenderer.positionCount = 0;
            RenderRayAt(localTransform.position);

            float remaining = rayEmitter.maxLength; // init
            for (var i = 0; i < rayEmitter.reflections; i++)
            {
                // hit
                bool isHit = Physics.Raycast(_ray.origin, _ray.direction, out _hit, remaining);
                if (isHit)
                {
                    // test if hit the target
                    RenderRayAt(_hit.point);
                    remaining -= Vector3.Distance(_ray.origin, _hit.point);
                    
                    // hit the target and trigger
                    if (hitOptions.enabled && hitOptions.hitType == HitType.Trigger
                                           && _hit.collider.gameObject == hitOptions.hitTarget)
                    {
                        hitOptions.onHit?.Invoke();
                    }
                    // raycast fracture
                    else if (hitOptions.enabled && hitOptions.hitType == HitType.RaycastHitFracture)
                    {
                        // if hit the fracture objects
                        if (_hit.collider.CompareTag($"Fracture"))
                        {
                            var fracture = _hit.collider.gameObject.GetComponent<Fracture.Runtime.Scripts.Fracture>();
                            if (fracture != null)
                            {
                                fracture.FirstRaycastHit = _hit;
                                fracture.RaycastFracture();
                                hitOptions.onHit?.Invoke();
                            }
                        }
                    }
                    
                    // reflection on mirror
                    if (_hit.collider.CompareTag($"Mirror"))
                        _ray = new Ray(_hit.point, Vector3.Reflect(_ray.direction, _hit.normal));
                    else
                        break;
                }
                else // tailing
                {
                    RenderRayAt(_ray.origin + _ray.direction * remaining);
                }
            }
        }

        private void RenderRayAt(Vector3 pos)
        {
            int positionCount = _lineRenderer.positionCount;
            positionCount += 1;
            _lineRenderer.positionCount = positionCount;
            _lineRenderer.SetPosition(positionCount - 1, pos);
        }

    }
}
