using System;
using UnityEngine;

namespace MyCombat
{
    public sealed class BeamWeapon : MonoBehaviour
    {
        public GameObject BeamPrefab;
        public GameObject MuzzleFlashPrefab;
        public GameObject ImpactEffectPrefab;
        public GameObject ImpactEffectDecalPrefab;
        public bool IsUsingAureole;
        public GameObject AureolePrefab;
        public float ImpactDecalLifeTime = 1f;

        public float MaxDistance = 1000;
        public LayerMask LayerMask = ~0;

        public float InstantiateDelay = 0;
        public float InstantiateMuzzleFlashDelay = 0;

        public float beamSize;

        private GameObject _laserBeamClone;
        private GameObject _muzzleFlashClone;
        private GameObject _impactEffect;
        private GameObject _impactEffectDecalPrefab;
        private BeamPositionSetter _beamPositionSetter;
        private Vector3 _prevImpactPosition;
        private float _startTime;

        private void Start()
        {
            _startTime = Time.time;

            Invoke("InstantiateBeam", InstantiateDelay);
            Invoke("InstantiateMuzzleFlash", InstantiateMuzzleFlashDelay);

            if (IsUsingAureole)
            {
                var aureoleClone = Instantiate(AureolePrefab, transform.position, transform.rotation);
                aureoleClone.transform.parent = transform;
                aureoleClone.transform.localScale = new Vector3(1, 1, 1);
                Destroy(aureoleClone, InstantiateDelay);
            }

            gameObject.SetActive(false);
        }

        private void InstantiateBeam()
        {
            _laserBeamClone = Instantiate(BeamPrefab, transform.position, transform.rotation);
            _laserBeamClone.transform.parent = transform;
            LineRenderer line_renderer = _laserBeamClone.GetComponent<LineRenderer>();
            line_renderer.widthMultiplier = 1;

            _beamPositionSetter = _laserBeamClone.GetComponent<BeamPositionSetter>();
        }


        private void InstantiateMuzzleFlash()
        {
            _muzzleFlashClone = Instantiate(MuzzleFlashPrefab, transform.position, transform.rotation);
            _muzzleFlashClone.transform.parent = transform;
        }

        private void Update()
        {
            if (Time.time - _startTime < InstantiateDelay)
                return;

            if (_laserBeamClone == null || _muzzleFlashClone == null)
                return;

            _laserBeamClone.transform.rotation = transform.rotation;

            RaycastHit hit;
            var ray = new Ray(transform.position, transform.forward);

            if (Physics.Raycast(ray, out hit, MaxDistance, LayerMask))
            {
                HandleLaserHit(hit);
            }
            else
            {
                HandleLaserMiss();
            }
        }

        private void HandleLaserHit(RaycastHit hit)
        {
            _beamPositionSetter.Set(transform.position, hit.point);

            // Create or update impact effect
            HandleImpactEffect(hit);

            // Handle impact decals if the hit point is different from the previous one
            if (hit.point != _prevImpactPosition)
            {
                HandleImpactDecals(hit);
            }

            _prevImpactPosition = hit.point;
        }

        private void HandleImpactEffect(RaycastHit hit)
        {
            if (_impactEffect != null)
            {
                _impactEffect.transform.position = hit.point;
            }
            else
            {
                _impactEffect = Instantiate(ImpactEffectPrefab, hit.point, new Quaternion());
                _impactEffect.transform.parent = transform;
            }
        }

        private void HandleImpactDecals(RaycastHit hit)
        {
            if (_impactEffectDecalPrefab != null)
            {
                Destroy(_impactEffectDecalPrefab, ImpactDecalLifeTime);
            }

            _impactEffectDecalPrefab = Instantiate(ImpactEffectDecalPrefab, hit.point, new Quaternion());
            _impactEffectDecalPrefab.transform.parent = transform;
            _impactEffectDecalPrefab.transform.LookAt(transform.position + hit.normal);
        }

        private void HandleLaserMiss()
        {
            _beamPositionSetter.Set(transform.position, transform.position + transform.forward * MaxDistance);

            if (_impactEffect != null)
            {
                Destroy(_impactEffect.gameObject);
                _impactEffect = null;
            }

            if (_impactEffectDecalPrefab != null)
            {
                Destroy(_impactEffectDecalPrefab, ImpactDecalLifeTime);
                _impactEffectDecalPrefab = null;
            }
        }
    }
}
