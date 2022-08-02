using Gameplay.RayKit.Options;
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
            RenderRay(localTransform.position);

            float remaining = rayEmitter.maxLength; // init
            for (var i = 0; i < rayEmitter.reflections; i++)
            {
                // hit
                bool isHit = Physics.Raycast(_ray.origin, _ray.direction, out _hit, remaining);
                if (isHit)
                {
                    RenderRay(_hit.point);
                    remaining -= Vector3.Distance(_ray.origin, _hit.point);
                    if (hitOptions.enabled && _hit.collider.gameObject == hitOptions.hitObject)
                    {
                        hitOptions.onHit?.Invoke();
                    }
                    // reflection on mirror
                    if (_hit.collider.CompareTag($"Mirror"))
                        _ray = new Ray(_hit.point, Vector3.Reflect(_ray.direction, _hit.normal));
                    else
                        break;
                }
                else // tailing
                {
                    RenderRay(_ray.origin + _ray.direction * remaining);
                }
            }
        }

        private void RenderRay(Vector3 pos)
        {
            int positionCount = _lineRenderer.positionCount;
            positionCount += 1;
            _lineRenderer.positionCount = positionCount;
            _lineRenderer.SetPosition(positionCount - 1, pos);
        }

    }
}
