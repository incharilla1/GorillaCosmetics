using GorillaCosmetics.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace GorillaCosmetics
{
    public class AssetLoader : IAssetLoader, IDisposable
    {
        readonly Dictionary<Type, List<IAsset>> assets = new Dictionary<Type, List<IAsset>>();

        public AssetLoader()
        {
            string folder = Path.GetDirectoryName(typeof(Plugin).Assembly.Location);
            assets.Add(typeof(GorillaMaterial), LoadAssets(Path.Combine(folder, "Materials"), new[] { "*.material", "*.gmat" }, path => new GorillaMaterial(path)));
            assets.Add(typeof(GorillaHat), LoadAssets(Path.Combine(folder, "Hats"), new[] { "*.hat", "*.ghat" }, path => new GorillaHat(path)));
        }

        public T GetAsset<T>(string name) where T : IAsset
        {
            if (string.IsNullOrWhiteSpace(name))
                return default;
            if (assets.TryGetValue(typeof(T), out List<IAsset> list))
            {
                foreach (IAsset asset in list)
                {
                    if (string.Equals(asset.Descriptor.Name.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase))
                        return (T)asset;
                }
            }
            return default;
        }

        public List<T> GetAssets<T>() where T : IAsset
        {
            return assets.TryGetValue(typeof(T), out List<IAsset> list) ? list.Cast<T>().ToList() : new List<T>();
        }

        static List<IAsset> LoadAssets(string path, string[] filters, Func<string, IAsset> load)
        {
            List<IAsset> list = new List<IAsset>();
            try
            {
                Directory.CreateDirectory(path);
                foreach (string file in filters.SelectMany(filter => Directory.GetFiles(path, filter)).Distinct().OrderBy(file => file, StringComparer.OrdinalIgnoreCase))
                {
                    try
                    {
                        list.Add(load(file));
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"Could not load cosmetic {file}: {e}");
                    }
                }
            }
            catch (IOException e)
            {
                Debug.LogWarning($"Could not read cosmetics in {path}: {e}");
            }
            catch (UnauthorizedAccessException e)
            {
                Debug.LogWarning($"Could not read cosmetics in {path}: {e}");
            }
            return list;
        }

        public void Dispose()
        {
            foreach (List<IAsset> list in assets.Values)
            {
                foreach (IDisposable asset in list.Cast<IDisposable>())
                    asset.Dispose();
            }
            assets.Clear();
        }
    }
}
