using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PokeBloobs.Classes
{
    public static class ZipAssetLoader
    {
        private static readonly Dictionary<string, byte[]> _byteCache = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
        private static string ZipPath =>
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "PokeBloobs.assets.zip");

        private static Dictionary<string, string> _entryLookup;

        public static bool ZipExists()
        {
            return File.Exists(ZipPath);
        }

        public static void BuildIndex()
        {
            _entryLookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (!File.Exists(ZipPath))
            {
                Debug.LogWarning($"[PokeBloobs] Asset zip not found at: {ZipPath}");
                return;
            }

            using (ZipArchive zip = ZipFile.OpenRead(ZipPath))
            {
                foreach (var entry in zip.Entries)
                {
                    _entryLookup[entry.FullName] = entry.FullName;
                }
            }

            Debug.Log($"[PokeBloobs] Indexed {_entryLookup.Count} zip entries.");
        }

        public static string FindAssetPath(string folderHint, string assetName, string extension)
        {
            if (_entryLookup == null)
                BuildIndex();

            string ext = "." + extension.TrimStart('.');

            return _entryLookup.Keys.FirstOrDefault(path =>
                path.IndexOf(folderHint, StringComparison.OrdinalIgnoreCase) >= 0 &&
                Path.GetFileNameWithoutExtension(path).Equals(assetName, StringComparison.OrdinalIgnoreCase) &&
                path.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
        }

        public static byte[] LoadAssetBytes(string relativePath)
        {
            string normalizedPath = relativePath.Replace("\\", "/");

            if (_byteCache.TryGetValue(normalizedPath, out var cached))
                return cached;

            if (!File.Exists(ZipPath))
                return null;

            using (ZipArchive zip = ZipFile.OpenRead(ZipPath))
            {
                ZipArchiveEntry entry = zip.Entries.FirstOrDefault(e =>
                    string.Equals(e.FullName, normalizedPath, StringComparison.OrdinalIgnoreCase));

                if (entry == null)
                    return null;

                using (Stream stream = entry.Open())
                using (MemoryStream ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    byte[] bytes = ms.ToArray();
                    _byteCache[normalizedPath] = bytes;
                    return bytes;
                }
            }
        }

        public static Stream OpenAssetStream(string folderHint, string assetName, string extension)
        {
            string assetPath = FindAssetPath(folderHint, assetName, extension);
            if (assetPath == null)
                return null;

            byte[] bytes = LoadAssetBytes(assetPath);
            if (bytes == null)
                return null;

            return new MemoryStream(bytes);
        }
    }
}
