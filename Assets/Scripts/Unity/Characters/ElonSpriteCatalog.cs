using System.Collections.Generic;
using ElonLifeSim.Core.Content;
using UnityEngine;

namespace ElonLifeSim.Unity.Characters
{
    /// <summary>
    /// Loads Elon era sprites from Resources/Characters/Elon/.
    /// Magenta (#FF00FF) pixels become transparent for in-world sprites.
    /// Walk cycles play distinct same-facing frames; facing is SpriteRenderer.flipX.
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

        /// <summary>
        /// Walk frames in play order. Real stride poses are kept; idle copies and
        /// horizontal flips are not treated as animation (flipX handles facing).
        /// When two or more distinct same-facing frames exist they play as-is —
        /// no synthesized fake stride.
        /// </summary>
        public static Sprite[] LoadWalkCycle(string locationId, string actId = null)
        {
            var loaded = new List<Sprite>();
            for (int i = 0; i <= 4; i++)
            {
                var s = LoadSprite(ElonEraResolver.WalkResourceKey(locationId, i, actId));
                if (s != null)
                    loaded.Add(s);
            }

            var idle = LoadIdle(locationId, actId);
            var playable = DistinctSameFacing(loaded, idle);
            if (playable.Count >= 2)
                return playable.ToArray();

            if (playable.Count == 1)
                return playable.ToArray();

            if (idle != null)
                return new[] { idle };

            return playable.Count > 0 ? playable.ToArray() : System.Array.Empty<Sprite>();
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

        internal static bool NearlySame(Sprite a, Sprite b, float tolerance = 0.012f)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a == null || b == null || a.texture == null || b.texture == null)
                return false;
            var ta = a.texture;
            var tb = b.texture;
            if (ta.width != tb.width || ta.height != tb.height)
                return false;
            return PixelMismatch(ta.GetPixels32(), tb.GetPixels32(), ta.width, ta.height, mirror: false) <= tolerance;
        }

        internal static bool IsHorizontalMirror(Sprite a, Sprite b, float tolerance = 0.03f)
        {
            if (a == null || b == null || a.texture == null || b.texture == null)
                return false;
            var ta = a.texture;
            var tb = b.texture;
            if (ta.width != tb.width || ta.height != tb.height)
                return false;
            return PixelMismatch(ta.GetPixels32(), tb.GetPixels32(), ta.width, ta.height, mirror: true) <= tolerance;
        }

        private static List<Sprite> DistinctSameFacing(List<Sprite> frames, Sprite idle)
        {
            var unique = new List<Sprite>();
            var face = idle;
            for (int i = 0; i < frames.Count; i++)
            {
                var s = frames[i];
                if (s == null)
                    continue;
                if (face != null && IsHorizontalMirror(face, s))
                    continue;
                bool dup = false;
                for (int u = 0; u < unique.Count; u++)
                {
                    if (NearlySame(unique[u], s))
                    {
                        dup = true;
                        break;
                    }
                }

                if (!dup)
                    unique.Add(s);
                if (face == null)
                    face = s;
            }

            return unique;
        }

        private static float PixelMismatch(Color32[] a, Color32[] b, int w, int h, bool mirror)
        {
            int considered = 0;
            int differ = 0;
            for (int y = 0; y < h; y++)
            {
                int row = y * w;
                for (int x = 0; x < w; x++)
                {
                    var ca = a[row + x];
                    var cb = b[row + (mirror ? w - 1 - x : x)];
                    if (ca.a <= 8 && cb.a <= 8)
                        continue;
                    considered++;
                    if (ca.a != cb.a || Abs(ca.r - cb.r) > 8 || Abs(ca.g - cb.g) > 8 || Abs(ca.b - cb.b) > 8)
                        differ++;
                }
            }

            if (considered == 0)
                return 0f;
            return differ / (float)considered;
        }

        private static int Abs(int v) => v < 0 ? -v : v;
    }
}
