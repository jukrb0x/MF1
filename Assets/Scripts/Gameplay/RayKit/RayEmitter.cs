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
        public  HitOptions     hitOptions; //todo

        void Start()
        {
            _lineRenderer = GetComponent<LineRenderer>();
        }

        void Update()
        {
            var localTransform = transform;
            // first ray
            _ray = new Ray(localTransform.position, localTransform.forward);
            _lineRenderer.positionCount = 1;
            _lineRenderer.SetPosition(0, localTransform.position);

            float remaining = rayEmitter.maxLength; // init
            for (var i = 0; i < rayEmitter.reflections; i++)
            {
                // hit
                bool isHit = Physics.Raycast(_ray.origin, _ray.direction, out _hit, remaining);
                if (isHit)
                {
                    _lineRenderer.positionCount += 1;
                    _lineRenderer.SetPosition(_lineRenderer.positionCount - 1, _hit.point); // world
                    remaining -= Vector3.Distance(_ray.origin, _hit.point);
                    // todo: fracture when hit
                    // reflection
                    if (_hit.collider.CompareTag($"Mirror"))
                        _ray = new Ray(_hit.point, Vector3.Reflect(_ray.direction, _hit.normal));
                    else
                        break;
                }
                else // tailing
                {
                    _lineRenderer.positionCount += 1;
                    _lineRenderer.SetPosition(_lineRenderer.positionCount - 1, _ray.origin + _ray.direction * remaining);
                }
            }
        }




    }
}
