using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using BoplFixedMath;
using HarmonyLib;
using UnityEngine;

namespace More_Ability
{
    [BepInPlugin("com.000diggity000.More_Ability_Drop", "More Ability Drop", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static int abilityCount = 0;
        internal static ConfigFile config = new ConfigFile(Path.Combine(Paths.ConfigPath, "More_Ability_Drop_Config.cfg"), true);
        internal static ConfigEntry<int> AbilityConfig;
        private void Awake()
        {
            Harmony harmony = new Harmony("com.000diggity000.More_Ability_Drop");
            harmony.PatchAll(typeof(Patches));
            AbilityConfig = config.Bind("More Ability Drop", "Abilities", 3, "Minimum is 1");
            if (AbilityConfig.Value <= 1)
            {
                AbilityConfig.Value = 1;
            }
            abilityCount = AbilityConfig.Value;
        }
    }
    public class Patches
    {
        [HarmonyPatch(typeof(SlimeController), "DropAbilities")]
        [HarmonyPrefix]
        public static void DropAbilities_Patch(SlimeController __instance, ref AbilityReadyIndicator[] ___AbilityReadyIndicators, ref NamedSpriteList ___abilityIconsFull, ref NamedSpriteList ___abilityIconsDemo, ref DynamicAbilityPickup ___abilityPickupPrefab)
        {
            if (!GameSession.IsInitialized() || GameSessionHandler.HasGameEnded() || __instance.abilities.Count <= 0)
            {
                return;
            }
            int num = Settings.Get().NumberOfAbilities - 1;
            while (num >= 0 && (num >= ___AbilityReadyIndicators.Length || ___AbilityReadyIndicators[num] == null))
            {
                num--;
            }
            if (num < 0)
            {
                return;
            }
            Vec2 launchDirection = Vec2.NormalizedSafe(Vec2.up + new Vec2(Updater.RandomFix((Fix)(-0.3f), (Fix)0.3f), (Fix)0L));
            Sprite primarySprite = ___AbilityReadyIndicators[num].GetPrimarySprite();
            NamedSprite namedSprite = ___abilityIconsFull.sprites[___abilityIconsFull.IndexOf(primarySprite)];
            if (namedSprite.associatedGameObject == null)
            {
                namedSprite = ___abilityIconsDemo.sprites[___abilityIconsDemo.IndexOf(primarySprite)];
            }
            for (int i = 2; i <= Plugin.abilityCount; i++)
            {
                
                DynamicAbilityPickup dynamicAbilityPickup = FixTransform.InstantiateFixed<DynamicAbilityPickup>(___abilityPickupPrefab, __instance.body.position);
                
                
                dynamicAbilityPickup.InitPickup(namedSprite.associatedGameObject, primarySprite, launchDirection);
            }
            
        }
    }
}
