using Dalamud.Game.ClientState.JobGauge.Types;
using ECommons.DalamudServices;
using XIVSlothCombo.Combos.JobHelpers;
using XIVSlothCombo.Combos.PvE.Content;
using XIVSlothCombo.CustomComboNS;
using XIVSlothCombo.CustomComboNS.Functions;
using XIVSlothCombo.Data;
using XIVSlothCombo.Extensions;


namespace XIVSlothCombo.Combos.PvE
{
    internal class MCH
    {
        public const byte JobID = 31;

        internal const uint
            CleanShot = 2873,
            HeatedCleanShot = 7413,
            SplitShot = 2866,
            HeatedSplitShot = 7411,
            SlugShot = 2868,
            GaussRound = 2874,
            Ricochet = 2890,
            HeatedSlugshot = 7412,
            Drill = 16498,
            HotShot = 2872,
            Reassemble = 2876,
            AirAnchor = 16500,
            Hypercharge = 17209,
            HeatBlast = 7410,
            SpreadShot = 2870,
            Scattergun = 25786,
            AutoCrossbow = 16497,
            RookAutoturret = 2864,
            RookOverdrive = 7415,
            AutomatonQueen = 16501,
            QueenOverdrive = 16502,
            Tactician = 16889,
            ChainSaw = 25788,
            BioBlaster = 16499,
            BarrelStabilizer = 7414,
            Wildfire = 2878,
            Dismantle = 2887,
            Flamethrower = 7418,
            CrownCollider = 25787;

        internal static class Buffs
        {
            internal const ushort
                Reassembled = 851,
                Tactician = 1951,
                Wildfire = 1946,
                Overheated = 2688,
                Flamethrower = 1205;
        }

        internal static class Debuffs
        {
            internal const ushort
            Dismantled = 2887;
        }

        internal static class Config
        {
            public static UserInt
                MCH_ST_SecondWindThreshold = new("MCH_ST_SecondWindThreshold"),
                MCH_AoE_SecondWindThreshold = new("MCH_AoE_SecondWindThreshold"),
                MCH_ST_RotationSelection = new("MCH_ST_RotationSelection"),
                MCH_VariantCure = new("MCH_VariantCure"),
                MCH_ST_TurretUsage = new("MCH_ST_Adv_TurretGauge"),
                MCH_AoE_TurretUsage = new("MCH_AoE_TurretUsage"),
                MCH_ST_ReassemblePool = new("MCH_ST_ReassemblePool"),
                MCH_AoE_ReassemblePool = new("MCH_AoE_ReassemblePool");
            public static UserBoolArray
                MCH_ST_Reassembled = new("MCH_ST_Reassembled"),
                MCH_AoE_Reassembled = new("MCH_AoE_Reassembled");
            public static UserBool
                MCH_AoE_Hypercharge = new("MCH_AoE_Hypercharge");
        }

        internal static class Levels
        {
            internal const byte
                SlugShot = 2,
                Hotshot = 4,
                GaussRound = 15,
                CleanShot = 26,
                Hypercharge = 30,
                HeatBlast = 35,
                RookOverdrive = 40,
                Wildfire = 45,
                Ricochet = 50,
                Drill = 58,
                AirAnchor = 76,
                AutoCrossbow = 52,
                HeatedSplitShot = 54,
                Tactician = 56,
                HeatedSlugshot = 60,
                HeatedCleanShot = 64,
                BioBlaster = 72,
                ChargedActionMastery = 74,
                QueenOverdrive = 80,
                Scattergun = 82,
                BarrelStabilizer = 66,
                ChainSaw = 90,
                Dismantle = 62;
        }
        internal class MCH_ST_BasicCombo : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.MCH_ST_BasicCombo;

            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                if (actionID is CleanShot)
                {
                    //1-2-3 Combo
                    if (comboTime > 0)
                    {
                        if (lastComboMove is SplitShot && LevelChecked(OriginalHook(SlugShot)))
                            return OriginalHook(SlugShot);

                        if (lastComboMove is SlugShot && LevelChecked(OriginalHook(CleanShot)))
                            return OriginalHook(CleanShot);
                    }

                    return OriginalHook(SplitShot);
                }

