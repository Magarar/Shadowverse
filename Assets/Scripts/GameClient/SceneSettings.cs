using Unit;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameClient
{
    /// <summary>
    /// Component added to a scene to add some generic sfx/music to the arena
    /// </summary>
    public class SceneSettings: MonoBehaviour
    {
        public AudioClip startAudio;
        public AudioClip[] gameMusic;
        public AudioClip[] gameAmbience;

        private static SceneSettings instance;

        private void Awake()
        {
            instance = this;
        }

        void Start()
        {
            AudioTool.Get().PlaySFX("game_sfx", startAudio);
            if (gameMusic.Length > 0)
                AudioTool.Get().PlayMusic("music", gameMusic[Random.Range(0, gameMusic.Length)]);
            if (gameAmbience.Length > 0)
                AudioTool.Get().PlaySFX("ambience", gameAmbience[Random.Range(0, gameAmbience.Length)], 0.5f, true);
        }

        void Update()
        {

        }

        public static SceneSettings Get()
        {
            return instance;
        }
    }
}