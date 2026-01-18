using System.Collections;
using System.Collections.Generic;
using Data;
using GameClient;
using GameLogic;
using Unit;
using UnityEngine;

namespace FX
{
    /// <summary>
    /// All FX/anims related to a card on the board
    /// </summary>
    public class BoardCardFX : MonoBehaviour
    {
        public Material killMat;
        public string killMatFade = "noise_fade";

        private BoardCard bcard;

        private ParticleSystem exhaustedFx = null;

        private Dictionary<StatusType, GameObject> statusFxList = new Dictionary<StatusType, GameObject>();
        
        void Awake()
        {
            bcard = GetComponent<BoardCard>();
            bcard.onKill += OnKill;
        }

        void Start()
        {
            Gameclient client = Gameclient.Get();
            client.onCardMoved += OnMove;
            client.onCardPlayed += OnPlayed;
            client.onCardDamaged += OnCardDamaged;
            client.onAttackStart += OnAttack;
            client.onAttackPlayerStart += OnAttackPlayer;
            client.onAbilityStart += OnAbilityStart;
            client.onAbilityTargetCard += OnAbilityEffect;
            client.onAbilityEnd += OnAbilityAfter;
            
            client.onCardEvolved += OnCardEvolved;
            client.onCardSuperEvolved += OnCardSuperEvolved;
            client.onPlayerEvolveCard += OnPlayerEvolveCard;
            client.onPlayerSuperEvolveCard += OnPlayerSuperEvolveCard;

            OnSpawn();
        }

        private void OnDestroy()
        {
            Gameclient client = Gameclient.Get();
            client.onCardMoved -= OnMove;
            client.onCardPlayed -= OnPlayed;
            client.onCardDamaged -= OnCardDamaged;
            client.onAttackStart -= OnAttack;
            client.onAttackPlayerStart -= OnAttackPlayer;
            client.onAbilityStart -= OnAbilityStart;
            client.onAbilityTargetCard -= OnAbilityEffect;
            client.onAbilityEnd -= OnAbilityAfter;
            
            client.onCardEvolved -= OnCardEvolved;
            client.onCardSuperEvolved -= OnCardSuperEvolved;
            client.onPlayerEvolveCard -= OnPlayerEvolveCard;
            client.onPlayerSuperEvolveCard -= OnPlayerSuperEvolveCard;
        }

        void Update()
        {
            if (!Gameclient.Get().IsReady())
                return;
            
            
            Card card = bcard.GetCard();
            
            //Status FX
            List<CardStatus> statusAll = card.GetAllStatus();
            foreach (CardStatus status in statusAll)
            {
                StatusData istatus = StatusData.Get(status.type);
                if (istatus != null && !statusFxList.ContainsKey(status.type) && istatus.statusFX != null)
                {
                    GameObject fx = Instantiate(istatus.statusFX, transform);
                    fx.transform.localPosition = Vector3.zero;
                    statusFxList[istatus.effect] = fx;
                }
            }
            
            //Remove status FX
            List<StatusType> removeList = new List<StatusType>();
            foreach (KeyValuePair<StatusType, GameObject> pair in statusFxList)
            {
                if (!card.HasStatus(pair.Key))
                {
                    removeList.Add(pair.Key);
                    Destroy(pair.Value);
                }
            }
            
            foreach (StatusType status in removeList)
                statusFxList.Remove(status);
            
            //Exhausted add/remove
            if (exhaustedFx != null && !exhaustedFx.isPlaying && card.exhausted)
                exhaustedFx.Play();
            if (exhaustedFx != null && exhaustedFx.isPlaying && !card.exhausted)
                exhaustedFx.Stop();

        }

        private void OnSpawn()
        {
            CardData icard = bcard.GetCardData();
            
            //Spawn Audio
            AudioClip audio = icard?.spawnAudio != null ? icard.spawnAudio : AssetData.Get().cardSpawnAudio;
            AudioTool.Get().PlaySFX("card_spawn", audio);
            
            //Spawn FX
            GameObject spawnFX = icard.cardSpawnFX != null ? icard.cardSpawnFX : AssetData.Get().cardSpawnFX;
            FXTool.DoFX(spawnFX, transform.position);
            
            //Spawn dissolve fx
            if (GameTool.IsURP())
            {
                SpriteRenderer render = bcard.cardSprite;
                render.material = killMat;

                FadeSetVal(bcard.cardSprite, 0f);
                FadeKill(bcard.cardSprite, 1f, 0.5f);
            }
            
            //Exhausted fx
            if (AssetData.Get().cardExhaustedFX != null)
            {
                GameObject efx = Instantiate(AssetData.Get().cardExhaustedFX, transform);
                efx.transform.localPosition = Vector3.zero;
                exhaustedFx = efx.GetComponent<ParticleSystem>();
            }

            //Idle status
            TimeTool.WaitFor(1f, () =>
            {
                if (icard.cardIdleFX != null)
                {
                    GameObject fx = Instantiate(icard.cardIdleFX, transform);
                    fx.transform.localPosition = Vector3.zero;
                }
            });

        }
        
