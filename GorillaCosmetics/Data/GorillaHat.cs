using GorillaCosmetics.Utils;
using System;
using System.IO;
using UnityEngine;

namespace GorillaCosmetics.Data
{
    public class GorillaHat : IAsset, IDisposable
    {
        public string FileName { get; }
        public CosmeticDescriptor Descriptor { get; }
        readonly AssetBundle assetBundle;
        readonly GameObject assetTemplate;

        public GorillaHat(string path)
        {
            FileName = path;
            (AssetBundle bundle, PackageJSON json) = PackageUtils.AssetBundleAndJSONFromPackage(path);
            assetBundle = bundle;
            try
            {
                assetTemplate = assetBundle.LoadAsset<GameObject>("_Hat");
                if (assetTemplate == null)
                    throw new InvalidDataException("missing _Hat prefab");
                assetTemplate.SetActive(false);
                foreach (Collider collider in assetTemplate.GetComponentsInChildren<Collider>(true))
                    collider.enabled = false;
                foreach (Rigidbody body in assetTemplate.GetComponentsInChildren<Rigidbody>(true))
                {
                    body.isKinematic = true;
                    body.detectCollisions = false;
                }
                Descriptor = PackageUtils.ConvertJsonToDescriptor(json);
            }
            catch
            {
                assetBundle.Unload(true);
                throw;
            }
        }

        public GameObject GetAsset()
        {
            GameObject hat = UnityEngine.Object.Instantiate(assetTemplate);
            hat.SetActive(true);
            return hat;
        }

        public GameObject GetCleanAsset()
        {
            GameObject hat = UnityEngine.Object.Instantiate(assetTemplate);
            foreach (Behaviour component in hat.GetComponentsInChildren<Behaviour>(true))
            {
                if (component is Light || component is Camera || component is AudioSource)
                    component.enabled = false;
            }
            foreach (ParticleSystem particles in hat.GetComponentsInChildren<ParticleSystem>(true))
            {
                ParticleSystem.MainModule main = particles.main;
                main.playOnAwake = false;
                particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            hat.SetActive(true);
            return hat;
        }

        public void Dispose()
        {
            assetBundle.Unload(true);
        }
    }
}
