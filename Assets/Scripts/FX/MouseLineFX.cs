using System.Collections.Generic;
using GameClient;
using GameLogic;
using UnityEngine;

namespace FX
{
    /// <summary>
    /// Line FX that appear when dragin a board card to attack
    /// </summary>
    public class MouseLineFX: MonoBehaviour
    {
        public GameObject dotTemplate;
        public float dotSpacing = 0.2f;

        private List<GameObject> dotList = new List<GameObject>();
        private List<Vector3> points = new List<Vector3>();
        
        void Start()
        {
            dotTemplate.SetActive(false);
        }
        
        void Update()
        {
            if (!Gameclient.Get().IsReady())
                return;

            RefreshLine();
            RefreshRender();
        }
        
        private void RefreshLine()
        {
            points.Clear();

            Game gdata = Gameclient.Get().GetGameData();
            PlayerControls controls = PlayerControls.Get();
            BoardCard bcard = controls.GetSelected();
            BoardEvolutionary boardEvolutionary = controls.GetSelectedEvolutionary();

            bool visible = false;
            Vector3 source = Vector3.zero;
            if (bcard != null)
            {
                source = bcard.transform.position;
                visible = true;
            }

            if (boardEvolutionary != null)
            {
                Debug.Log($"Selected Evolutionary {boardEvolutionary.type.ToString()}");
                source = boardEvolutionary.transform.position;
                visible = true;
            }

            HandCard drag = HandCard.GetDrag();
            if (drag != null)
            {
                source = drag.transform.position;
                visible = drag.GetCardData().IsRequireTarget();
            }

            if (gdata.selector == SelectorType.SelectTarget && gdata.selectorPlayerId == Gameclient.Get().GetPlayerID())
            {
                BoardCard caster = BoardCard.Get(gdata.selectorCasterUid);
                if (caster != null)
                {
                    source = caster.transform.position;
                    visible = true;
                }
            }

            if (visible)
            {
                Vector3 dest = GameBoard.Get().RaycastMouseBoard();
                Vector3 dir = (dest - source).normalized;
                float dist = (dest - source).magnitude;

                float value = 0f;
                while (value < dist)
                {
                    Vector3 pos = source + dir * value;
                    points.Add(pos);

                    value += dotSpacing;
                }
            }
        }
        
        private void RefreshRender()
        {
            while (dotList.Count < points.Count)
            {
                AddDot();
            }

            int index = 0;
            foreach (GameObject dot in dotList)
            {
                bool active = false;
                if (index < points.Count)
                {
                    Vector3 pos = points[index];
                    dot.transform.position = pos;
                    active = true;
                }

                if (dot.activeSelf != active)
                    dot.SetActive(active);

                index++;
            }
        }
        
        public void AddDot()
        {
            GameObject dot = Instantiate(dotTemplate, transform);
            dot.SetActive(true);
            dotList.Add(dot);
        }
    }
}