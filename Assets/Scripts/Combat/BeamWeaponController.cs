using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace MyCombat
{
    public class BeamWeaponController : MonoBehaviour
    {
        public List<BeamWeapon> BeamWeaponPrefabs; // 拖入预设体（或预放置在场景中）

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
