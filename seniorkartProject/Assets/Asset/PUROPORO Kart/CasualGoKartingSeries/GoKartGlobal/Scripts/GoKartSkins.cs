using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PUROPORO
{
    public class GoKartSkins : MonoBehaviour
    {
        public SkinnedMeshRenderer[] skinnedMeshRenderers;
        public GoKartSkin[] skins;
        public GoKartSkin[] wheelSkins;
        private int m_CurrentSkin;
        private int m_CurrentTexture;
        private int m_CurrentWheelTexture;

        private void OnEnable()
        {
            UIButton.OnClick += Change;

            ExecuteChange();
        }

        private void OnDisable()
        {
            UIButton.OnClick -= Change;
        }

        public void Change(UIButtonAction buttonAction)
        {
            if (buttonAction.actionName == "ChangeColor")
            {
                m_CurrentTexture++;

                if (m_CurrentTexture >= skins[m_CurrentSkin].textures.Length)
                    m_CurrentTexture = 0;

                ExecuteChange();
            }

            if (buttonAction.actionName == "ChangeWheels")
            {
                m_CurrentWheelTexture++;

                if (m_CurrentWheelTexture >= wheelSkins[m_CurrentSkin].textures.Length)
                    m_CurrentWheelTexture = 0;

                ExecuteChange();
            }

            if (buttonAction.actionName == "ChangeShader")
            {
                m_CurrentSkin++;

                if (m_CurrentSkin >= skins.Length)
                    m_CurrentSkin = 0;

                ExecuteChange();
            }
        }

        private void ExecuteChange()
        {
            Material tempMaterial = skins[m_CurrentSkin].material;
            tempMaterial.SetTexture(skins[m_CurrentSkin].baseMapName, skins[m_CurrentSkin].textures[m_CurrentTexture]);

            Material tempWheelMaterial = wheelSkins[m_CurrentSkin].material;
            tempWheelMaterial.SetTexture(wheelSkins[m_CurrentSkin].baseMapName, wheelSkins[m_CurrentSkin].textures[m_CurrentWheelTexture]);

            Material[] mats = new Material[] { tempMaterial, tempWheelMaterial };

            for (int i = 0; i < skinnedMeshRenderers.Length; i++)
                skinnedMeshRenderers[i].materials = mats;
        }
    }

    [System.Serializable]
    public class GoKartSkin
    {
        public string skinName;
        public Material material;
        public string baseMapName = "_MainTex";   // or _BaseMap
        public Texture[] textures;
    }
}
