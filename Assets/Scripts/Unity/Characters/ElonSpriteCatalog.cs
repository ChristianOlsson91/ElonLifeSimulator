using System.Collections.Generic;
using ElonLifeSim.Core.Content;
using UnityEngine;

namespace ElonLifeSim.Unity.Characters
{
    /// <summary>
    /// Loads Elon era sprites from Resources/Characters/Elon/.
    /// Magenta (#FF00FF) pixels become transparent for in-world sprites.
    /// Authored walk_00..walk_03 play in file order; facing is SpriteRenderer.flipX.
    /// DistinctSameFacing / SynthesizeSnesWalk are fallback only when an era has
    /// no real cycle (0–1 unique frames).
    /// </summary>
    public static class ElonSpriteCatalog
    {
        public const string ResourcesRoot = ElonEraResolver.ResourcesRoot;
        public const float PixelsPerUnit = 64f;

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
        /// Walk frames in play order. If authored walk_00..walk_03 (or more) loaded
        /// successfully — 3+ frames, or 2+ that are not idle duplicates — they play
        /// in file order. Opposite-leg strides are kept even when they look like a
        /// horizontal mirror of idle (flipX still handles facing).
        /// DistinctSameFacing / SynthesizeSnesWalk run only when an era has no real
        /// cycle (0–1 unique frames), so fake mirrored-idle sheets still collapse.
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
            if (HasAuthoredWalkCycle(loaded, idle))
                return loaded.ToArray();

            var playable = DistinctSameFacing(loaded, idle);
            if (playable.Count >= 2)
                return playable.ToArray();

            var source = idle != null ? idle : (playable.Count > 0 ? playable[0] : null);
            if (source != null)
                return SynthesizeSnesWalk(source);

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
            return SpriteFromKeyed(readable);
        }

        public static Sprite SpriteFromKeyed(Texture2D readable)
        {
            if (readable == null)
                return null;
            return Sprite.Create(
                readable,
                new Rect(0, 0, readable.width, readable.height),
                new Vector2(0.5f, 0f), // pivot feet
                PixelsPerUnit,
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

        /// <summary>
        /// True when walk_00.. loaded a real cycle: 3+ frames, or 2+ that are not
        /// idle copies. Opposite-leg poses that happen to mirror idle still count.
        /// </summary>
        private static bool HasAuthoredWalkCycle(List<Sprite> loaded, Sprite idle)
        {
            if (loaded == null || loaded.Count == 0)
                return false;
            if (loaded.Count >= 3)
                return true;

            int notIdleDup = 0;
            for (int i = 0; i < loaded.Count; i++)
            {
                var s = loaded[i];
                if (s == null)
                    continue;
                if (idle == null || !NearlySame(s, idle))
                    notIdleDup++;
            }

            return notIdleDup >= 2;
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

        /// <summary>
        /// 5-frame SNES stride from a still: contact, pass, plant, opposite pass, contact.
        /// Fallback only — never used when authored walk_00.. frames form a real cycle.
        /// </summary>
        private static Sprite[] SynthesizeSnesWalk(Sprite idle)
        {
            var src = idle.texture;
            if (src == null)
                return new[] { idle };

            if (!TryOpaqueBounds(src, out int minX, out int minY, out int maxX, out int maxY))
                return new[] { idle };

            int contentH = maxY - minY + 1;
            int contentW = maxX - minX + 1;
            int waist = minY + Mathf.Max(6, Mathf.RoundToInt(contentH * 0.42f));
            int stride = Mathf.Max(6, Mathf.RoundToInt(contentW * 0.10f));
            int bob = Mathf.Max(2, Mathf.RoundToInt(contentH * 0.018f));

            return new[]
            {
                idle,
                SpriteFromKeyed(ComposeStride(src, bodyDy: bob, legDx: stride, waistY: waist)),
                SpriteFromKeyed(ComposeStride(src, bodyDy: -bob, legDx: 0, waistY: waist)),
                SpriteFromKeyed(ComposeStride(src, bodyDy: bob, legDx: -stride, waistY: waist)),
                idle
            };
        }

        private static Texture2D ComposeStride(Texture2D src, int bodyDy, int legDx, int waistY)
        {
            int w = src.width;
            int h = src.height;
            var from = src.GetPixels32();
            var to = new Color32[from.Length];
            for (int i = 0; i < to.Length; i++)
                to[i] = new Color32(0, 0, 0, 0);

            for (int y = 0; y < h; y++)
            {
                int sdx = y <= waistY ? legDx : 0;
                int sdy = bodyDy;
                for (int x = 0; x < w; x++)
                {
                    var c = from[x + y * w];
                    if (c.a == 0)
                        continue;
                    int nx = x + sdx;
                    int ny = y + sdy;
                    if (nx < 0 || nx >= w || ny < 0 || ny >= h)
                        continue;
                    to[nx + ny * w] = c;
                }
            }

            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.SetPixels32(to);
            tex.Apply(updateMipmaps: false, makeNoLongerReadable: false);
            return tex;
        }

        private static bool TryOpaqueBounds(Texture2D tex, out int minX, out int minY, out int maxX, out int maxY)
        {
            minX = tex.width;
            minY = tex.height;
            maxX = -1;
            maxY = -1;
            var p = tex.GetPixels32();
            int w = tex.width;
            int h = tex.height;
            for (int y = 0; y < h; y++)
            {
                int row = y * w;
                for (int x = 0; x < w; x++)
                {
                    if (p[row + x].a <= 16)
                        continue;
                    if (x < minX) minX = x;
                    if (y < minY) minY = y;
                    if (x > maxX) maxX = x;
                    if (y > maxY) maxY = y;
                }
            }

            return maxX >= 0;
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