        private void OnKill()
        {
            StartCoroutine(KillRoutine());
        }

        private IEnumerator KillRoutine()
        {
            yield return new WaitForSeconds(0.5f);
            
            CardData icard = bcard.GetCardData();
            
            //Death FX
            GameObject deathFX = icard.cardDestroyFX != null ? icard.cardDestroyFX : AssetData.Get().cardDestroyFX;
            FXTool.DoFX(deathFX, transform.position);

            //Death audio
            AudioClip audio = icard?.destroyAudio  != null ? icard.destroyAudio : AssetData.Get().cardDestroyAudio;
            AudioTool.Get().PlaySFX("card_spawn", audio);

            //Death dissolve fx
            if (GameTool.IsURP())
            {
                FadeKill(bcard.cardSprite, 0f, 0.5f);
            }
        }
        
        private void FadeSetVal(SpriteRenderer render, float val)
        {
            render.material = killMat;
            render.material.SetFloat(killMatFade, val);
        }

        
        private void FadeKill(SpriteRenderer render, float val, float duration)
        {
            AnimMatFX anim = AnimMatFX.Create(render.gameObject, render.material);
            anim.SetFloat(killMatFade, val, duration);
        }
        
        private void OnMove(Card card, Slot slot)
        {
            
        }

        private void OnPlayed(Card card, Slot slot)
        {
            Card ecard = bcard?.GetEquipCard();
            if (ecard != null && card.uid == ecard.uid && transform != null)
            {
                FXTool.DoFX(ecard.CardData.cardSpawnFX, transform.position);
                AudioTool.Get().PlaySFX("card_spawn", ecard.CardData.spawnAudio);
            }
        }
        
        private void OnCardDamaged(Card target, int damage)
        {
            Card card = bcard.GetCard();
            if (card.uid == target.uid && damage > 0)
            {
                DamageFX(bcard.transform, damage);
            }
        }
        
        private void OnAttack(Card attacker, Card target)
        {
            Card card = bcard.GetCard();
            CardData icard = bcard.GetCardData();
            if (attacker == null || target == null)
                return;

            if (card.uid == attacker.uid)
            {
                BoardCard btarget = BoardCard.Get(target.uid);
                if (btarget != null)
                {
                    //Card charge into target
                    ChargeInto(btarget);

                    //Attack FX and Audio
                    GameObject fx = icard.cardAttackFX != null ? icard.cardAttackFX : AssetData.Get().cardAttackFX;
                    FXTool.DoSnapFX(fx, transform);
                    AudioClip audio = icard?.attackAudio != null ? icard.attackAudio : AssetData.Get().cardAttackAudio;
                    AudioTool.Get().PlaySFX("card_attack", audio);

                    //Equip FX
                    Card ecard = bcard.GetEquipCard();
                    if (ecard != null)
                    {
                        FXTool.DoFX(ecard.CardData.cardAttackFX, transform.position);
                        AudioTool.Get().PlaySFX("card_attack_equip", ecard.CardData.attackAudio);
                    }
                }
            }

        }
        
        private void OnAttackPlayer(Card attacker, Player player)
        {
            if (attacker == null || player == null)
                return;

            Card card = bcard.GetCard();
            if (card.uid == attacker.uid)
            {
                bool isOther = player.id != Gameclient.Get().GetPlayerID();
                CardData icard = bcard.GetCardData();
                BoardSlotPlayer zone = BoardSlotPlayer.Get(isOther);

                ChargeIntoPlayer(zone);

                AudioClip audio = icard?.attackAudio != null ? icard.attackAudio : AssetData.Get().cardAttackAudio;
                AudioTool.Get().PlaySFX("card_attack", audio);

                //Equip FX
                Card ecard = bcard.GetEquipCard();
                if (ecard != null)
                {
                    FXTool.DoFX(ecard.CardData.cardAttackFX, transform.position);
                    AudioTool.Get().PlaySFX("card_attack_equip", ecard.CardData.attackAudio);
                }
            }
        }
        
        
        private void DamageFX(Transform target, int value, float delay = 0.5f)
        {
            TimeTool.WaitFor(delay, () =>
            {
                GameObject fx = FXTool.DoFX(AssetData.Get().damageFX, target.position);
                fx.GetComponent<DamageFX>().SetValue(value);
            });
        }
        
