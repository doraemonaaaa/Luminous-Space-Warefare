using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace MyCombat
{
    public class BeamWeaponController : MonoBehaviour
    {
        public List<BeamWeapon> BeamWeaponPrefabs; // ����Ԥ���壨��Ԥ�����ڳ����У�

        public void OpenFire()
        {
            foreach (var BeamWeaponPrefab in BeamWeaponPrefabs)
                BeamWeaponPrefab.gameObject.SetActive(true);
        }

        public void HoldFire()
        {
            foreach (var BeamWeaponPrefab in BeamWeaponPrefabs)
                BeamWeaponPrefab.gameObject.SetActive(false);
        }
    }
}
