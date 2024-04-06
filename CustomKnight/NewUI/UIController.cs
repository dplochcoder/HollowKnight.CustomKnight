﻿using System.Linq;
using UnityEngine.UI;

namespace CustomKnight.NewUI
{
    public static class UIController
    {
        public static GameObject UI, content, viewport;
        public static ScrollRect scrollRect;

        public static Font arial;
        public static Font perpetua;
        public static Font trajanBold;
        public static Font trajanNormal;
        private static Coroutine inputCoroutine;
        private static bool isVisible;
        private static float lastScrollPosition;

        public static bool EnableListener { get; private set; }

        private static void LoadResources()
        {
            foreach (Font font in Resources.FindObjectsOfTypeAll<Font>())
            {
                if (font != null && font.name == "TrajanPro-Bold")
                {
                    trajanBold = font;
                }

                if (font != null && font.name == "TrajanPro-Regular")
                {
                    trajanNormal = font;
                }

                //Just in case for some reason the computer doesn't have arial
                if (font != null && font.name == "Perpetua")
                {
                    perpetua = font;
                }

                foreach (string fontName in Font.GetOSInstalledFontNames())
                {
                    if (fontName.ToLower().Contains("arial"))
                    {
                        arial = Font.CreateDynamicFontFromOSFont(fontName, 13);
                        break;
                    }
                }
            }

        }
        public static void StartKeybindListener()
        {
            inputCoroutine = CoroutineHelper.GetRunner().StartCoroutine(ListenForInput());
        }
        static UIController()
        {
            LoadResources();
        }
        private static void OnSceneChange(Scene prevScene, Scene nextScene)
        {
            if (nextScene.name == "Menu_Title")
            {
                hideMenu();
            }
        }
        private static void OnUnpause(On.HeroController.orig_UnPause orig, HeroController self)
        {
            hideMenu();
            orig(self);
        }
        private static void OnPause(On.HeroController.orig_Pause orig, HeroController self)
        {
            showMenu();
            orig(self);
        }
        public static void EnableMenu()
        {
            CustomKnight.GlobalSettings.EnablePauseMenu = true;
            if (UI == null)
            {
                GenerateMenu();
            }
            On.HeroController.Pause += OnPause;
            On.HeroController.UnPause += OnUnpause;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChange;
        }

        private static IEnumerator ListenForInput()
        {
            while (true)
            {
                if (GameManager.instance.isPaused)
                {
                    if (CustomKnight.GlobalSettings.Keybinds.OpenSkinList.WasPressed)
                    {
                        //EnableListener = false;
                        if (CustomKnight.GlobalSettings.EnablePauseMenu)
                        {
                            DisableMenu();
                            lastScrollPosition = scrollRect.verticalNormalizedPosition;
                        }
                        else
                        {
                            EnableMenu();
                            showMenu();
                            scrollRect.verticalNormalizedPosition = lastScrollPosition;
                        }
                    }
                }

                yield return new WaitForEndOfFrame();
            }

        }

        public static void DisableMenu()
        {
            CustomKnight.GlobalSettings.EnablePauseMenu = false;
            hideMenu();
            //CoroutineHelper.GetRunner().StopCoroutine(inputCoroutine);
            On.HeroController.Pause -= OnPause;
            On.HeroController.UnPause -= OnUnpause;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= OnSceneChange;
        }
        public static void GenerateMenu()
        {
            UI = new GameObject("UI Parent");
            var cv = UI.AddComponent<UnityEngine.Canvas>();
            cv.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = UI.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            UI.AddComponent<GraphicRaycaster>();
            var rt = UI.GetAddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(300f, 1080f);
            rt.pivot = new Vector2(1f, 0f);


            scrollRect = UI.AddComponent<ScrollRect>();


            viewport = new GameObject("Viewport");
            viewport.transform.SetParent(UI.transform, false);
            var viewrt = viewport.GetAddComponent<RectTransform>();
            viewrt.sizeDelta = new Vector2(300f, 1080f);
            viewrt.anchorMin = new Vector2(1f, 0f);
            viewrt.anchorMax = new Vector2(1f, 0f);
            viewrt.pivot = new Vector2(1f, 0f);
            var mask = viewport.AddComponent<Mask>();

            CreateUpdateGUI();

            scrollRect.viewport = viewrt;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.scrollSensitivity = 25f;
            scrollRect.verticalNormalizedPosition = 1f;

            GameObject.DontDestroyOnLoad(UI);
            hideMenu();
        }
        public static void hideMenu()
        {
            isVisible = false;
            content.GetAddComponent<VerticalLayoutGroup>().padding = new RectOffset() { top = 1000, bottom = 100 };
            UI.SetActive(false);
        }
        public static void showMenu()
        {
            if (!CustomKnight.GlobalSettings.EnablePauseMenu) { return; }
            isVisible = true;
            UI.SetActive(true);
        }

