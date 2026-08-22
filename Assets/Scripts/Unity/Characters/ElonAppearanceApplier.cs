using ElonLifeSim.Core.Content;
using ElonLifeSim.Unity.Controllers;
using ElonLifeSim.Unity.UI;
using UnityEngine;

namespace ElonLifeSim.Unity.Characters
{
    /// <summary>
    /// Single refresh entry: load idle + walk (+ dialogue portrait) for a location
    /// onto the one <c>Player_Elon</c>. Creates the player if missing; never duplicates it.
    /// </summary>
    public static class ElonAppearanceApplier
    {
        public const string PlayerName = "Player_Elon";
        public const string PlaceholderName = "Player_YoungElon_PLACEHOLDER";
        public const float TargetWorldHeight = 1.85f;

        /// <param name="actId">Reserved act hook; location mapping is current truth.</param>
        public static GameObject Apply(string locationId, string actId = null)
        {
            if (string.IsNullOrEmpty(locationId))
                locationId = PrototypeContent.LocationPretoria;

            DestroyPlaceholder();
            var player = FindOrCreateSinglePlayer();
            var controller = player.GetComponent<PixelPlayerController>();
            if (controller == null)
                controller = player.AddComponent<PixelPlayerController>();

            controller.ApplyLocation(locationId, actId);
            FitHeight(player);
            TuneRenderer(player);

            var dialogues = Object.FindObjectsByType<DialogueUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < dialogues.Length; i++)
                dialogues[i].RefreshPortrait();

            Debug.Log($"[Elon] Appearance applied for location '{locationId}' era '{ElonEraResolver.EraFolderForLocation(locationId, actId)}'.");
            return player;
        }

        private static void DestroyPlaceholder()
        {
            var leftover = GameObject.Find(PlaceholderName);
            if (leftover != null)
                Object.DestroyImmediate(leftover);
        }

        private static GameObject FindOrCreateSinglePlayer()
        {
            GameObject keep = null;
            var controllers = Object.FindObjectsByType<PixelPlayerController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < controllers.Length; i++)
            {
                var go = controllers[i].gameObject;
                if (go.name != PlayerName)
                    continue;
                if (keep == null)
                    keep = go;
                else
                    Object.DestroyImmediate(go);
            }

            if (keep != null)
                return keep;

            var named = GameObject.Find(PlayerName);
            if (named != null)
                return named;

            return CreatePlayer();
        }

        private static GameObject CreatePlayer()
        {
            var player = new GameObject(PlayerName);
            player.transform.position = Vector3.zero;
            var sr = player.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 20;
            sr.color = Color.white;

            var rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.None;

            player.AddComponent<PixelPlayerController>();

            var col = player.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.55f, 0.95f);
            col.offset = new Vector2(0f, 0.48f);
            return player;
        }

        private static void TuneRenderer(GameObject player)
        {
            var sr = player.GetComponent<SpriteRenderer>();
            if (sr == null)
                return;
            sr.sortingOrder = 20;
            sr.color = Color.white;
            sr.flipY = false;
        }

        private static void FitHeight(GameObject player)
        {
            var sr = player.GetComponent<SpriteRenderer>();
            if (sr == null || sr.sprite == null)
                return;
            float h = ElonSpriteCatalog.TightWorldHeight(sr.sprite);
            if (h < 0.01f)
                h = sr.sprite.bounds.size.y;
            if (h > 0.01f)
                player.transform.localScale = Vector3.one * (TargetWorldHeight / h);
        }
    }
}
