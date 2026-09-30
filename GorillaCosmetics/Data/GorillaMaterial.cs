using GorillaCosmetics.Utils;
using System;
using System.IO;
using UnityEngine;

namespace GorillaCosmetics.Data
{
    public class GorillaMaterial : IAsset, IDisposable
    {
        public string FileName { get; }
        public AssetBundle AssetBundle { get; }
        public CosmeticDescriptor Descriptor { get; }
        readonly Material material;

        public GorillaMaterial(string path)
        {
            FileName = path;
            (AssetBundle bundle, PackageJSON json) = PackageUtils.AssetBundleAndJSONFromPackage(path);
            AssetBundle = bundle;
            try
            {
                GameObject prefab = AssetBundle.LoadAsset<GameObject>("_Material");
                Renderer renderer = prefab == null ? null : prefab.GetComponent<Renderer>();
                if (renderer == null || renderer.sharedMaterial == null)
                    throw new InvalidDataException("missing _Material renderer");
                material = renderer.sharedMaterial;
                Descriptor = PackageUtils.ConvertJsonToDescriptor(json);
            }
            catch
            {
                AssetBundle.Unload(true);
                throw;
            }
        }

        public Material GetMaterial()
        {
            return UnityEngine.Object.Instantiate(material);
        }

        public GameObject GetPreviewOrb(Transform parent)
        {
            GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.GetComponent<Collider>().enabled = false;
            UnityEngine.Object.Destroy(orb.GetComponent<Collider>());
            orb.layer = parent.gameObject.layer;
            orb.transform.SetParent(parent, false);
            orb.transform.localPosition = Constants.PreviewOrbLocalPosition;
            orb.transform.localRotation = Quaternion.identity;
            orb.transform.localScale = Constants.PreviewOrbLocalScale;
            orb.GetComponent<Renderer>().sharedMaterial = material;
            return orb;
        }

        public void Dispose()
        {
            AssetBundle.Unload(true);
        }
    }
}
