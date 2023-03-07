using UnityEngine;

namespace HueShifter;

public static class HeroLightHandler
{
    public static void Hook() => On.SceneColorManager.UpdateScriptParameters += OnUpdateScriptParameters;
    public static void Unhook() => On.SceneColorManager.UpdateScriptParameters -= OnUpdateScriptParameters;

    private static void OnUpdateScriptParameters(On.SceneColorManager.orig_UpdateScriptParameters orig, SceneColorManager self)
    {
        orig(self);
        Color.RGBToHSV(HeroController.instance.heroLight.color, out var h, out var s, out var v);
        HeroController.instance.heroLight.color = Color.HSVToRGB( Mathf.Repeat(h + HueShifter.Instance.GetPhase(), 1.0f), s, v);
    }
}