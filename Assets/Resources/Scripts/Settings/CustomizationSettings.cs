using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Platinum.Settings
{
    [CreateAssetMenu(menuName = "CustomizationSettings")]
    public class CustomizationSettings : ScriptableObject
    {
        public Mesh[] FreeCharacterModels;
        public AvatarSkin[] FreeCharacterMaterials;
        public SkinType SkinType { get; private set; } = SkinType.Free;

        public int CurrentIndexFreeModel { get; private set; } = 0;
        public int CurrentIndexFreeMaterial { get; private set; } = 0;

        private int IndexFreeModel = 0;
        private int IndexFreeMaterial = 0;

        private List<Mesh> AllModels;
        private List<AvatarSkin> AllMaterials;

        public Mesh GetRandomModel()
        {
            return FreeCharacterModels[UnityEngine.Random.Range(0, FreeCharacterModels.Length)];
        }
        public AvatarSkin GetRandomSkin()
        {
            return FreeCharacterMaterials[UnityEngine.Random.Range(0, FreeCharacterMaterials.Length)];
        }

        public Mesh GetModel(SkinType type, int index)
        {
            switch (type)
            {
                case (SkinType.NFT):
                    return FreeCharacterModels[index];
                default:
                    return FreeCharacterModels[index];
            }
            return FreeCharacterModels[index];
        }
        public AvatarSkin GetSkin(SkinType type, int index)
        {
            switch (type)
            {
                case SkinType.NFT:
                    return FreeCharacterMaterials[index];
                default:
                    return FreeCharacterMaterials[index];
            }
            return FreeCharacterMaterials[IndexFreeMaterial];
        }

        public Mesh GetCurrentModel()
        {
            switch (SkinType)
            {
                case SkinType.NFT:
                    return FreeCharacterModels[IndexFreeModel];
                default:
                    return FreeCharacterModels[IndexFreeModel];
            }
            return FreeCharacterModels[IndexFreeModel];
        }

        public AvatarSkin GetCurrentSkin()
        {
            switch (SkinType)
            {
                case SkinType.NFT:
                    return FreeCharacterMaterials[IndexFreeMaterial];
                default:
                    return FreeCharacterMaterials[IndexFreeMaterial];
            }
            return FreeCharacterMaterials[IndexFreeMaterial];
        }

        public bool CheckIndexModel()
        {
            switch (SkinType)
            {
                case SkinType.NFT:
                    return CurrentIndexFreeModel == IndexFreeModel;
                default:
                    return CurrentIndexFreeModel == IndexFreeModel;
            }
            return CurrentIndexFreeModel == IndexFreeModel;
        }

        public bool CheckIndexMaterial()
        {
            switch (SkinType)
            {
                case SkinType.NFT:
                    return CurrentIndexFreeMaterial == IndexFreeMaterial;
                default:
                    return CurrentIndexFreeMaterial == IndexFreeMaterial;
            }
            return CurrentIndexFreeMaterial == IndexFreeMaterial;
        }

        public int GetCurrentIndexModel()
        {
            switch (SkinType)
            {
                case SkinType.NFT:
                    return CurrentIndexFreeModel;
                default:
                    return CurrentIndexFreeModel;
            }
            return CurrentIndexFreeModel;
        }

        public int GetCurrentIndexSkin()
        {
            switch (SkinType)
            {
                case SkinType.NFT:
                    return CurrentIndexFreeMaterial;
                default:
                    return CurrentIndexFreeMaterial;
            }
            return CurrentIndexFreeMaterial;
        }


        public void SetCharacterType(SkinType newType)
        {
            SkinType = newType;
        }

        public void SetCurrentIndexModel(int index)
        {
            switch (SkinType)
            {
                case SkinType.NFT:
                    CurrentIndexFreeModel = index;
                    break;
                default:
                    CurrentIndexFreeModel = index;
                    break;
            }
        }

        public void SetCurrentIndexSkin(int index)
        {
            switch (SkinType)
            {
                case SkinType.NFT:
                    CurrentIndexFreeMaterial = index;
                    break;
                default:
                    CurrentIndexFreeMaterial = index;
                    break;
            }
        }

        public void SetCurrentIndexModel()
        {
            switch (SkinType)
            {
                case SkinType.NFT:
                    CurrentIndexFreeModel = IndexFreeModel;
                    break;
                default:
                    CurrentIndexFreeModel = IndexFreeModel;
                    break;
            }
        }

        public void SetCurrentIndexSkin()
        {
            switch (SkinType)
            {
                case SkinType.NFT:
                    CurrentIndexFreeMaterial = IndexFreeMaterial;
                    break;
                default:
                    CurrentIndexFreeMaterial = IndexFreeMaterial;
                    break;
            }
        }

        public Mesh LeftModel()
        {
            switch (SkinType)
            {
                case SkinType.NFT:
                    IndexFreeModel--;
                    if (IndexFreeModel < 0)
                        IndexFreeModel = FreeCharacterModels.Length - 1;
                    break;
                default:
                    IndexFreeModel--;
                    if (IndexFreeModel < 0)
                        IndexFreeModel = FreeCharacterModels.Length - 1;
                    break;
            }
            return FreeCharacterModels[IndexFreeModel];
        }
        public Mesh RightModel()
        {
            switch (SkinType)
            {
                case SkinType.NFT:
                    IndexFreeModel++;
                    if (IndexFreeModel >= FreeCharacterModels.Length)
                        IndexFreeModel = 0;
                    break;
                default:
                    IndexFreeModel++;
                    if (IndexFreeModel >= FreeCharacterModels.Length)
                        IndexFreeModel = 0;
                    break;
            }
            return FreeCharacterModels[IndexFreeModel];
        }

        public AvatarSkin LeftSkin()
        {
            switch (SkinType)
            {
                case SkinType.NFT:
                    IndexFreeMaterial--;
                    if (IndexFreeMaterial < 0)
                        IndexFreeMaterial = FreeCharacterMaterials.Length - 1;
                    break;
                default:
                    IndexFreeMaterial--;
                    if (IndexFreeMaterial < 0)
                        IndexFreeMaterial = FreeCharacterMaterials.Length - 1;
                    break;
            }
            return FreeCharacterMaterials[IndexFreeMaterial];
        }
        public AvatarSkin RightSkin()
        {
            switch (SkinType)
            {
                case SkinType.NFT:
                    IndexFreeMaterial++;
                    if (IndexFreeMaterial >= FreeCharacterMaterials.Length)
                        IndexFreeMaterial = 0;
                    break;
                default:
                    IndexFreeMaterial++;
                    if (IndexFreeMaterial >= FreeCharacterMaterials.Length)
                        IndexFreeMaterial = 0;
                    break;
            }
            return FreeCharacterMaterials[IndexFreeMaterial];
        }
    }
    
    public enum SkinType
    {
        Free,
        NFT,
    }

    [Serializable]
    public class AvatarSkin
    {
        public string Name;
        public Material[] Materials;
    }

}