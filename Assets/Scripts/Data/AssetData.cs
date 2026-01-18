using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;

namespace Data
{
    [CreateAssetMenu(fileName = "AssetData", menuName = "Data/AssetData")]
    public class AssetData:ScriptableObject
    {
        [Header("FX")] 
        public GameObject cardSpawnFX;
        public GameObject cardDestroyFX;
        public GameObject cardAttackFX;
        public GameObject cardDamageFX;
        public GameObject cardExhaustedFX;
        public GameObject playerDamageFX;
        public GameObject damageFX;
        public GameObject playCardFX;
        public GameObject playOtherCardFX;
        public GameObject playSecretFX;
        public GameObject playSecretOtherFX;
        public GameObject diceRollFX;
        public GameObject hoverTextBox;
        public GameObject newTurnFX;
        public GameObject winFX;
        public GameObject loseFX;
        public GameObject tiedFX;
        
        [Header("Audio")]
        public AudioClip cardSpawnAudio;
        public AudioClip cardDestroyAudio;
        public AudioClip cardAttackAudio;
        public AudioClip cardDamageAudio;
        public AudioClip cardExhaustedAudio;
        public AudioClip playerDamageAudio;
        public AudioClip damageAudio;
        public AudioClip playCardAudio;
        public AudioClip playOtherCardAudio;
        public AudioClip playSecretAudio;
        public AudioClip playSecretOtherAudio;
        public AudioClip diceRollAudio;
        public AudioClip newTurnAudio;
        public AudioClip winAudio;
        public AudioClip loseAudio;
        public AudioClip tiedAudio;
        public AudioClip handCardClickAudio;
        public AudioClip winMusic;
        public AudioClip defaultmusic;
        public AudioClip evolveAudio;

        public static AssetData Get()
        {
            return DataLoader.Get().assetData;
        }


    }

    
}