        private void ChargeInto(BoardCard target)
        {
            if (target != null)
            {
                ChargeInto(target.gameObject);

                CardData icard = target.GetCardData();
                TimeTool.WaitFor(0.25f, () =>
                {
                    //Damage fx and audio
                    GameObject prefab = icard.cardDamageFX ? icard.cardDamageFX : AssetData.Get().cardDamageFX;
                    AudioClip audio = icard.damageAudio? icard.damageAudio : AssetData.Get().cardDamageAudio;
                    FXTool.DoFX(prefab, target.transform.position);
                    AudioTool.Get().PlaySFX("card_hit", audio);
                });
            }
        }
        
        private void ChargeIntoPlayer(BoardSlotPlayer target)
        {
            if (target != null)
            {
                ChargeInto(target.gameObject);

                TimeTool.WaitFor(0.25f, () =>
                {
                    //Damage fx and audio
                    FXTool.DoFX(AssetData.Get().playerDamageFX, target.transform.position);
                    AudioClip audio = AssetData.Get().playerDamageAudio;
                    AudioTool.Get().PlaySFX("card_hit", audio);
                });
            }
        }
        
        private void ChargeInto(GameObject target)
        {
            if (target != null)
            {
                int currentOrder = bcard.cardSprite.sortingOrder;
                Vector3 dir = target.transform.position - transform.position;
                Vector3 targetPos = target.transform.position - dir.normalized * 1f;
                Vector3 currentPos = transform.position;
                bcard.SetOrder(currentOrder + 10);

                AnimFX anim = AnimFX.Create(gameObject);
                anim.MoveTo(currentPos - dir.normalized * 0.5f, 0.3f);
                anim.MoveTo(target.transform.position, 0.1f);
                anim.MoveTo(currentPos, 0.3f);
                anim.Callback(0f, () =>
                {
                    if (bcard != null)
                        bcard.SetOrder(currentOrder);
                });
            }
        }
        
        private void OnAbilityStart(AbilityData iability, Card caster)
        {
            if (iability != null && caster != null)
            {
                if (caster.uid == bcard.GetCardUID())
                {
                    FXTool.DoSnapFX(iability.casterFX, bcard.transform);
                    AudioTool.Get().PlaySFX("ability", iability.castAudio);
                }
            }
        }
        
        private void OnAbilityAfter(AbilityData iability, Card caster)
        {
            if (iability != null && caster != null)
            {
                if (caster.uid == bcard.GetCardUID())
                {

                }
            }
        }
        
        private void OnAbilityEffect(AbilityData iability, Card caster, Card target)
        {
            if (iability != null && caster != null && target != null)
            {
                if (target.uid == bcard.GetCardUID())
                {
                    FXTool.DoSnapFX(iability.targetFX, bcard.transform);
                    FXTool.DoProjectileFX(iability.projectileFX, GetFXSource(caster), bcard.transform, iability.GetDamage());
                    AudioTool.Get().PlaySFX("ability_effect", iability.targetAudio);
                }

                if (caster.uid == bcard.GetCardUID())
                {
                    if (iability.chargeTarget && caster.CardData.IsBoardCard())
                    {
                        BoardCard btarget = BoardCard.Get(target.uid);
                        ChargeInto(btarget);
                    }
                }
            }
        }

        private void OnPlayerEvolveCard(Card card)
        {
            if (card != null && card.uid == bcard.GetCardUID())
            {
                
            }
        }

        private void OnPlayerSuperEvolveCard(Card card)
        {
            if (card != null && card.uid == bcard.GetCardUID())
            {
                
            }
        }

        private void OnCardEvolved(Card card, bool isPlayer)
        {
            if (card != null && card.uid == bcard.GetCardUID())
            {
                AudioClip clip = card.CardData.evolutionAudio==null? AssetData.Get().evolveAudio : card.CardData.evolutionAudio;
                AudioTool.Get().PlaySFX("ability_effect", clip);
            }
        }

        private void OnCardSuperEvolved(Card card, bool isPlayer)
        {
            if (card != null && card.uid == bcard.GetCardUID())
            {
                AudioClip clip = card.CardData.exEvolutionAudio==null? AssetData.Get().evolveAudio : card.CardData.exEvolutionAudio;
                AudioTool.Get().PlaySFX("ability_effect", clip);
            }
        }
        
        private Transform GetFXSource(Card caster)
        {
            if (caster.CardData.IsBoardCard())
            {
                BoardCard bcard = BoardCard.Get(caster.uid);
                if (bcard != null)
                    return bcard.transform;
            }
            else
            {
                BoardSlotPlayer slot = BoardSlotPlayer.Get(caster.playerID);
                if (slot != null)
                    return slot.transform;
            }
            return null;
        }

      

        
   

      

    

       


      
    }
}