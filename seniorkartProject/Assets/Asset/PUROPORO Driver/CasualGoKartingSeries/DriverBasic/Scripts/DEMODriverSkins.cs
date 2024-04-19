using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PUROPORO
{
    public class DEMODriverSkins : MonoBehaviour
    {
        public SkinnedMeshRenderer[] skinnedMeshRenderers;
        public DriverSkin[] skins;
        private int m_CurrentSkin;
        private int m_CurrentTexture;

        private void OnEnable()
        {
            UIButton.OnClick += Change;

            ExecuteChange();
        }

        private void OnDisable()
        {
            UIButton.OnClick -= Change;
        }

        public void Change(UIButtonAction ButtonAction)
        {
            if (ButtonAction.actionName == "ChangeColor")
            {
                m_CurrentTexture++;

                if (m_CurrentTexture >= skins[m_CurrentSkin].textures.Length)
                    m_CurrentTexture = 0;

                ExecuteChange();
            }

            if (ButtonAction.actionName == "ChangeShader")
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

            for (int i = 0; i < skinnedMeshRenderers.Length; i++)
                skinnedMeshRenderers[i].material = tempMaterial;
        }
    }

    [System.Serializable]
    public class DriverSkin
    {
        public string skinName;
        public Material material;
        public string baseMapName = "_MainTex";   // or _BaseMap
        public Texture[] textures;
    }
}
