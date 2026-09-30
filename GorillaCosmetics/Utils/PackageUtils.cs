using GorillaCosmetics.Data;
using Newtonsoft.Json;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GorillaCosmetics.Utils
{
    public static class PackageUtils
    {
        public static (AssetBundle bundle, PackageJSON json) AssetBundleAndJSONFromPackage(string path)
        {
            using (ZipArchive archive = ZipFile.OpenRead(path))
            {
                ZipArchiveEntry jsonEntry = archive.Entries.FirstOrDefault(entry => entry.Name == "package.json");
                if (jsonEntry == null)
                    throw new InvalidDataException("missing package.json");
                PackageJSON json;
                using (StreamReader reader = new StreamReader(jsonEntry.Open(), Encoding.UTF8))
                    json = JsonConvert.DeserializeObject<PackageJSON>(reader.ReadToEnd());
                if (json?.descriptor == null || string.IsNullOrWhiteSpace(json.descriptor.objectName) || string.IsNullOrWhiteSpace(json.pcFileName))
                    throw new InvalidDataException("invalid package metadata");
                ZipArchiveEntry bundleEntry = archive.GetEntry(json.pcFileName) ?? archive.Entries.FirstOrDefault(entry => entry.Name == json.pcFileName);
                if (bundleEntry == null)
                    throw new InvalidDataException("missing pc asset bundle");
                using (Stream stream = bundleEntry.Open())
                using (MemoryStream data = new MemoryStream())
                {
                    stream.CopyTo(data);
                    AssetBundle bundle = AssetBundle.LoadFromMemory(data.ToArray());
                    if (bundle == null)
                        throw new InvalidDataException("could not load asset bundle");
                    return (bundle, json);
                }
            }
        }

        public static CosmeticDescriptor ConvertJsonToDescriptor(PackageJSON json)
        {
            return new CosmeticDescriptor
            {
                Name = json.descriptor.objectName.Trim(),
                AuthorName = json.descriptor.author ?? string.Empty,
                Description = json.descriptor.description ?? string.Empty,
                CustomColors = json.config?.customColors ?? false,
                DisablePublicLobbies = json.config?.disableInPublicLobbies ?? false
            };
        }
    }
}
