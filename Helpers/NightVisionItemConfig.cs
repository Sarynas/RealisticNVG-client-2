using System;
using System.Collections.Generic;
using UnityEngine;
using WindowsInput.Native;

namespace BorkelRNVG
{
    // feel like this shit is way too over-engineered
    public class NightVisionItemConfig
    {
        public float Intensity { get; set; }
        public float NoiseIntensity { get; set; }
        public float NoiseScale { get; set; }
        public float MaskSize { get; set; }
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }
        public VirtualKeyCode Key { get; set; }
        public Texture2D BinocularMaskTexture { get; set; }
        public Action Update { get; set; }

        // am i abusing delegates too hard? probably not.. just feels weird
        public NightVisionItemConfig(
            Func<float> intensityCalc,
            Func<float> noiseIntensityCalc,
            Func<float> noiseScaleCalc,
            Func<float> maskSizeCalc,
            Func<float> r,
            Func<float> g,
            Func<float> b,
            VirtualKeyCode key,
            Texture2D binocularMaskTexture)
        {
            // define the update action
            Update = () => 
            {
                Intensity = intensityCalc();
                NoiseIntensity = noiseIntensityCalc();
                NoiseScale = noiseScaleCalc();
                MaskSize = maskSizeCalc();
                R = r();
                G = g();
                B = b();
                Key = key;
                BinocularMaskTexture = binocularMaskTexture;
            };

            // initial calculation
            Update();
        }

        public NightVisionItemConfig(
            Func<float> intensityCalc,
            Func<float> noiseIntensityCalc,
            Func<float> noiseScaleCalc,
            Func<float> maskSizeCalc,
            Func<float> r,
            Func<float> g,
            Func<float> b,
            VirtualKeyCode key): 
            this(intensityCalc, noiseIntensityCalc, noiseScaleCalc, maskSizeCalc, r, g, b, key, null) {}

        public static Dictionary<string, NightVisionItemConfig> Items = new();

        public static NightVisionItemConfig Add(string itemId, NightVisionItemConfig config)
        {
            if (Items.ContainsKey(itemId))
                return Items[itemId];

            Items.Add(itemId, config);
            return config;
        }

        public static void Remove(string itemId) // but why would you use it..? idk
        {
            if (Items.ContainsKey(itemId))
            {
                Items.Remove(itemId);
            }
        }

        public static NightVisionItemConfig Get(string itemId)
        {
            Items.TryGetValue(itemId, out NightVisionItemConfig config);
            return config;
        }

        public static void InitializeNVGs()
        {
            // vanilla GPNVG-18
            string gpnvg18 = "5c0558060db834001b735271";
            Add(gpnvg18, new NightVisionItemConfig(
                () => Plugin.quadGain.Value * Plugin.globalGain.Value + Plugin.quadGain.Value * Plugin.globalGain.Value * 0.3f * Plugin.gatingLevel.Value / 2,
                () => 2 * Plugin.quadNoiseIntensity.Value,
                () => 2f - 2 * Plugin.quadNoiseSize.Value,
                () => Plugin.quadMaskSize.Value * Plugin.globalMaskSize.Value,
                () => Plugin.quadR.Value / 255,
                () => Plugin.quadG.Value / 255,
                () => Plugin.quadB.Value / 255,
                VirtualKeyCode.NUMPAD9
            ));

            // artem
            Add("66326bfd46817c660d01512e", Get(gpnvg18));
            Add("66326bfd46817c660d015148", Get(gpnvg18));
            Add("66326bfd46817c660d015146", Get(gpnvg18));

            // PVS-14
            Add("57235b6f24597759bf5a30f1", new NightVisionItemConfig(
                () => Plugin.pvsGain.Value * Plugin.globalGain.Value + Plugin.pvsGain.Value * Plugin.globalGain.Value * 0.3f * Plugin.gatingLevel.Value / 2,
                () => 2 * Plugin.pvsNoiseIntensity.Value,
                () => 2f - 2 * Plugin.pvsNoiseSize.Value,
                () => Plugin.pvsMaskSize.Value * Plugin.globalMaskSize.Value,
                () => Plugin.pvsR.Value / 255,
                () => Plugin.pvsG.Value / 255,
                () => Plugin.pvsB.Value / 255,
                VirtualKeyCode.NUMPAD8
            ));

            // N-15
            Add("5c066e3a0db834001b7353f0", new NightVisionItemConfig(
                () => Plugin.nGain.Value * Plugin.globalGain.Value + Plugin.nGain.Value * Plugin.globalGain.Value * 0.3f * Plugin.gatingLevel.Value / 2,
                () => 2 * Plugin.nNoiseIntensity.Value,
                () => 2f - 2 * Plugin.nNoiseSize.Value,
                () => Plugin.nMaskSize.Value * Plugin.globalMaskSize.Value,
                () => Plugin.nR.Value / 255,
                () => Plugin.nG.Value / 255,
                () => Plugin.nB.Value / 255,
                VirtualKeyCode.NUMPAD7,
                Plugin.maskBino
            ));

            // PNV-10T
            Add("5c0696830db834001d23f5da", new NightVisionItemConfig(
                () => Plugin.pnvGain.Value * Plugin.globalGain.Value + Plugin.pnvGain.Value * Plugin.globalGain.Value * 0.3f * Plugin.gatingLevel.Value / 2,
                () => 2 * Plugin.pnvNoiseIntensity.Value,
                () => 2f - 2 * Plugin.pnvNoiseSize.Value,
                () => Plugin.pnvMaskSize.Value * Plugin.globalMaskSize.Value,
                () => Plugin.pnvR.Value / 255,
                () => Plugin.pnvG.Value / 255,
                () => Plugin.pnvB.Value / 255,
                VirtualKeyCode.NUMPAD6,
                Plugin.maskPnv
            ));
        }
    }
}
