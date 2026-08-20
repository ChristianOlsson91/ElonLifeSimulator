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
        public const string ResourcesRoot = ElonEraResolver.ResourcesRoot;

        /// <summary>Era folder names under Resources/Characters/Elon/. Delegates to Core.</summary>
        public static string EraFolderForLocation(string locationId, string actId = null)
        {
            return ElonEraResolver.EraFolderForLocation(locationId, actId);
        }

        public static string PrefixForEra(string eraFolder)
        {
            return ElonEraResolver.PrefixForEra(eraFolder);
        }

        public static Sprite LoadIdle(string locationId, string actId = null)
        {
            return LoadSprite(ElonEraResolver.IdleResourceKey(locationId, actId));
        }

        public static Sprite LoadPortrait(string locationId, string actId = null)
        {
            return LoadSprite(ElonEraResolver.PortraitResourceKey(locationId, actId));
        }

        /// <summary>Walk frames in play order (0..n-1). Falls back to idle only if missing.</summary>
        public static Sprite[] LoadWalkCycle(string locationId, string actId = null)
        {
            var list = new List<Sprite>();
            for (int i = 0; i <= 4; i++)
            {
                var s = LoadSprite(ElonEraResolver.WalkResourceKey(locationId, i, actId));
                if (s != null)
                    list.Add(s);
            }

            if (list.Count == 0)
            {
                var idle = LoadIdle(locationId, actId);
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
        /// Copies texture, converts near-magenta (#FF00FF) to alpha, Point filter.
        /// Does not key black or dark clothing/hair/shoes.
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
                if (c.r > 200 && c.g < 40 && c.b > 200)
                    pixels[i] = new Color32(0, 0, 0, 0);
            }
            tmp.SetPixels32(pixels);
            tmp.Apply(updateMipmaps: false, makeNoLongerReadable: false);
            return tmp;
        }
    }
}

