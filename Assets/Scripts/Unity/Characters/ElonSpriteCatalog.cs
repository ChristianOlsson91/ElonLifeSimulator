using System.Collections.Generic;
using ElonLifeSim.Core.Content;
using UnityEngine;

namespace ElonLifeSim.Unity.Characters
{
    /// <summary>
    /// Loads Elon era sprites from Resources/Characters/Elon/.
    /// Magenta (#FF00FF) pixels become transparent for in-world sprites.
    /// </summary>
    public static class ElonSpriteCatalog
    {
        public const string ResourcesRoot = "Characters/Elon";

        /// <summary>Era folder names under Resources/Characters/Elon/.</summary>
        public static string EraFolderForLocation(string locationId)
        {
            if (locationId == PrototypeContent.LocationPretoria)
                return "01_young_sa";
            if (locationId == PrototypeContent.LocationToronto)
                return "02_young_adult_90s";
            if (locationId == PrototypeContent.LocationPaloAlto)
                return "03_early_2000s"; // Zip2 / X.com era look
            return "04_modern";
        }

        public static string PrefixForEra(string eraFolder)
        {
            switch (eraFolder)
            {
                case "01_young_sa": return "elon_young_sa";
                case "02_young_adult_90s": return "elon_young_adult";
                case "03_early_2000s": return "elon_early2000s";
                case "04_modern": return "elon_modern";
                case "05_mars": return "elon_mars";
                default: return "elon_modern";
            }
        }

        public static Sprite LoadIdle(string locationId)
        {
            var era = EraFolderForLocation(locationId);
            var prefix = PrefixForEra(era);
            return LoadSprite($"{ResourcesRoot}/{era}/{prefix}_idle");
        }

        public static Sprite LoadPortrait(string locationId)
        {
            var era = EraFolderForLocation(locationId);
            var prefix = PrefixForEra(era);
            return LoadSprite($"{ResourcesRoot}/{era}/{prefix}_portrait");
        }

        /// <summary>Walk frames in play order (0..n-1). Falls back to idle only if missing.</summary>
        public static Sprite[] LoadWalkCycle(string locationId)
        {
            var era = EraFolderForLocation(locationId);
            var prefix = PrefixForEra(era);
            var list = new List<Sprite>();
            for (int i = 0; i <= 4; i++)
            {
                var s = LoadSprite($"{ResourcesRoot}/{era}/walk/{prefix}_walk_0{i}");
                if (s != null)
                    list.Add(s);
            }

            if (list.Count == 0)
            {
                var idle = LoadIdle(locationId);
                if (idle != null)
                    list.Add(idle);
            }

            return list.ToArray();
        }

        public static Sprite LoadSprite(string resourcesPath)
        {
            // Prefer Texture2D; fall back if Unity imported as Sprite.
            var tex = Resources.Load<Texture2D>(resourcesPath);
            if (tex == null)
            {
                var existing = Resources.Load<Sprite>(resourcesPath);
                if (existing != null)
                {
                    tex = existing.texture;
                }
            }

            if (tex == null)
            {
                Debug.LogWarning($"[ElonSpriteCatalog] Missing Resources texture: {resourcesPath}");
                return null;
            }

            var readable = MakeReadableMagentaKeyed(tex);
            return Sprite.Create(
                readable,
                new Rect(0, 0, readable.width, readable.height),
                new Vector2(0.5f, 0f), // pivot feet
                pixelsPerUnit: 64f,
                extrude: 0,
                meshType: SpriteMeshType.FullRect);
        }

        /// <summary>
        /// Copies texture, converts near-magenta and near-black to alpha, Point filter.
        /// </summary>
        public static Texture2D MakeReadableMagentaKeyed(Texture2D source)
        {
            var tmp = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            tmp.filterMode = FilterMode.Point;
            tmp.wrapMode = TextureWrapMode.Clamp;

            // Blit via RenderTexture so we can read even if source is not readable.
            var rt = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(source, rt);
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            tmp.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
            tmp.Apply();
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);

            var pixels = tmp.GetPixels32();
            for (int i = 0; i < pixels.Length; i++)
            {
                var c = pixels[i];
                // Magenta key
                if (c.r > 200 && c.g < 40 && c.b > 200)
                {
                    pixels[i] = new Color32(0, 0, 0, 0);
                    continue;
                }
                // Near-black leftover key
                if (c.r < 12 && c.g < 12 && c.b < 12)
                {
                    pixels[i] = new Color32(0, 0, 0, 0);
                }
            }
            tmp.SetPixels32(pixels);
            tmp.Apply(updateMipmaps: false, makeNoLongerReadable: false);
            return tmp;
        }
    }
}

