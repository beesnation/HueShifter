﻿using System;
using Modding;
using System.Collections.Generic;
using Satchel.BetterMenus;
using UnityEngine;
using Random = UnityEngine.Random;
using UObject = UnityEngine.Object;

namespace HueShifter
{
    public class HueShifter : Mod, ICustomMenuMod, ITogglableMod, IGlobalSettings<HueShifterSettings>
    {
        internal static HueShifter Instance;
        public HueShifterSettings GS { get; private set; } = new();
        public override string GetVersion() => GetType().Assembly.GetName().Version.ToString();

        private Menu _menuRef;

        public Shader RainbowDefault;
        public Shader RainbowScreenBlend;
        public Shader RainbowLit;
        public Shader RainbowParticleAdd;
        public Shader RainbowParticleAddSoft;
        
        public Dictionary<string, float> Palette = new();

        // Rider did this it's more efficient or something
        private static readonly int PhaseProperty = Shader.PropertyToID("_Phase");
        private static readonly int TimeFrequencyProperty = Shader.PropertyToID("_TimeFrequency");
        private static readonly int XFrequencyProperty = Shader.PropertyToID("_XFrequency");
        private static readonly int YFrequencyProperty = Shader.PropertyToID("_YFrequency");

        public void OnLoadGlobal(HueShifterSettings s) => GS = s;
        public HueShifterSettings OnSaveGlobal() => GS;

        public void LoadAssets()
        {
            var platform = Application.platform switch
            {
                RuntimePlatform.LinuxPlayer => "linux",
                RuntimePlatform.WindowsPlayer => "windows",
                RuntimePlatform.OSXPlayer => "osx",
                _ => throw new PlatformNotSupportedException("What platform are you even on??")
            };

            var assetBundle = AssetBundle.LoadFromStream(
                typeof(HueShifter).Assembly.GetManifestResourceStream(
                    $"HueShifter.Resources.AssetBundles.hueshiftshaders-{platform}"));
            foreach (var name in assetBundle.GetAllAssetNames()) Log($"assetBundle contains {name}");

            RainbowDefault = assetBundle.LoadAsset<Shader>("assets/shader/rainbowdefault.shader");
            RainbowScreenBlend = assetBundle.LoadAsset<Shader>("assets/shader/rainbowscreenblend.shader");
            RainbowLit = assetBundle.LoadAsset<Shader>("assets/shader/rainbowlit.shader");
            RainbowParticleAdd = assetBundle.LoadAsset<Shader>("assets/shader/rainbowparticleadd.shader");
            RainbowParticleAddSoft = assetBundle.LoadAsset<Shader>("assets/shader/rainbowparticleaddsoft.shader");
        }

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Log("Initializing");
            Instance = this;
            if (RainbowDefault is null) LoadAssets();
            On.GameManager.OnNextLevelReady += OnNextLevelReady;
        }

        public void Unload()
        {
            On.GameManager.OnNextLevelReady -= OnNextLevelReady;
        }
        private void OnNextLevelReady(On.GameManager.orig_OnNextLevelReady orig, GameManager self)
        {
            orig(self);
            SetAllTheShaders();
        }

        public float GetPhase()
        {
            string location;
            switch (GS.RandomPhase)
            {
                case RandomPhaseSetting.PerMapArea:
                    location = GameManager.instance.sm.mapZone.ToString();
                    break;
                case RandomPhaseSetting.PerRoom:
                    location = GameManager.instance.sceneName;
                    break;
                case RandomPhaseSetting.Off:
                default:
                    return GS.Phase/360;
            }
            if (!Palette.ContainsKey(location)) Palette[location] = Random.Range(0f, 1f);
            return Palette[location];
        }

        public void SetAllTheShaders()
        {
            var props = new MaterialPropertyBlock();

            foreach (var renderer in UObject.FindObjectsOfType<Renderer>(true))
            {
                if (renderer.gameObject.scene.name != GameManager.instance.sceneName) continue;

                foreach (var material in renderer.materials)
                {
                    material.shader = material.shader.name switch
                    {
                        "Sprites/Lit" => GS.RespectLighting ? RainbowLit : RainbowDefault,
                        "Sprites/Default" => RainbowDefault,
                        "Sprites/Cherry-Default" => RainbowDefault,
                        "UI/BlendModes/Screen" => RainbowScreenBlend,
                        "Legacy Shaders/Particles/Additive" => RainbowParticleAdd,
                        "Legacy Shaders/Particles/Additive (Soft)" => RainbowParticleAddSoft,
                        _ => material.shader
                    };

                    if (material.shader.name is not (
                        "Custom/RainbowLit" or
                        "Custom/RainbowDefault" or
                        "Custom/RainbowScreenBlend" or
                        "Custom/RainbowParticleAdd" or
                        "Custom/RainbowParticleAddSoft")) continue;
                    renderer.GetPropertyBlock(props);
                    props.SetFloat(PhaseProperty, GetPhase());
                    props.SetFloat(TimeFrequencyProperty, GS.TimeFrequency / 10);
                    props.SetFloat(XFrequencyProperty, GS.XFrequency * GS.XFrequency * Math.Sign(GS.XFrequency) / 20);
                    props.SetFloat(YFrequencyProperty, GS.YFrequency * GS.YFrequency * Math.Sign(GS.YFrequency) / 20);
                    renderer.SetPropertyBlock(props);
                }
            }
        }

        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? maybeToggleDelegates)
        {
            if (maybeToggleDelegates is not { } toggleDelegates) throw new InvalidOperationException();
            _menuRef ??= new Menu("HueShifter", new Element[]
                {
                    new HorizontalOption("Enabled", "", new[] {"False", "True"},
                        val => toggleDelegates.SetModEnabled(val != 0),
                        () => toggleDelegates.GetModEnabled() ? 1 : 0),
                    new HorizontalOption("Respect Lighting", "Turn off for more vibrant colours. Applies on room reload.", new[] {"False", "True"},
                        val => GS.RespectLighting = (val != 0),
                        () => GS.RespectLighting ? 1 : 0),
                    new CustomSlider("Phase",
                            val => GS.Phase = val,
                            () => GS.Phase)
                        {minValue = 0, maxValue = 360f, wholeNumbers = false},
                    new HorizontalOption("Random Phase", "", Enum.GetNames(typeof(RandomPhaseSetting)),
                        val => GS.RandomPhase = (RandomPhaseSetting) val,
                        () => (int) GS.RandomPhase),
                    new MenuButton("Re-roll Palette", "", _ =>
                    {
                        Palette.Clear();
                        SetAllTheShaders();
                    }),
                    new CustomSlider("X Frequency",
                            val => GS.XFrequency = val,
                            () => GS.XFrequency)
                        {minValue = -10f, maxValue = 10f, wholeNumbers = false},
                    new CustomSlider("Y Frequency",
                            val => GS.YFrequency = val,
                            () => GS.YFrequency)
                        {minValue = -10f, maxValue = 10f, wholeNumbers = false},
                    new CustomSlider("Time Frequency",
                            val => GS.TimeFrequency = val,
                            () => GS.TimeFrequency)
                        {minValue = -100, maxValue = 100f, wholeNumbers = false},
                    new MenuButton("Apply to Current Room", "", _ => SetAllTheShaders()),
                    new MenuButton("Reset to Defaults", "", _ =>
                    {
                        GS = new HueShifterSettings();
                        _menuRef.Update();
                        SetAllTheShaders();
                    })
                });
            return _menuRef.GetMenuScreen(modListMenu);
        }

        public bool ToggleButtonInsideMenu => true;
    }
}