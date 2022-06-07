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
        public RandomPhaseSetting RandomPhase = RandomPhaseSetting.Off;
        public bool RespectLighting = true;
        public float XFrequency = 0;
        public float YFrequency = 0;
        public float TimeFrequency = 0;
    }
}