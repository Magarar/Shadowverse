using TMPro;
using Unit;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Text that is displayed at the bottom of the screen when things cant be done
    /// </summary>
    public class WarningText: MonoBehaviour
    {
        public AudioClip warningAudio;
        public TextMeshProUGUI text;
        
        private CanvasGroup canvasGroup;
        private Animator animator;

        private static WarningText instance;

        void Awake()
        {
            instance = this;
            canvasGroup = GetComponent<CanvasGroup>();
            animator = GetComponent<Animator>();
            canvasGroup.alpha = 0f;
        }
        
        public void Show(string txt)
        {
            text.text = txt;
            canvasGroup.alpha = 1f;
            animator.SetTrigger("play");
            AudioTool.Get().PlaySFX("warning", warningAudio, 0.7f, false);
        }
        
        public static void ShowText(string txt)
        {
            WarningText w = Get();
            w.Show(txt);
        }

        public static void ShowNotYourTurn()
        {
            ShowText("Not your turn");
        }

        public static void ShowExhausted()
        {
            ShowText("No more action");
        }

        public static void ShowNoMana()
        {
            ShowText("Not enough Mana");
        }

        public static void ShowSpellImmune()
        {
            ShowText("Spell Immunity");
        }

        public static void ShowInvalidTarget()
        {
            ShowText("Invalid target");
        }
        
        public static void ShowCantEvolutionary()
        {
            ShowText("Cant Evolutionary Card");
        }

        public static WarningText Get()
        {
            return instance;
        }

        
    }
}