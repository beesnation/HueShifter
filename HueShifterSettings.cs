using System;

namespace HueShifter
{
    public enum RandomPhaseSetting
    {
        Off,
        PerMapArea,
        PerRoom,
    }
    
    public class HueShifterSettings
    {
        public float Phase = 0;
        public RandomPhaseSetting RandomPhase = RandomPhaseSetting.PerMapArea;
        public bool RespectLighting = true;
        public float XFrequency = 0;
        public float YFrequency = 0;
        public float ZFrequency = 0;
        public float TimeFrequency = 0;
        public bool AllowVanillaPhase = false;
    }
}