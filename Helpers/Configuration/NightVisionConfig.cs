using BepInEx.Configuration;

namespace BorkelRNVG.Helpers.Configuration
{
    public class NightVisionConfig
    {
        // night vision
        public ConfigEntry<float> Gain;
        public ConfigEntry<float> NoiseIntensity;
        public ConfigEntry<float> NoiseSize;
        public ConfigEntry<float> MaskSize;
        public ConfigEntry<float> Red;
        public ConfigEntry<float> Green;
        public ConfigEntry<float> Blue;

        // auto-gating
        public ConfigEntry<bool> AutoGatingEnabled;
        public ConfigEntry<float> GatingSpeed;
        public ConfigEntry<float> MaxBrightness;
        public ConfigEntry<float> MinBrightness;
        public ConfigEntry<float> MinBrightnessThreshold;
        public ConfigEntry<float> MaxBrightnessThreshold;

        // config constructor. all parameters are DEFAULT values
        public NightVisionConfig(ConfigFile config, string category,
            float gain, float noiseIntensity, float noiseSize, float maskSize,
            float red, float green, float blue,
            bool autoGatingEnabled, float gatingSpeed, float maxBrightness,
            float minBrightness, float minBrightnessThreshold, float maxBrightnessThreshold)
        {
            // night vision
            Gain = config.Bind(category, "1. Gain", gain, new ConfigDescription("Light amplification", new AcceptableValueRange<float>(0f, 5f)));
            NoiseIntensity = config.Bind(category, "2. Noise Intensity", noiseIntensity, new ConfigDescription("Controls the intensity of the noise overlay.", new AcceptableValueRange<float>(0f, 1f)));
            NoiseSize = config.Bind(category, "3. Noise Scale", noiseSize, new ConfigDescription("Controls the scale of the noise pattern.", new AcceptableValueRange<float>(0.01f, 0.99f)));
            MaskSize = config.Bind(category, "4. Mask Size", maskSize, new ConfigDescription("Adjusts the size of the NVG mask.", new AcceptableValueRange<float>(0.01f, 2f)));
            Red = config.Bind(category, "5. Red", red, new ConfigDescription("Adjusts the red color component of the NVG tint.", new AcceptableValueRange<float>(0f, 255f)));
            Green = config.Bind(category, "6. Green", green, new ConfigDescription("Adjusts the green color component of the NVG tint.", new AcceptableValueRange<float>(0f, 255f)));
            Blue = config.Bind(category, "7. Blue", blue, new ConfigDescription("Adjusts the blue color component of the NVG tint.", new AcceptableValueRange<float>(0f, 255f)));

            // auto-gating
            AutoGatingEnabled = config.Bind(category, "8. Enable Auto-Gating", autoGatingEnabled, new ConfigDescription("EXPERIMENTAL, WILL REDUCE FPS! Enables auto-gating, which automatically adjusts brightness to protect night vision devices from damage caused by bright lights."));
            GatingSpeed = config.Bind(category, "9. Auto-Gating Speed", gatingSpeed, new ConfigDescription("Changes the rate at which the auto-gating multiplier changes"));
            MaxBrightness = config.Bind(category, "10. Max Brightness Multiplier", maxBrightness, new ConfigDescription("Changes the maximum brightness multiplier for auto-gating.", null, new ConfigurationManagerAttributes() { IsAdvanced = true }));
            MinBrightness = config.Bind(category, "11. Max Brightness Multiplier", minBrightness, new ConfigDescription("Changes the minimum brightness multiplier for auto-gating.", null, new ConfigurationManagerAttributes() { IsAdvanced = true }));
            MinBrightnessThreshold = config.Bind(category, "12. Min Brightness Threshold", minBrightnessThreshold, new ConfigDescription("Changes the minimum brightness level for auto-gating", null, new ConfigurationManagerAttributes() { IsAdvanced = true }));
            MaxBrightnessThreshold = config.Bind(category, "13. Max Brightness Threshold", maxBrightnessThreshold, new ConfigDescription("Changes the maximum brightness level for auto-gating.", null, new ConfigurationManagerAttributes() { IsAdvanced = true }));

            Gain.SettingChanged += Util.ApplyNightVisionSettings;
            NoiseIntensity.SettingChanged += Util.ApplyNightVisionSettings;
            NoiseSize.SettingChanged += Util.ApplyNightVisionSettings;
            MaskSize.SettingChanged += Util.ApplyNightVisionSettings;
            Red.SettingChanged += Util.ApplyNightVisionSettings;
            Green.SettingChanged += Util.ApplyNightVisionSettings;
            Blue.SettingChanged += Util.ApplyNightVisionSettings;

            AutoGatingEnabled.SettingChanged += Util.ApplyGatingSettings;
            GatingSpeed.SettingChanged += Util.ApplyGatingSettings;
            MaxBrightness.SettingChanged += Util.ApplyGatingSettings;
            MinBrightness.SettingChanged += Util.ApplyGatingSettings;
            MinBrightnessThreshold.SettingChanged += Util.ApplyGatingSettings;
            MaxBrightnessThreshold.SettingChanged += Util.ApplyGatingSettings;
        }
    }
}

