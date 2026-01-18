using FX;
using GameClient;
using GameLogic;
using UnityEngine;

namespace Unit
{
    /// <summary>
    /// Static functions to spawn FX prefabs
    /// </summary>
    public class FXTool:MonoBehaviour
    {
        public static GameObject DoFX(GameObject fxPrefab, Vector3 pos, float duration = 5f)
        {
            if (fxPrefab != null)
            {
                GameObject fx = Instantiate(fxPrefab, pos, GetFXRotation());
                Destroy(fx, duration);
                return fx;
            }
            return null;
        }
        
        public static GameObject DoSnapFX(GameObject fxPrefab, Transform snapTarget)
        {
            return DoSnapFX(fxPrefab, snapTarget, Vector3.zero);
        }

        private static GameObject DoSnapFX(GameObject fxPrefab, Transform snapTarget, Vector3 offset,float duration = 5f)
        {
            if (fxPrefab != null)
            {
                GameObject fx = Instantiate(fxPrefab, snapTarget.position+offset, GetFXRotation());
                SnapFX snap = fx.AddComponent<SnapFX>();
                snap.target = snapTarget;
                snap.offset = offset;
                Destroy(fx, duration);
                return fx;
            }
            return null;
        }
        
        public static GameObject DoProjectileFX(GameObject fxPrefab, Transform source, Transform target, int damage)
        {
            if (fxPrefab != null && source != null && target != null)
            {
                GameObject fx = Instantiate(fxPrefab, source.position, GetFXRotation());
                Projectile projectile = fx.GetComponent<Projectile>();
                if (projectile == null)
                    projectile = fx.AddComponent<Projectile>();

                projectile.SetSource(source);
                projectile.SetTarget(target);
                projectile.damage = damage;
                projectile.DelayDamage();

                Destroy(fx, projectile.duration);
                return fx;
            }
            return null;
        }

        private static Quaternion GetFXRotation()
        {
            GameBoard board = GameBoard.Get();
            Vector3 facing = board != null ? board.transform.forward : Vector3.forward;
            return Quaternion.LookRotation(facing,Vector3.up);
        }
    }
}