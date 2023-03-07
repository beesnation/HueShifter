using System;
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
        public Shader RainbowGrassDefault;
        public Shader RainbowGrassLit;

        public Dictionary<string, float> Palette = new();

        // Rider did this it's more efficient or something
        private static readonly int PhaseProperty = Shader.PropertyToID("_Phase");
        private static readonly int FrequencyProperty = Shader.PropertyToID("_Frequency");

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
            // foreach (var name in assetBundle.GetAllAssetNames()) Log($"assetBundle contains {name}");

            RainbowDefault = assetBundle.LoadAsset<Shader>("assets/shader/rainbowdefault.shader");
            RainbowScreenBlend = assetBundle.LoadAsset<Shader>("assets/shader/rainbowscreenblend.shader");
            RainbowLit = assetBundle.LoadAsset<Shader>("assets/shader/rainbowlit.shader");
            RainbowParticleAdd = assetBundle.LoadAsset<Shader>("assets/shader/rainbowparticleadd.shader");
            RainbowParticleAddSoft = assetBundle.LoadAsset<Shader>("assets/shader/rainbowparticleaddsoft.shader");
            RainbowGrassDefault = assetBundle.LoadAsset<Shader>("assets/shader/rainbowgrassdefault.shader");
            RainbowGrassLit = assetBundle.LoadAsset<Shader>("assets/shader/rainbowgrasslit.shader");
        }

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Log("Initializing");
            Instance = this;
            if (RainbowDefault is null) LoadAssets();
            On.GameManager.OnNextLevelReady += OnNextLevelReady;
            LightingHandler.Hook();
        }

        public void Unload()
        {
            On.GameManager.OnNextLevelReady -= OnNextLevelReady;
            LightingHandler.Unhook();
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
                case RandomPhaseSetting.RandomPerMapArea:
                    location = GameManager.instance.sm.mapZone.ToString();
                    break;
                case RandomPhaseSetting.RandomPerRoom:
                    location = GameManager.instance.sceneName;
                    break;
                case RandomPhaseSetting.Fixed:
                default:
                    return GS.Phase / 360;
            }

            if (!Palette.ContainsKey(location))
                Palette[location] = GS.AllowVanillaPhase ? Random.Range(0f, 1f) : Random.Range(0.05f, 0.95f);
            return Palette[location];
        }

        public void SetAllTheShaders()
        {
            var props = new MaterialPropertyBlock();
            var frequencyVector = new Vector4(GS.XFrequency / 40, GS.YFrequency / 40, GS.ZFrequency / 200,
                GS.TimeFrequency / 10);

            foreach (var renderer in UObject.FindObjectsOfType<Renderer>(true))
            {
                if (GameManager.GetBaseSceneName(renderer.gameObject.scene.name) != GameManager.instance.sceneName) continue;
                if (renderer.gameObject.name == "Item Sprite") continue;

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
                        "Hollow Knight/Grass-Default" => RainbowGrassDefault,
                        "Hollow Knight/Grass-Diffuse" => GS.RespectLighting ? RainbowGrassLit : RainbowGrassDefault,
                        _ => material.shader
                    };

                    if (material.shader.name is not (
                        "Custom/RainbowLit" or
                        "Custom/RainbowDefault" or
                        "Custom/RainbowScreenBlend" or
                        "Custom/RainbowParticleAdd" or
                        "Custom/RainbowParticleAddSoft" or
                        "Custom/RainbowGrassDefault" or
                        "Custom/RainbowGrassLit")) continue;
                    renderer.GetPropertyBlock(props);
                    props.SetFloat(PhaseProperty, GetPhase());
                    props.SetVector(FrequencyProperty, frequencyVector);
                    renderer.SetPropertyBlock(props);
                }
            }
        }

        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates)
        {
            _menuRef ??= new Menu("HueShifter", new Element[]
            {
                toggleDelegates?.CreateToggle("Mod Enabled", ""),
                new HorizontalOption("Randomize Hues", "", Enum.GetNames(typeof(RandomPhaseSetting)),
                    val =>
                    {
                        GS.RandomPhase = (RandomPhaseSetting) val;
                        UpdateMenu();
                    },
                    () => (int) GS.RandomPhase),
                new CustomSlider("Hue Shift Angle",
                    val => GS.Phase = val,
                    () => GS.Phase, 
                    0, 360f, Id: "PhaseSlider")
                {isVisible = GS.RandomPhase == RandomPhaseSetting.Fixed},
                new HorizontalOption("Allow Vanilla Colours?", "", new[] {"False", "True"},
                        val => GS.AllowVanillaPhase = val != 0,
                        () => GS.AllowVanillaPhase ? 1 : 0, Id: "AllowVanillaOption")
                    {isVisible = GS.RandomPhase != RandomPhaseSetting.Fixed},
                new MenuButton("Re-roll Palette", "", _ =>
                {
                    Palette.Clear();
                    SetAllTheShaders();
                }, Id: "ReRollButton") {isVisible = GS.RandomPhase != RandomPhaseSetting.Fixed},
                new HorizontalOption("Shift Scene Lighting", "Applies on room reload",
                    new[] {"False", "True"},
                    val => GS.ShiftLighting = val != 0,
                    () => GS.ShiftLighting ? 1 : 0),
                new HorizontalOption("Respect Lighting", "Whether scene lighting tints recoloured objects. Applies on room reload",
                    new[] {"False", "True"},
                    val => GS.RespectLighting = val != 0,
                    () => GS.RespectLighting ? 1 : 0),
                new CustomSlider("Rainbow X",
                        val => GS.XFrequency = val,
                        () => GS.XFrequency,
                        -100, 100),
                new CustomSlider("Rainbow Y",
                        val => GS.YFrequency = val,
                        () => GS.YFrequency,
                        -100, 100),
                new CustomSlider("Rainbow Z",
                        val => GS.ZFrequency = val,
                        () => GS.ZFrequency,
                        -100, 100),
                new CustomSlider("Animate Speed",
                        val => GS.TimeFrequency = val,
                        () => GS.TimeFrequency,
                        -100, 100),
                new MenuButton("Apply to Current Room", "", _ => SetAllTheShaders()),
                new MenuButton("Reset to Defaults", "", _ =>
                {
                    GS = new HueShifterSettings();
                    UpdateMenu();
                    SetAllTheShaders();
                })
            });
            return _menuRef.GetMenuScreen(modListMenu);
        }

        private void UpdateMenu()
        {
            _menuRef.Find("PhaseSlider").isVisible = GS.RandomPhase == RandomPhaseSetting.Fixed;
            _menuRef.Find("AllowVanillaOption").isVisible = GS.RandomPhase != RandomPhaseSetting.Fixed;
            _menuRef.Find("ReRollButton").isVisible = GS.RandomPhase != RandomPhaseSetting.Fixed;
            _menuRef.Update();
        }

        public bool ToggleButtonInsideMenu => true;
    }
}