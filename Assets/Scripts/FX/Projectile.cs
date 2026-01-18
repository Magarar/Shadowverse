using GameClient;
using UI;
using Unit;
using UnityEngine;
using UnityEngine.Serialization;

namespace FX
{
    public class Projectile:MonoBehaviour
    {
        public float speed = 10f;
        public float duration = 4f;
        public GameObject explodeFX;
        public AudioClip explodeAudio;

        [HideInInspector]
        public int damage; //Damage dealth by projectile, to delay HP display by this amount

        private Transform source;
        private Transform target;
        private Vector3 source_offset;
        private Vector3 target_offset;
        private float timer = 0f;
        
        public void DelayDamage()
        {
            BoardCard tcard = target?.GetComponent<BoardCard>();
            if (tcard != null)
            {
                //Delay visual HP so that the HP dont change before projectile hit
                tcard.DelayDamage(damage, 8f / speed);
            }

            BoardSlotPlayer pslot = target?.GetComponent<BoardSlotPlayer>();
            if (pslot != null)
            {
                PlayerUI playerUI = PlayerUI.Get(pslot.GetPlayerID() != Gameclient.Get().GetPlayerID());
                playerUI.DelayDamage(damage, 8f / speed);
            }
        }
        
        void Update()
        {
            timer += Time.deltaTime;

            if (source == null || target == null)
            {
                Destroy(gameObject);
                return;
            }

            if (timer > duration)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 spos = transform.position;
            Vector3 tpos = target.position + target_offset;
            Vector3 dir = (tpos - spos);
            transform.position += dir.normalized * Mathf.Min(dir.magnitude, 1f) * speed * Time.deltaTime;
            transform.rotation = GetFXRotation(dir.normalized);

            if (dir.magnitude < 0.2f)
            {
                FXTool.DoFX(explodeFX, target.position);
                AudioTool.Get().PlaySFX("fx", explodeAudio);
                Destroy(gameObject);
            }
        }
        
        public void SetSource(Transform source)
        {
            this.source = source;
            transform.position = source.position;
        }

        public void SetSource(Transform source, Vector3 offset)
        {
            this.source = source;
            source_offset = offset;
            transform.position = source.position + source_offset;
        }

        public void SetTarget(Transform target)
        {
            this.target = target;
        }

        public void SetTarget(Transform target, Vector3 offset)
        {
            this.target = target;
            target_offset = offset;
        }

        private static Quaternion GetFXRotation(Vector3 dir)
        {
            GameBoard board = GameBoard.Get();
            Vector3 facing = board != null ? board.transform.forward : Vector3.forward;
            return Quaternion.LookRotation(facing, dir);
        }

     
    }
}