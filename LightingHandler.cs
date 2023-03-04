using MonoMod.RuntimeDetour;
using UnityEngine;

namespace HueShifter;

public static class LightingHandler
{
    public static void Hook() => On.SceneManager.SetLighting += OnSetLighting;
    public static void Unhook() => On.SceneManager.SetLighting -= OnSetLighting;
    
    private static void OnSetLighting(On.SceneManager.orig_SetLighting orig, Color ambientlightcolor, float ambientlightintensity)
    {
        if (HueShifter.Instance.GS.ShiftLighting)
        {
            Color.RGBToHSV(ambientlightcolor, out var h, out var s, out var v);
            ambientlightcolor = Color.HSVToRGB( Mathf.Repeat(h + HueShifter.Instance.GetPhase(), 1.0f), s, v);
        }
        orig(ambientlightcolor, ambientlightintensity);
    }
}