        public static Texture2D GetSkinIcon(ISelectableSkin skin)
        {
            var orbIcon = "orbicon.png";
            var orbFull = "OrbFull.png";
            var knight = "knight.png";
            if (skin.Exists(orbIcon))
            {
                return skin.GetTexture(orbIcon);
            }
            Texture2D defaultOrb = Texture2D.blackTexture;

            if (skin.Exists(knight))
            {
                var defaultSkin = SkinManager.GetDefaultSkin();
                if (skin.Exists(orbFull))
                {
                    defaultOrb = skin.GetTexture(orbFull);
                }
                else if (defaultSkin.Exists(orbFull))
                {
                    defaultOrb = defaultSkin.GetTexture(orbFull);
                }
                var tex = skin.GetTexture(knight).GetCropped(new Rect(2802f, 4096f - 3155f, 86f, 120f));
                if (defaultOrb != Texture2D.blackTexture)
                {
                    tex = SheetItem.Overlay(defaultOrb, tex, 50, 65);
                }
                DefaultSkin.Save(tex, skin.GetId(), orbIcon, true);
                return tex;
            }

            //should never happen but still
            return defaultOrb;
        }

        public static void CreateUpdateGUI()
        {
            if (UI == null)
            {
                return;
            }
            if (content != null)
            {
                GameObject.DestroyImmediate(content);
            }
            // create new content object

            content = new GameObject("content");
            var bg = content.GetAddComponent<Image>();
            bg.raycastTarget = true;
            bg.color = Color.clear;
            content.transform.SetParent(viewport.transform, false);
            var contentrt = content.GetAddComponent<RectTransform>();
            contentrt.anchorMin = Vector2.zero;
            contentrt.anchorMax = new Vector2(1f, 0f);
            contentrt.pivot = new Vector2(1f, 0.5f);
            var group = content.AddComponent<VerticalLayoutGroup>();
            content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            group.childForceExpandWidth = false;
            group.childAlignment = TextAnchor.MiddleCenter;
            group.padding = new RectOffset() { top = 1000, bottom = 100 };
            group.childControlHeight = false;

            scrollRect.content = contentrt;

            var title = new UIText(content, "Title", "Skin List");
            title.text.fontSize = 25;
            var favSkins = new List<string>();
            favSkins.AddRange(CustomKnight.GlobalSettings.FavoriteSkins);
            favSkins.AddRange(CustomKnight.GlobalSettings.RecentSkins);
            favSkins = favSkins.Distinct().ToList();
            for (var index = 0; favSkins.Count < CustomKnight.GlobalSettings.MaxSkinCache; index++)
            {
                var id = SkinManager.SkinsList[index].GetId();
                if (!favSkins.Contains(id))
                {
                    favSkins.Add(id);
                }
            }
            foreach (var skinId in favSkins)
            {
                var skin = SkinManager.GetSkinById(skinId);
                var tex = GetSkinIcon(skin);
                var skinName = skin.GetName();
                skinName = skinName.Length > CustomKnight.GlobalSettings.NameLength ? skinName.Substring(0, CustomKnight.GlobalSettings.NameLength) : skinName;

                if (tex != Texture2D.blackTexture)
                {
                    var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                    var btn = content.AddButton(skinName, sprite, (e) =>
                    {
                        CustomKnight.GlobalSettings.AddRecentSkin(skin.GetId());
                        UIController.CreateUpdateGUI();
                        CoroutineHelper.WaitForFramesBeforeInvoke(1, () =>
                        {
                            scrollRect.verticalNormalizedPosition = 1f;
                            SkinManager.SetSkinById(skin.GetId());
                        });
                    });
                }
                var txtbtn = content.AddButton($"Name_{skinName}", skinName, (e) =>
                {
                    CustomKnight.GlobalSettings.AddRecentSkin(skin.GetId());
                    UIController.CreateUpdateGUI();
                    CoroutineHelper.WaitForFramesBeforeInvoke(1, () =>
                    {
                        scrollRect.verticalNormalizedPosition = 1f;
                        SkinManager.SetSkinById(skin.GetId());
                    });
                });
            }
            foreach (var skin in SkinManager.SkinsList)
            {
                if (favSkins.Contains(skin.GetId()))
                {
                    continue;
                }
                var skinName = skin.GetName();
                skinName = skinName.Length > CustomKnight.GlobalSettings.NameLength ? skinName.Substring(0, CustomKnight.GlobalSettings.NameLength) : skinName;

                var btn = content.AddButton(skinName, skinName, (e) =>
                {
                    CustomKnight.GlobalSettings.AddRecentSkin(skin.GetId());
                    UIController.CreateUpdateGUI();
                    CoroutineHelper.WaitForFramesBeforeInvoke(1, () =>
                    {
                        scrollRect.verticalNormalizedPosition = 1f;
                        SkinManager.SetSkinById(skin.GetId());
                    });
                });
            }
        }


        static UIButton AddButton(this GameObject parent, string name, Sprite sprite, Action<UIButton> callback)
        {
            return new UIButton(parent, name, sprite, callback);
        }

        static UIButton AddButton(this GameObject parent, string name, string displayName, Action<UIButton> callback)
        {
            return new UIButton(parent, name, displayName, callback);
        }
    }
}