                return actionID;
            }
        }

        internal class MCH_ST_SimpleMode : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.MCH_ST_SimpleMode;
            internal static MCHOpenerLogic MCHOpener = new();

            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                float wildfireCDTime = GetCooldownRemainingTime(Wildfire);
                MCHGauge? gauge = GetJobGauge<MCHGauge>();
                bool interruptReady = ActionReady(All.HeadGraze) && CanInterruptEnemy();

                if (actionID is SplitShot)
                {
                    if (IsEnabled(CustomComboPreset.MCH_Variant_Cure) &&
                    IsEnabled(Variant.VariantCure) && PlayerHealthPercentageHp() <= Config.MCH_VariantCure)
                        return Variant.VariantCure;

                    if (IsEnabled(CustomComboPreset.MCH_Variant_Rampart) &&
                        IsEnabled(Variant.VariantRampart) &&
                        IsOffCooldown(Variant.VariantRampart) &&
                        CanWeave(actionID))
                        return Variant.VariantRampart;

                    // Opener for MCH
                    if (MCHOpener.DoFullOpener(false, out var openerId))
                        return openerId;

                    //Standard Rotation

                    // Interrupt
                    if (interruptReady)
                        return All.HeadGraze;

                    // BarrelStabilizer use
                    if (CanWeave(actionID) && gauge.Heat <= 45 && LevelChecked(BarrelStabilizer) && ActionReady(BarrelStabilizer))
                        return BarrelStabilizer;

                    // Wildfire

                    if (gauge.Heat >= 50 && CanDelayedWeave(actionID) && LevelChecked(ChainSaw) && ActionReady(Wildfire) &&
                        !gauge.IsOverheated && WasLastWeaponskill(AirAnchor)) //these try to ensure the correct loops
                        return Wildfire;

                    else if (gauge.Heat >= 50 && ActionReady(Wildfire))
                        return Wildfire;

                    //queen
                    if (CanWeave(actionID) && !gauge.IsOverheated && LevelChecked(OriginalHook(RookAutoturret)) && gauge.Battery > 0)
                    {
                        if (LevelChecked(ChainSaw) &&
                            ((gauge.Battery is 50 && CombatEngageDuration().TotalSeconds > 59 && CombatEngageDuration().TotalSeconds < 68) || // First Minute Queen 
                            (gauge.Battery is 100 && wildfireCDTime <= 7 && GetCooldownRemainingTime(AirAnchor) <= 3 && CombatEngageDuration().Minutes % 2 == 0) || // Even Minute Queen
                            (gauge.Battery >= 80 && CombatEngageDuration().Minutes % 2 == 1 && wildfireCDTime > 45 && wildfireCDTime < 70))) // Odd minute Queen
                            return OriginalHook(RookAutoturret);

                        else if (gauge.Battery is 100)
                            return OriginalHook(RookAutoturret);
                    }

                    if (CanWeave(actionID) && gauge.Heat >= 50 && LevelChecked(Hypercharge) && !gauge.IsOverheated)
                    {
                        //Protection & ensures Hyper charged is double weaved with WF during reopener
                        if ((WasLastAction(ChainSaw) && HasEffect(Buffs.Wildfire)) ||
                            (!LevelChecked(ChainSaw) && HasEffect(Buffs.Wildfire)) ||
                            !LevelChecked(Wildfire))
                            return Hypercharge;

                        if (LevelChecked(OriginalHook(AirAnchor)) && GetCooldownRemainingTime(OriginalHook(AirAnchor)) >= 8)
                        {
                            if (LevelChecked(Drill) && GetCooldownRemainingTime(Drill) >= 8)
                            {
                                if (LevelChecked(ChainSaw) && GetCooldownRemainingTime(ChainSaw) >= 8)
                                {

                                    if (UseHyperchargeStandard(gauge))
                                        return Hypercharge;
                                }

                                else if (!LevelChecked(ChainSaw))
                                {

                                    if (UseHyperchargeStandard(gauge))
                                        return Hypercharge;
                                }
                            }

                            else if (!LevelChecked(Drill))
                            {

                                if (UseHyperchargeStandard(gauge))
                                   return Hypercharge;
                            }

                        }
                        else if (!LevelChecked(OriginalHook(AirAnchor)))
                        {

                            if (UseHyperchargeStandard(gauge))
                                return Hypercharge;
                        }

                    }

                    //Heatblast, Gauss, Rico
                    if (gauge.IsOverheated)
                    {
                        if (CanWeave(actionID) && WasLastAction(HeatBlast) && ActionWatching.GetAttackType(ActionWatching.LastAction) != ActionWatching.ActionAttackType.Ability)
                        {
                            if (LevelChecked(GaussRound) && GetRemainingCharges(GaussRound) >= GetRemainingCharges(Ricochet))
                                return GaussRound;

                            if (LevelChecked(Ricochet) && GetRemainingCharges(Ricochet) > GetRemainingCharges(GaussRound))
                                return Ricochet;
                        }

                        if (LevelChecked(HeatBlast))
                            return HeatBlast;
                    }

                    if (ReassembledTools(ref actionID))
                        return actionID;

                    //gauss and ricochet overcap protection
                    if (CanWeave(actionID) && !gauge.IsOverheated && !HasEffect(Buffs.Wildfire))
                    {
                        if (level >= Levels.Ricochet && HasCharges(Ricochet))
                            return Ricochet;
                        if (level >= Levels.GaussRound && HasCharges(GaussRound))
                            return GaussRound;
                    }


                    //1-2-3 Combo
                    if (comboTime > 0)
                    {
                        if (lastComboMove is SplitShot && LevelChecked(OriginalHook(SlugShot)))
                            return OriginalHook(SlugShot);

                        if (!LevelChecked(Drill) && !HasEffect(Buffs.Reassembled) && HasCharges(Reassemble) && lastComboMove is SlugShot)
                            return Reassemble;

                        if (lastComboMove is SlugShot && LevelChecked(OriginalHook(CleanShot)))
                            return OriginalHook(CleanShot);
                    }

                    return OriginalHook(SplitShot);
                }

                return actionID;
            }
            private static bool ReassembledTools(ref uint actionId)
            {
                // TOOLS!! ChainSaw Drill Air Anchor
                if (!HasEffect(Buffs.Wildfire) &&
                    !HasEffect(Buffs.Reassembled) && HasCharges(Reassemble) &&
                    ((LevelChecked(ChainSaw) && GetCooldownRemainingTime(ChainSaw) < 1) || ActionReady(ChainSaw) ||
                    (LevelChecked(AirAnchor) && GetCooldownRemainingTime(AirAnchor) < 1) || ActionReady(AirAnchor) ||
                    (!LevelChecked(AirAnchor) && LevelChecked(Drill) && (GetCooldownRemainingTime(Drill) < 1)) || ActionReady(Drill)))
                {
                    actionId = Reassemble;
                    return true;
                }

                if (ChainSaw.LevelChecked() &&
                    (GetCooldownRemainingTime(ChainSaw) < 1 || ActionReady(ChainSaw)))
                {
                    actionId = ChainSaw;
                    return true;
                }

                if (LevelChecked(OriginalHook(AirAnchor)) &&
                    (GetCooldownRemainingTime(OriginalHook(AirAnchor)) < 1 || ActionReady(OriginalHook(AirAnchor))))
                {
                    actionId = OriginalHook(AirAnchor);
                    return true;
                }

                if (Drill.LevelChecked() &&
                    (GetCooldownRemainingTime(Drill) < 1 || ActionReady(Drill)))
                {
                    actionId = Drill;
                    return true;
                }

                return false;
            }


            private bool UseHyperchargeStandard(MCHGauge gauge)
            {
                // i really do not remember why i put > 70 here for heat, and im afraid if i remove it itll break it lol
                if (CombatEngageDuration().Minutes == 0 &&
                    (gauge.Heat > 70 || CombatEngageDuration().Seconds <= 30) && !WasLastWeaponskill(OriginalHook(CleanShot)))
                    return true;

                if (CombatEngageDuration().Minutes > 0)
                {
                    if (CombatEngageDuration().Minutes % 2 == 1 && gauge.Heat >= 90)
                        return true;

                    if (CombatEngageDuration().Minutes % 2 == 0)
                        return true;
                }
                return false;
            }
        }

        internal class MCH_ST_AdvancedMode : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.MCH_ST_AdvancedMode;
            internal static MCHOpenerLogic MCHOpener = new();

            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                float wildfireCDTime = GetCooldownRemainingTime(Wildfire);
                MCHGauge? gauge = GetJobGauge<MCHGauge>();
                int rotationSelection = Config.MCH_ST_RotationSelection;
                bool interruptReady = ActionReady(All.HeadGraze) && CanInterruptEnemy();

                if (actionID is SplitShot or HeatedSplitShot)
                {
                    if (IsEnabled(CustomComboPreset.MCH_Variant_Cure) &&
                    IsEnabled(Variant.VariantCure) && PlayerHealthPercentageHp() <= Config.MCH_VariantCure)
                        return Variant.VariantCure;

                    if (IsEnabled(CustomComboPreset.MCH_Variant_Rampart) &&
                        IsEnabled(Variant.VariantRampart) &&
                        IsOffCooldown(Variant.VariantRampart) &&
                        CanWeave(actionID))
                        return Variant.VariantRampart;

                    // Opener for MCH
                    if (IsEnabled(CustomComboPreset.MCH_ST_Adv_Opener))
                    {
                        if (MCHOpener.DoFullOpener(false, out var openerId))
                            return openerId;
                    }

                    //Standard Rotation
                    if (rotationSelection is 0)
                    {
                        // Interrupt
                        if (IsEnabled(CustomComboPreset.MCH_ST_Adv_Interrupt) && interruptReady)
                            return All.HeadGraze;

                        // BarrelStabilizer use
                        if (IsEnabled(CustomComboPreset.MCH_ST_Adv_Stabilizer) && CanWeave(actionID) && gauge.Heat <= 50 &&
                            ActionReady(BarrelStabilizer) && !gauge.IsOverheated)
                            return BarrelStabilizer;

                        // Wildfire
                        if (IsEnabled(CustomComboPreset.MCH_ST_Adv_WildFire))
                        {
                            if (gauge.Heat >= 50 && CanDelayedWeave(actionID) && LevelChecked(ChainSaw) && ActionReady(Wildfire) &&
                                !gauge.IsOverheated && WasLastWeaponskill(AirAnchor) && GetCooldownRemainingTime(ChainSaw) < 1) //these try to ensure the correct loops
                                return Wildfire;

                            else if (gauge.Heat >= 50 && ActionReady(Wildfire))
                                return Wildfire;
                        }

                        //queen
                        if (IsEnabled(CustomComboPreset.MCH_Adv_TurretQueen) &&
                            CanWeave(actionID) && !gauge.IsOverheated && LevelChecked(OriginalHook(RookAutoturret)) && gauge.Battery > 0)
                        {
                            if (Config.MCH_ST_TurretUsage == 0 && gauge.Battery >= 50)
                                return OriginalHook(RookAutoturret);

                            if (Config.MCH_ST_TurretUsage == 1)
                            {
                                if (LevelChecked(ChainSaw) &&
                                    ((gauge.Battery is 50 && CombatEngageDuration().TotalSeconds > 59 && CombatEngageDuration().TotalSeconds < 68) || // First Minute Queen 
                                    (gauge.Battery is 100 && wildfireCDTime <= 7 && GetCooldownRemainingTime(AirAnchor) <= 3 && CombatEngageDuration().Minutes % 2 == 0) || // Even Minute Queen
                                    (gauge.Battery >= 80 && CombatEngageDuration().Minutes % 2 == 1 && wildfireCDTime > 45 && wildfireCDTime < 70))) // Odd minute Queen
                                    return OriginalHook(RookAutoturret);

                                else if (gauge.Battery is 100)
                                    return OriginalHook(RookAutoturret);
                            }
                        }

                        if (IsEnabled(CustomComboPreset.MCH_ST_Adv_Hypercharge) &&
                                CanWeave(actionID) && gauge.Heat >= 50 && LevelChecked(Hypercharge) && !gauge.IsOverheated)
                        {
                            //Protection & ensures Hyper charged is double weaved with WF during reopener
                            if ((WasLastAction(ChainSaw) && HasEffect(Buffs.Wildfire)) ||
                                (!LevelChecked(ChainSaw) && HasEffect(Buffs.Wildfire)) ||
                                !LevelChecked(Wildfire))
                                return Hypercharge;

                            if (LevelChecked(OriginalHook(AirAnchor)) && GetCooldownRemainingTime(OriginalHook(AirAnchor)) >= 8)
                            {
                                if (LevelChecked(Drill) && GetCooldownRemainingTime(Drill) >= 8)
                                {
                                    if (LevelChecked(ChainSaw) && GetCooldownRemainingTime(ChainSaw) >= 8)
                                    {
                                        if (UseHyperchargeDelayedTools(gauge, wildfireCDTime))
                                            return Hypercharge;
                                    }

                                    else if (!LevelChecked(ChainSaw))
                                    {
                                        if (UseHyperchargeDelayedTools(gauge, wildfireCDTime))
                                            return Hypercharge;
                                    }
                                }

                                else if (!LevelChecked(Drill))
                                {
                                    if (UseHyperchargeDelayedTools(gauge, wildfireCDTime))
                                        return Hypercharge;
                                }
                            }

                            else if (!LevelChecked(OriginalHook(AirAnchor)))
                            {
                                if (UseHyperchargeDelayedTools(gauge, wildfireCDTime))
                                    return Hypercharge;
                            }

                        }

                        //Heatblast, Gauss, Rico

                        if (gauge.IsOverheated && LevelChecked(HeatBlast))
                        {
                            if (IsEnabled(CustomComboPreset.MCH_ST_Adv_GaussRicochet))
                            {
                                if (CanWeave(actionID))
                                {
                                    if (GetRemainingCharges(GaussRound) >= GetRemainingCharges(Ricochet) && WasLastAction(HeatBlast))
                                        return GaussRound;

                                    if (GetRemainingCharges(Ricochet) >= GetRemainingCharges(GaussRound) && WasLastAction(HeatBlast))
                                        return Ricochet;
                                }
                            }

                            if (IsEnabled(CustomComboPreset.MCH_ST_Adv_HeatBlast))
                                return HeatBlast;
                        }

                        if (IsEnabled(CustomComboPreset.MCH_ST_Adv_HeatBlast) &&
                            gauge.IsOverheated && LevelChecked(HeatBlast))
                            return HeatBlast;

                        if (ReassembledTools(ref actionID))
                            return actionID;
                    }

                    //123Tools Rotation
                    if (rotationSelection is 1)
                    {
                        if (IsEnabled(CustomComboPreset.MCH_ST_Adv_Interrupt) && interruptReady)
                            return All.HeadGraze;

                        // BarrelStabilizer use
                        if (IsEnabled(CustomComboPreset.MCH_ST_Adv_Stabilizer) && CanWeave(actionID) &&
                            gauge.Heat <= 55 && ActionReady(BarrelStabilizer) &&
                            ((((wildfireCDTime <= 25 && wildfireCDTime >= 100) || HasEffect(Buffs.Wildfire)) && IsEnabled(CustomComboPreset.MCH_ST_Adv_Stabilizer_Wildfire_Only)) ||
                            (wildfireCDTime >= 110 && !IsEnabled(CustomComboPreset.MCH_ST_Adv_Stabilizer_Wildfire_Only))))
                            return BarrelStabilizer;

                        //Wildfire stuff
                        //these TRY to ensure the correct loop, HC > CS > WF
                        if (IsEnabled(CustomComboPreset.MCH_ST_Adv_WildFire) && ActionReady(Wildfire))
                        {
                            if (CanDelayedWeave(actionID, 0.8) && gauge.IsOverheated && WasLastWeaponskill(ChainSaw))
                                return Wildfire;

                            else if (CanWeave(actionID) && gauge.IsOverheated)
                                return Wildfire;
                        }

                        //Queen aka Robot
                        if (IsEnabled(CustomComboPreset.MCH_Adv_TurretQueen) && Config.MCH_ST_TurretUsage == 1 && CanWeave(actionID) && !gauge.IsRobotActive && (!WasLastAbility(Wildfire)) && LevelChecked(OriginalHook(RookAutoturret)))
                        {
                            // First condition
                            if (gauge.Battery == 50 && CombatEngageDuration().TotalSeconds > 61 && CombatEngageDuration().TotalSeconds < 68)
                                return OriginalHook(RookAutoturret);

                            // Second condition
                            if (!WasLastAction(OriginalHook(CleanShot)) && gauge.Battery == 100 && gauge.LastSummonBatteryPower == 50 &&
                                (GetCooldownRemainingTime(AirAnchor) <= 3 || ActionReady(AirAnchor)) && AirAnchor.LevelChecked())
                                return OriginalHook(RookAutoturret);

                            // Third condition
                            while (gauge.LastSummonBatteryPower == 100 && gauge.Battery >= 90) //was previously 80 with 30 overcap for 10mins
                                return OriginalHook(RookAutoturret);

                            // Fourth condition
                            while (gauge.LastSummonBatteryPower != 50 && gauge.Battery == 100 && (GetCooldownRemainingTime(AirAnchor) <= 3 || ActionReady(AirAnchor)) && AirAnchor.LevelChecked())
                                return OriginalHook(RookAutoturret);
                        }

                        if (IsEnabled(CustomComboPreset.MCH_Adv_TurretQueen) &&
                            Config.MCH_ST_TurretUsage == 0 &&
                            LevelChecked(OriginalHook(RookAutoturret)) && gauge.Battery >= 50 && !gauge.IsRobotActive)
                            return OriginalHook(RookAutoturret);

                        //Overheated Reassemble & Heatblast & GaussRico featuring a small ChainSaw addendum
                        if (gauge.IsOverheated && LevelChecked(HeatBlast) && IsEnabled(CustomComboPreset.MCH_ST_Adv_HeatBlast))
                        {
                            if (CanWeave(actionID, 0.6) && wildfireCDTime > 2 && IsEnabled(CustomComboPreset.MCH_ST_Adv_GaussRicochet)) //check to see if this prevents Gauss/Rico from weaving on reopener deaths later
                            {
                                if (HasCharges(GaussRound) && (!LevelChecked(Ricochet) || GetCooldownRemainingTime(GaussRound) < GetCooldownRemainingTime(Ricochet)))
                                    return GaussRound;

                                else if (ActionReady(Ricochet))
                                    return Ricochet;
                            }

                            if ((GetCooldownRemainingTime(ChainSaw) <= 1 || IsOffCooldown(ChainSaw)) && (wildfireCDTime < 3 || IsOffCooldown(Wildfire)) && ChainSaw.LevelChecked() && IsEnabled(CustomComboPreset.MCH_ST_Adv_ChainSaw))
                                return ChainSaw;

                            return HeatBlast;
                        }

                        //HYPERCHARGE!!
                        if (IsEnabled(CustomComboPreset.MCH_ST_Adv_Hypercharge) && gauge.Heat >= 50 && LevelChecked(Hypercharge) && !gauge.IsOverheated)
                        {
                            //Tries to ensure the HC > CS > WF loop for the back-to-back HC loops in full uptime fights.

                            if (LevelChecked(Drill) && GetCooldownRemainingTime(Drill) >= 8)
                            {
                                if (LevelChecked(AirAnchor) && GetCooldownRemainingTime(AirAnchor) >= 8)
                                {
                                    if (LevelChecked(ChainSaw) && GetCooldownRemainingTime(ChainSaw) <= 2 && (wildfireCDTime <= 4 || IsOffCooldown(Wildfire)))
                                    {
                                        if (CanDelayedWeave(actionID) && UseHypercharge123Tools(gauge, wildfireCDTime))
                                            return Hypercharge;
                                    }
                                    else if (LevelChecked(ChainSaw) && GetCooldownRemainingTime(ChainSaw) >= 8)
                                    {
                                        if (CanWeave(actionID) && UseHypercharge123Tools(gauge, wildfireCDTime))
                                            return Hypercharge;
                                    }
                                    else if (!LevelChecked(ChainSaw))
                                    {
                                        if (CanWeave(actionID) && UseHypercharge123Tools(gauge, wildfireCDTime))
                                            return Hypercharge;
                                    }
                                }
                                else if (!LevelChecked(AirAnchor))
                                {
                                    if (CanWeave(actionID) && UseHypercharge123Tools(gauge, wildfireCDTime))
                                        return Hypercharge;
                                }
                            }
                            else if (!LevelChecked(Drill))
                            {
                                if (CanWeave(actionID) && UseHypercharge123Tools(gauge, wildfireCDTime))
                                    return Hypercharge;
                            }
                        }

                        if (ReassembledTools(ref actionID))
                            return actionID;
                    }

                    //Early Tools Rotation
                    if (rotationSelection is 2)
                    {
                        if (IsEnabled(CustomComboPreset.MCH_ST_Adv_Interrupt) && interruptReady)
                            return All.HeadGraze;

                        // BarrelStabilizer use
                        if (IsEnabled(CustomComboPreset.MCH_ST_Adv_Stabilizer) &&
                            CanWeave(actionID) && gauge.Heat <= 55 && ActionReady(BarrelStabilizer) &&
                            ((((wildfireCDTime <= 25 && wildfireCDTime >= 100) || HasEffect(Buffs.Wildfire)) && IsEnabled(CustomComboPreset.MCH_ST_Adv_Stabilizer_Wildfire_Only)) ||
                            (wildfireCDTime >= 110 && !IsEnabled(CustomComboPreset.MCH_ST_Adv_Stabilizer_Wildfire_Only))))
                            return BarrelStabilizer;

                        //Wildfire stuff
                        //these try to ensure the correct loop, 1/2/3 > HC > WF
                        if (ActionReady(Wildfire) && IsEnabled(CustomComboPreset.MCH_ST_Adv_WildFire))
                        {
                            if (CanDelayedWeave(actionID, 0.8) &&
                            (WasLastWeaponskill(HeatedSplitShot) || WasLastWeaponskill(HeatedSlugshot) || WasLastWeaponskill(HeatedCleanShot)))
                                return Wildfire;

                            else if (CanWeave(actionID) && gauge.IsOverheated)
                                return Wildfire;
                        }

                        //Queen aka Robot
                        if (CanWeave(actionID) && IsEnabled(CustomComboPreset.MCH_Adv_TurretQueen) && Config.MCH_ST_TurretUsage == 1 &&
                            !gauge.IsRobotActive && !WasLastAbility(Wildfire) && OriginalHook(RookAutoturret).LevelChecked())
                        {
                            // First condition
                            if (gauge.Battery == 70 && CombatEngageDuration().TotalSeconds > 61 && CombatEngageDuration().TotalSeconds < 68)
                                return OriginalHook(RookAutoturret);

                            // Second condition
                            if (!WasLastAction(OriginalHook(CleanShot)) &&
                                gauge.Battery >= 90 && gauge.LastSummonBatteryPower == 70)
                                return OriginalHook(RookAutoturret);

                            // Third condition
                            if (gauge.LastSummonBatteryPower >= 90 && gauge.Battery >= 90)
                                return OriginalHook(RookAutoturret);

                            // Fourth condition
                            while (gauge.LastSummonBatteryPower != 50 && gauge.Battery == 100)
                                return OriginalHook(RookAutoturret);

                            // Fifth condition
                            while (gauge.LastSummonBatteryPower == 100 && gauge.Battery >= 90) //was previously 80 with 30 overcap for 10mins
                                return OriginalHook(RookAutoturret);
                        }

                        if (IsEnabled(CustomComboPreset.MCH_Adv_TurretQueen) &&
                            Config.MCH_ST_TurretUsage == 0 &&
                            LevelChecked(OriginalHook(RookAutoturret)) && gauge.Battery >= 50 && !gauge.IsRobotActive)
                            return OriginalHook(RookAutoturret);

                        //Overheated & Heatblast & GaussRico
                        if (gauge.IsOverheated && LevelChecked(HeatBlast))
                        {
                            if (CanWeave(actionID, 0.6) && IsEnabled(CustomComboPreset.MCH_ST_Adv_GaussRicochet))
                            {
                                if (ActionReady(GaussRound) && (!LevelChecked(Ricochet) || GetCooldownRemainingTime(GaussRound) < GetCooldownRemainingTime(Ricochet)))
                                    return GaussRound;

                                else if (ActionReady(Ricochet))
                                    return Ricochet;
                            }
                            if (IsEnabled(CustomComboPreset.MCH_ST_Adv_HeatBlast))
                                return HeatBlast;
                        }

                        //HYPERCHARGE!!
                        if (IsEnabled(CustomComboPreset.MCH_ST_Adv_Hypercharge) &&
                            gauge.Heat >= 50 && ActionReady(Hypercharge) && !gauge.IsOverheated && CanWeave(actionID))
                        {
                            //Protection & ensures Hyper charged is double weaved with WF during reopener (12/19/2023, don't think this is needed anymore)
                            //if (HasEffect(Buffs.Wildfire) || level < Levels.Wildfire) return Hypercharge;

                            if (LevelChecked(Drill) && GetCooldownRemainingTime(Drill) >= 8)
                            {
                                if (LevelChecked(AirAnchor) && GetCooldownRemainingTime(AirAnchor) >= 8)
                                {
                                    if (LevelChecked(ChainSaw) && GetCooldownRemainingTime(ChainSaw) >= 8)
                                    {
                                        if (UseHyperchargeEarlyRotation(gauge, wildfireCDTime))
                                            return Hypercharge;
                                    }

                                    else if (!LevelChecked(ChainSaw))
                                    {
                                        if (UseHyperchargeEarlyRotation(gauge, wildfireCDTime))
                                            return Hypercharge;
                                    }
                                }

                                else if (!LevelChecked(AirAnchor))
                                {
                                    if (UseHyperchargeEarlyRotation(gauge, wildfireCDTime))
                                        return Hypercharge;
                                }
                            }

                            else if (!LevelChecked(Drill))
                            {
                                if (UseHyperchargeEarlyRotation(gauge, wildfireCDTime))
                                    return Hypercharge;
                            }
                        }

                        if (ReassembledTools(ref actionID))
                            return actionID;

                    }

                    //gauss and ricochet overcap protection
                    if (IsEnabled(CustomComboPreset.MCH_ST_Adv_GaussRicochet) &&
                        CanWeave(actionID) && !gauge.IsOverheated && !HasEffect(Buffs.Wildfire) &&
                        ActionWatching.GetAttackType(ActionWatching.LastAction) != ActionWatching.ActionAttackType.Ability)
                    {
                        if (level >= Levels.Ricochet && HasCharges(Ricochet))
                            return Ricochet;
                        if (level >= Levels.GaussRound && HasCharges(GaussRound))
                            return GaussRound;
                    }

                    // healing
                    if (IsEnabled(CustomComboPreset.MCH_ST_Adv_SecondWind) &&
                        CanWeave(actionID, 0.6) && PlayerHealthPercentageHp() <= Config.MCH_ST_SecondWindThreshold && ActionReady(All.SecondWind))
                        return All.SecondWind;

                    //1-2-3 Combo
                    if (comboTime > 0)
                    {
                        if (lastComboMove is SplitShot && LevelChecked(OriginalHook(SlugShot)))
                            return OriginalHook(SlugShot);

                        if (IsEnabled(CustomComboPreset.MCH_ST_Adv_Reassemble) && Config.MCH_ST_Reassembled[3] &&
                            !LevelChecked(Drill) && !HasEffect(Buffs.Reassembled) && HasCharges(Reassemble) && lastComboMove is SlugShot)
                            return Reassemble;

                        if (lastComboMove is SlugShot && LevelChecked(OriginalHook(CleanShot)))
                            return OriginalHook(CleanShot);
                    }

                    return OriginalHook(SplitShot);
                }

                return actionID;
            }

            private static bool ReassembledTools(ref uint actionId)
            {

                bool reassembledAnchor = (IsEnabled(CustomComboPreset.MCH_ST_Adv_Reassemble) && Config.MCH_ST_Reassembled[0] && HasEffect(Buffs.Reassembled)) || (IsEnabled(CustomComboPreset.MCH_ST_Adv_Reassemble) && !Config.MCH_ST_Reassembled[0] && !HasEffect(Buffs.Reassembled)) || (!HasEffect(Buffs.Reassembled) && GetRemainingCharges(Reassemble) <= Config.MCH_ST_ReassemblePool) || (!IsEnabled(CustomComboPreset.MCH_ST_Adv_Reassemble));
                bool reassembledDrill = (IsEnabled(CustomComboPreset.MCH_ST_Adv_Reassemble) && Config.MCH_ST_Reassembled[1] && HasEffect(Buffs.Reassembled)) || (IsEnabled(CustomComboPreset.MCH_ST_Adv_Reassemble) && !Config.MCH_ST_Reassembled[1] && !HasEffect(Buffs.Reassembled)) || (!HasEffect(Buffs.Reassembled) && GetRemainingCharges(Reassemble) <= Config.MCH_ST_ReassemblePool) || (!IsEnabled(CustomComboPreset.MCH_ST_Adv_Reassemble));
                bool reassembledChainsaw = (IsEnabled(CustomComboPreset.MCH_ST_Adv_Reassemble) && Config.MCH_ST_Reassembled[2] && HasEffect(Buffs.Reassembled)) || (IsEnabled(CustomComboPreset.MCH_ST_Adv_Reassemble) && !Config.MCH_ST_Reassembled[2] && !HasEffect(Buffs.Reassembled)) || (!HasEffect(Buffs.Reassembled) && GetRemainingCharges(Reassemble) <= Config.MCH_ST_ReassemblePool) || (!IsEnabled(CustomComboPreset.MCH_ST_Adv_Reassemble));
                // TOOLS!! ChainSaw Drill Air Anchor
                if (IsEnabled(CustomComboPreset.MCH_ST_Adv_Reassemble) && !HasEffect(Buffs.Wildfire) &&
                    !HasEffect(Buffs.Reassembled) && HasCharges(Reassemble) &&

                    GetRemainingCharges(Reassemble) > Config.MCH_ST_ReassemblePool &&
                    ((GetCooldownRemainingTime(OriginalHook(HotShot)) < 1 && Config.MCH_ST_Reassembled[0] && AirAnchor.LevelChecked()) ||
                    (GetCooldownRemainingTime(OriginalHook(Drill)) < 1 && Config.MCH_ST_Reassembled[1] && Drill.LevelChecked()) ||
                    (GetCooldownRemainingTime(OriginalHook(ChainSaw)) < 1 && Config.MCH_ST_Reassembled[2]) && ChainSaw.LevelChecked()))
                {
                    actionId = Reassemble;
                    return true;
                }

                if (IsEnabled(CustomComboPreset.MCH_ST_Adv_ChainSaw) &&
                    reassembledChainsaw &&
                    ChainSaw.LevelChecked() &&
                    (GetCooldownRemainingTime(ChainSaw) < 1.4 || ActionReady(ChainSaw))) //1.4sec added here for safety during reopeners and may ensure WF lateweave
                {
                    actionId = ChainSaw;
                    return true;
                }

                if (IsEnabled(CustomComboPreset.MCH_ST_Adv_AirAnchor) &&
                    reassembledAnchor &&
                    LevelChecked(OriginalHook(AirAnchor)) &&
                    (GetCooldownRemainingTime(OriginalHook(AirAnchor)) < 1 || ActionReady(OriginalHook(AirAnchor))))
                {
                    actionId = OriginalHook(AirAnchor);
                    return true;
                }

                if (IsEnabled(CustomComboPreset.MCH_ST_Adv_Drill) &&
                    reassembledDrill &&
                    Drill.LevelChecked() &&
                    (GetCooldownRemainingTime(Drill) < 1 || ActionReady(Drill)))
                {
                    actionId = Drill;
                    return true;
                }

                return false;
            }

            private bool UseHyperchargeDelayedTools(MCHGauge gauge, float wildfireCDTime)
            {
                if (CombatEngageDuration().Minutes == 0 && (gauge.Heat == 60 || CombatEngageDuration().Seconds <= 33))
                    return true;

                if (CombatEngageDuration().Minutes > 0)
                {
                    if (gauge.Heat >= 55 && wildfireCDTime > 25)
                        return true;

                    if (gauge.Heat >= 50 && wildfireCDTime <= 25 && wildfireCDTime >= 1)
                        return false;

                    if (gauge.Heat >= 55)
                        return true;
                }

                return false;
            }

            private bool UseHypercharge123Tools(MCHGauge gauge, float wildfireCDTime)
            {
                if (CombatEngageDuration().Minutes == 0 && (gauge.Heat >= 60 || CombatEngageDuration().Seconds <= 30) && !WasLastWeaponskill(OriginalHook(CleanShot)))
                    return true;

                if (CombatEngageDuration().Minutes > 0)
                {
                    if (gauge.Heat >= 50 && GetCooldownRemainingTime(ChainSaw) <= 1 && (wildfireCDTime <= 4 || IsOffCooldown(Wildfire)))
                        return true;

                    if (gauge.Heat >= 50 && wildfireCDTime <= 38 && wildfireCDTime >= 4)
                        return false;

                    if (gauge.Heat >= 55)
                        return true;

                    if (gauge.Heat >= 50 && wildfireCDTime >= 99)
                        return true;
                }

                return false;
            }

            private bool UseHyperchargeEarlyRotation(MCHGauge gauge, float wildfireCDTime)
            {
                if (CombatEngageDuration().Minutes == 0 && (gauge.Heat >= 50 || CombatEngageDuration().Seconds <= 30) && WasLastWeaponskill(HeatedSplitShot))
                    return true;

                if (CombatEngageDuration().Minutes > 0)
                {
                    if (gauge.Heat >= 50 && wildfireCDTime <= 36 && wildfireCDTime >= 1)
                        return false;

                    if (gauge.Heat >= 60)
                        return true;

                    if (gauge.Heat >= 50 && wildfireCDTime >= 99)
                        return true;
                }

                return false;
            }
        }

        internal class MCH_AoE_SimpleMode : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.MCH_AoE_SimpleMode;

            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                if (actionID is SpreadShot)
                {
                    MCHGauge? gauge = GetJobGauge<MCHGauge>();

                    if (IsEnabled(CustomComboPreset.MCH_Variant_Cure) &&
                     IsEnabled(Variant.VariantCure) && PlayerHealthPercentageHp() <= GetOptionValue(Config.MCH_VariantCure))
                        return Variant.VariantCure;

                    if (IsEnabled(CustomComboPreset.MCH_Variant_Rampart) &&
                        IsEnabled(Variant.VariantRampart) &&
                        IsOffCooldown(Variant.VariantRampart) &&
                        CanWeave(actionID))
                        return Variant.VariantRampart;

                    if (!gauge.IsOverheated)
                    {
                        if (gauge.Battery == 100)
                            return OriginalHook(RookAutoturret);
                    }

                    //gauss and ricochet overcap protection
                    if (CanWeave(actionID) && !gauge.IsOverheated)
                    {
                        if (ActionReady(GaussRound) && GetRemainingCharges(GaussRound) >= GetMaxCharges(GaussRound))
                            return GaussRound;

                        if (ActionReady(Ricochet) && GetRemainingCharges(Ricochet) >= GetMaxCharges(Ricochet))
                            return Ricochet;
                    }

                    // Hypercharge        
                    if (gauge.Heat >= 50 && LevelChecked(Hypercharge) && !gauge.IsOverheated)
                        return Hypercharge;

                    //Heatblast, Gauss, Rico
                    if (gauge.IsOverheated && LevelChecked(AutoCrossbow))
                    {
                        if (WasLastAction(AutoCrossbow) && CanWeave(actionID))
                        {
                            if (ActionReady(GaussRound) && GetRemainingCharges(GaussRound) >= GetRemainingCharges(Ricochet))
                                return GaussRound;

                            if (ActionReady(Ricochet) && GetRemainingCharges(Ricochet) >= GetRemainingCharges(GaussRound))
                                return Ricochet;
                        }
                        return AutoCrossbow;
                    }

                    if (ActionReady(BioBlaster) && !HasEffect(Buffs.Overheated) && IsEnabled(CustomComboPreset.MCH_AoE_Adv_Bioblaster))
                        return BioBlaster;

                    if (CanWeave(actionID, 0.6) && PlayerHealthPercentageHp() <= 20 && ActionReady(All.SecondWind))
                        return All.SecondWind;
                }

                return actionID;
            }
        }

        internal class MCH_AoE_AdvancedMode : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.MCH_AoE_AdvancedMode;

            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                if (actionID is SpreadShot or Scattergun)
                {
                    MCHGauge? gauge = GetJobGauge<MCHGauge>();

                    bool reassembledScattergun = (IsEnabled(CustomComboPreset.MCH_AoE_Adv_Reassemble) && Config.MCH_AoE_Reassembled[0] && HasEffect(Buffs.Reassembled));
                    bool reassembledCrossbow = (IsEnabled(CustomComboPreset.MCH_AoE_Adv_Reassemble) && Config.MCH_AoE_Reassembled[1] && HasEffect(Buffs.Reassembled)) || (IsEnabled(CustomComboPreset.MCH_AoE_Adv_Reassemble) && !Config.MCH_AoE_Reassembled[1] && !HasEffect(Buffs.Reassembled)) || (!IsEnabled(CustomComboPreset.MCH_AoE_Adv_Reassemble));
                    bool reassembledChainsaw = (IsEnabled(CustomComboPreset.MCH_AoE_Adv_Reassemble) && Config.MCH_AoE_Reassembled[2] && HasEffect(Buffs.Reassembled)) || (IsEnabled(CustomComboPreset.MCH_AoE_Adv_Reassemble) && !Config.MCH_AoE_Reassembled[2] && !HasEffect(Buffs.Reassembled)) || (!HasEffect(Buffs.Reassembled) && GetRemainingCharges(Reassemble) <= Config.MCH_AoE_ReassemblePool) || (!IsEnabled(CustomComboPreset.MCH_AoE_Adv_Reassemble));

                    if (IsEnabled(CustomComboPreset.MCH_Variant_Cure) &&
                     IsEnabled(Variant.VariantCure) && PlayerHealthPercentageHp() <= GetOptionValue(Config.MCH_VariantCure))
                        return Variant.VariantCure;

                    if (HasEffect(Buffs.Flamethrower) || JustUsed(Flamethrower))
                        return OriginalHook(11);

                    if (IsEnabled(CustomComboPreset.MCH_Variant_Rampart) &&
                        IsEnabled(Variant.VariantRampart) &&
                        IsOffCooldown(Variant.VariantRampart) &&
                        CanWeave(actionID))
                        return Variant.VariantRampart;

                    if (IsEnabled(CustomComboPreset.MCH_AoE_Adv_Reassemble) && !HasEffect(Buffs.Wildfire) &&
                        !HasEffect(Buffs.Reassembled) && HasCharges(Reassemble) &&
                        GetRemainingCharges(Reassemble) > Config.MCH_AoE_ReassemblePool &&
                        ((Config.MCH_AoE_Reassembled[0] && Scattergun.LevelChecked()) ||
                        (gauge.IsOverheated && Config.MCH_AoE_Reassembled[1] && AutoCrossbow.LevelChecked()) ||
                        (GetCooldownRemainingTime(OriginalHook(ChainSaw)) < 1 && Config.MCH_AoE_Reassembled[2] && ChainSaw.LevelChecked())))
                        return Reassemble;

                    if (IsEnabled(CustomComboPreset.MCH_AoE_Adv_Chainsaw) &&
                        reassembledChainsaw &&
                        ((LevelChecked(ChainSaw) && GetCooldownRemainingTime(ChainSaw) < 1) ||
                        ActionReady(ChainSaw)))
                        return ChainSaw;

                    if (reassembledScattergun)
                        return OriginalHook(Scattergun);

                    if (reassembledCrossbow &&
                        LevelChecked(AutoCrossbow) && gauge.IsOverheated)
                        return AutoCrossbow;

                    if (IsEnabled(CustomComboPreset.MCH_AoE_Adv_Bioblaster) && ActionReady(BioBlaster))
                        return OriginalHook(BioBlaster);

                    if (IsEnabled(CustomComboPreset.MCH_AoE_Adv_FlameThrower) && ActionReady(Flamethrower) && !IsMoving)
                        return OriginalHook(Flamethrower);

                    if (IsEnabled(CustomComboPreset.MCH_AoE_Adv_Queen) && !gauge.IsOverheated)
                    {
                        if (gauge.Battery >= Config.MCH_AoE_TurretUsage)
                            return OriginalHook(RookAutoturret);
                    }

                    // Hypercharge        
                    if (IsEnabled(CustomComboPreset.MCH_AoE_Adv_Hypercharge) &&
                        gauge.Heat >= 50 && LevelChecked(Hypercharge) && LevelChecked(AutoCrossbow) && !gauge.IsOverheated &&
                        ((BioBlaster.LevelChecked() && GetCooldownRemainingTime(BioBlaster) > 10) || !BioBlaster.LevelChecked() || IsNotEnabled(CustomComboPreset.MCH_AoE_Adv_Bioblaster)) &&
                        ((Flamethrower.LevelChecked() && GetCooldownRemainingTime(Flamethrower) > 10) || !Flamethrower.LevelChecked() || IsNotEnabled(CustomComboPreset.MCH_AoE_Adv_FlameThrower)))
                        return Hypercharge;

                    //Heatblast, Gauss, Rico
                    if (IsEnabled(CustomComboPreset.MCH_AoE_Adv_GaussRicochet) && CanWeave(actionID) &&
                        (Config.MCH_AoE_Hypercharge || (!Config.MCH_AoE_Hypercharge && gauge.IsOverheated)))
                    {
                        if ((WasLastAction(SpreadShot) || WasLastAction(AutoCrossbow) || Config.MCH_AoE_Hypercharge) && ActionWatching.GetAttackType(ActionWatching.LastAction) != ActionWatching.ActionAttackType.Ability)
                        {

                            if (ActionReady(Ricochet) && GetRemainingCharges(Ricochet) > 0)
                                return Ricochet;

                            if (ActionReady(Ricochet) && GetRemainingCharges(GaussRound) > 0)
                                return GaussRound;

                            if (LevelChecked(Ricochet) && GetRemainingCharges(Ricochet) > GetRemainingCharges(GaussRound))
                                return Ricochet;
                        }
                    }

                    if (gauge.IsOverheated && AutoCrossbow.LevelChecked())
                        return OriginalHook(AutoCrossbow);

                    if (IsEnabled(CustomComboPreset.MCH_AoE_Adv_SecondWind) && CanWeave(actionID, 0.6))
                    {
                        if (PlayerHealthPercentageHp() <= Config.MCH_AoE_SecondWindThreshold && ActionReady(All.SecondWind))
                            return All.SecondWind;
                    }
                }

                return actionID;
            }
        }

        /*internal class MCH_HeatblastGaussRicochet : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.MCH_Heatblast;

            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                MCHGauge? gauge = GetJobGauge<MCHGauge>();

                if (actionID is HeatBlast)
                {

                    if (IsEnabled(CustomComboPreset.MCH_Heatblast_AutoBarrel) && 
                        ActionReady(BarrelStabilizer) && 
                        gauge.Heat < 50 && 
                        !gauge.IsOverheated)
                        return BarrelStabilizer;

                    if (IsEnabled(CustomComboPreset.MCH_Heatblast_Wildfire) && 
                        ActionReady(Hypercharge) && 
                        ActionReady(Wildfire) && 
                        gauge.Heat >= 50)
                        return Wildfire;

                    if (!gauge.IsOverheated && LevelChecked(Hypercharge) && gauge.Heat >= 50)
                        return Hypercharge;

                    if (gauge.IsOverheated)
                    {
                        if (CanWeave(actionID) && WasLastAction(HeatBlast) && ActionWatching.GetAttackType(ActionWatching.LastAction) != ActionWatching.ActionAttackType.Ability)
                        {
                            if (LevelChecked(GaussRound) && GetRemainingCharges(GaussRound) >= GetRemainingCharges(Ricochet))
                                return GaussRound;

                    if (IsEnabled(CustomComboPreset.MCH_Heatblast_GaussRound) && gauge.IsOverheated)
                    {
                        if (!LevelChecked(Ricochet))
                            return GaussRound;

                        if (GetCooldownRemainingTime(GaussRound) < GetCooldownRemainingTime(Ricochet))
                            return GaussRound;
                        return Ricochet;
                    }
                }
                return actionID;
            }
        }*/

        internal class MCH_GaussRoundRicochet : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.MCH_GaussRoundRicochet;

            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {

                if (actionID is GaussRound or Ricochet)
                {
                    if (ActionReady(GaussRound) && GetRemainingCharges(GaussRound) >= GetRemainingCharges(Ricochet))
                        return GaussRound;

                    if (GetRemainingCharges(GaussRound) >= GetRemainingCharges(Ricochet))
                        return GaussRound;
                    else if (GetRemainingCharges(Ricochet) > 0)
                        return Ricochet;
                }

                return actionID;
            }
        }

        internal class MCH_Overdrive : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.MCH_Overdrive;

            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                if (actionID is RookAutoturret or AutomatonQueen)
                {
                    MCHGauge? gauge = GetJobGauge<MCHGauge>();
                    if (gauge.IsRobotActive)
                        return OriginalHook(QueenOverdrive);
                }

                return actionID;
            }
        }

        internal class MCH_HotShotDrillChainSaw : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.MCH_HotShotDrillChainSaw;

            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                if (actionID is Drill || actionID is HotShot || actionID is AirAnchor || actionID is ChainSaw)
                {
                    if (LevelChecked(ChainSaw))
                        return CalcBestAction(actionID, ChainSaw, AirAnchor, Drill);

                    if (LevelChecked(AirAnchor))
                        return CalcBestAction(actionID, AirAnchor, Drill);

                    if (LevelChecked(Drill))
                        return CalcBestAction(actionID, Drill, HotShot);

                    return HotShot;
                }

                return actionID;
            }
        }

        internal class MCH_DismantleTactician : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.MCH_DismantleTactician;
            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                if (actionID is Dismantle
                    && (IsOnCooldown(Dismantle) || !LevelChecked(Dismantle))
                    && ActionReady(Tactician)
                    && !HasEffect(Buffs.Tactician))
                    return Tactician;

                return actionID;
            }
        }

        internal class MCH_AutoCrossbowGaussRicochet : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.MCH_AutoCrossbow;

            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                if (actionID is AutoCrossbow)
                {
                    MCHGauge? gauge = GetJobGauge<MCHGauge>();

                    if (IsEnabled(CustomComboPreset.MCH_AutoCrossbow_AutoBarrel) && 
                        ActionReady(BarrelStabilizer) && 
                        gauge.Heat < 50 && 
                        !gauge.IsOverheated) 
                        return BarrelStabilizer;

                    if (!gauge.IsOverheated && ActionReady(Hypercharge) && gauge.Heat >= 50)
                        return Hypercharge;


                    if (GetCooldownRemaining(HeatBlast) < 0.7 && LevelChecked(AutoCrossbow)) // prioritize autocrossbow
                        return AutoCrossbow;

                    if (IsEnabled(CustomComboPreset.MCH_AutoCrossbow_GaussRound) && gauge.IsOverheated)
                    {
                        if (!LevelChecked(Ricochet))
                            return GaussRound;
                        if (GetCooldownRemaining(GaussRound) < GetCooldownRemaining(Ricochet))
                            return GaussRound;
                        else
                            return Ricochet;
                    }
                }

                return actionID;
            }
        }

        internal class All_PRanged_Dismantle : CustomCombo

        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.All_PRanged_Dismantle;

            protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
            {
                if (actionID is Dismantle)
                    if (TargetHasEffectAny(Debuffs.Dismantled) && IsOffCooldown(Dismantle))
                        return OriginalHook(11);

                return actionID;
            }
        }
    }
}
