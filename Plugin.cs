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
    [BepInPlugin("com.000diggity000.More_Ability_Drop", "More Ability Drop", "1.1.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static int abilityCount = 0;
        public static bool random = false;
        internal static ConfigFile config = new ConfigFile(Path.Combine(Paths.ConfigPath, "More_Ability_Drop_Config.cfg"), true);
        internal static ConfigEntry<int> AbilityConfig;
        internal static ConfigEntry<bool> RandomConfig;
        private void Awake()
        {
            Harmony harmony = new Harmony("com.000diggity000.More_Ability_Drop");
            harmony.PatchAll(typeof(Patches));
            AbilityConfig = config.Bind("More Ability Drop", "Abilities", 3, "Minimum is 1");
            RandomConfig = config.Bind("More Ability Drop", "Random", false, "If true, the abilities dropped are random. Only abilities from the ones being used can spawn.");
            if (AbilityConfig.Value <= 0)
            {
                AbilityConfig.Value = 0;
            }
            abilityCount = AbilityConfig.Value;
            random = RandomConfig.Value;
        }
    }
    public class Patches
    {
        [HarmonyPatch(typeof(SlimeController), "DropAbilities")]
        [HarmonyPrefix]
        public static bool DropAbilities_Patch(SlimeController __instance, ref AbilityReadyIndicator[] ___AbilityReadyIndicators, ref NamedSpriteList ___abilityIconsFull, ref NamedSpriteList ___abilityIconsDemo, ref DynamicAbilityPickup ___abilityPickupPrefab)
        {
            if (!GameSession.IsInitialized() || GameSessionHandler.HasGameEnded() || __instance.abilities.Count <= 0)
            {
                return false;
            }
            PlayerHandler.Get().GetPlayer(__instance.playerNumber);
            for (int i = 0; i < ___AbilityReadyIndicators.Length; i++)
            {
                if (___AbilityReadyIndicators[i] != null)
                {
                    ___AbilityReadyIndicators[i].InstantlySyncTransform();
                }
            }
            int num = Settings.Get().NumberOfAbilities - 1;
            while (num >= 0 && (num >= ___AbilityReadyIndicators.Length || ___AbilityReadyIndicators[num] == null))
            {
                num--;
            }
            if (num < 0)
            {
                return false;
            }
            
            Vec2 launchDirection = Vec2.NormalizedSafe(Vec2.up + new Vec2(Updater.RandomFix((Fix)(-0.3f), (Fix)0.3f), (Fix)0L));
            
            for (int i = 1; i <= Plugin.abilityCount; i++)
            {
                int rnd = UnityEngine.Random.Range(0, (int)___AbilityReadyIndicators.Length);
                if (Plugin.random)
                {
                    Sprite primarySprite = ___AbilityReadyIndicators[rnd].GetPrimarySprite();
                    NamedSprite namedSprite = ___abilityIconsFull.sprites[___abilityIconsFull.IndexOf(primarySprite)];
                    DynamicAbilityPickup dynamicAbilityPickup = FixTransform.InstantiateFixed<DynamicAbilityPickup>(___abilityPickupPrefab, __instance.body.position);
                    dynamicAbilityPickup.InitPickup(namedSprite.associatedGameObject, primarySprite, launchDirection);
                }
                else
                {
                    Sprite primarySprite = ___AbilityReadyIndicators[num].GetPrimarySprite();
                    NamedSprite namedSprite = ___abilityIconsFull.sprites[___abilityIconsFull.IndexOf(primarySprite)];
                    DynamicAbilityPickup dynamicAbilityPickup = FixTransform.InstantiateFixed<DynamicAbilityPickup>(___abilityPickupPrefab, __instance.body.position);
                    dynamicAbilityPickup.InitPickup(namedSprite.associatedGameObject, primarySprite, launchDirection);
                }
                
                
            }
            return false;
        }
    }
}
