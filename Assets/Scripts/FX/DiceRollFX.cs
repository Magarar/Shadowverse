using Unit;
using UnityEngine;

namespace FX
{
    /// <summary>
    /// Dice roll FX, coded for 6 faces only
    /// </summary>
    public class DiceRollFX: MonoBehaviour
    {
        public int value;

        [Header("Anim")]
        public Transform dice;
        public float rollSpeed = 20f;
        public float rollDuration = 1f;
        public AudioClip startAudio;
        public AudioClip endAudio;
        
        private Vector3[] dir;

        private bool ended = false;
        private float timer = 0f;
        private float x = 0f;
        private float y = 0f;
        private float z = 0f;
        
        void Start()
        {
            //Direction of each face
            dir = new Vector3[6];
            dir[0] = Vector3.forward;  //one
            dir[1] = Vector3.up;  //two
            dir[2] = Vector3.right;  //three
            dir[3] = Vector3.left;  //four
            dir[4] = Vector3.down;  //five
            dir[5] = Vector3.back;  //six

            AudioTool.Get().PlaySFX("dice", startAudio);
        }

        void Update()
        {
            timer += Time.deltaTime;
            
            if (!ended)
            {
                if (timer < rollDuration)
                {
                    x += 5f * Time.deltaTime;
                    y += 7f * Time.deltaTime;
                    dice.Rotate(x * rollSpeed, y * rollSpeed, z * rollSpeed, Space.Self);
                }
                else
                {
                    ended = true;
                    timer = 0f;
                    AudioTool.Get().PlaySFX("dice", endAudio);
                }
            }
            
            if (ended)
            {
                if (value >= 1 && value <= dir.Length)
                {
                    Vector3 target = dir[value - 1];
                    Vector3 up = target.y > target.z ? Vector3.back : Vector3.up;
                    Quaternion trot = Quaternion.LookRotation(target, up);
                    dice.localRotation = Quaternion.Slerp(dice.localRotation, trot, rollSpeed * Time.deltaTime);
                }

                if (timer > 1f)
                {
                    Destroy(gameObject);
                }
            }
        }
        
        
    }
}