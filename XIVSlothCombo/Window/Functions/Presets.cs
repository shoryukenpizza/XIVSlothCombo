﻿using Dalamud.Interface.Colors;
using Dalamud.Utility;
using ECommons.ImGuiMethods;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using System.Linq;
using System.Numerics;
using System.Text;
using XIVSlothCombo.Attributes;
using XIVSlothCombo.Combos;
using XIVSlothCombo.Combos.PvE;
using XIVSlothCombo.Core;
using XIVSlothCombo.Data;
using XIVSlothCombo.Extensions;
using XIVSlothCombo.Services;

namespace XIVSlothCombo.Window.Functions
{
    internal class Presets : ConfigWindow
    {
        internal unsafe static void DrawPreset(CustomComboPreset preset, CustomComboInfoAttribute info, ref int i)
        {
            var enabled = Service.Configuration.IsEnabled(preset);
            var secret = PluginConfiguration.IsSecret(preset);
            var conflicts = PluginConfiguration.GetConflicts(preset);
            var parent = PluginConfiguration.GetParent(preset);
            var blueAttr = preset.GetAttribute<BlueInactiveAttribute>();

            ImGui.PushItemWidth(200);

            if (ImGui.Checkbox($"{info.FancyName}###{info.FancyName}{i}", ref enabled))
            {
                if (enabled)
                {

                    EnableParentPresets(preset);
                    Service.Configuration.EnabledActions.Add(preset);
                    foreach (var conflict in conflicts)
                    {
                        Service.Configuration.EnabledActions.Remove(conflict);
                    }
                }

                else
                {
                    Service.Configuration.EnabledActions.Remove(preset);
                }

                Service.Configuration.Save();
            }

            ImGui.PopItemWidth();
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudGrey);

            DrawOpenerButtons(preset);

            Vector2 length = new();

            if (i != -1)
            {
                ImGui.Text($"#{i}: ");
                length = ImGui.CalcTextSize($"#{i}: ");
                ImGui.SameLine();
                ImGui.PushItemWidth(length.Length());
            }

            ImGui.TextWrapped($"{info.Description}");

            if (preset.GetHoverAttribute() != null)
            {
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.TextUnformatted(preset.GetHoverAttribute().HoverText);
                    ImGui.EndTooltip();
                }
            }


            ImGui.PopStyleColor();
            ImGui.Spacing();

            if (conflicts.Length > 0)
            {
                ImGui.TextColored(ImGuiColors.DalamudRed, "Conflicts with:");
                StringBuilder conflictBuilder = new();
                ImGui.Indent();
                foreach (var conflict in conflicts)
                {
                    var comboInfo = conflict.GetAttribute<CustomComboInfoAttribute>();
                    conflictBuilder.Insert(0, $"{comboInfo.FancyName}");
                    var par2 = conflict;

                    while (PluginConfiguration.GetParent(par2) != null)
                    {
                        var subpar = PluginConfiguration.GetParent(par2);
                        conflictBuilder.Insert(0, $"{subpar?.GetAttribute<CustomComboInfoAttribute>().FancyName} -> ");
                        par2 = subpar!.Value;

                    }

                    if (!string.IsNullOrEmpty(comboInfo.JobShorthand))
                    conflictBuilder.Insert(0, $"[{comboInfo.JobShorthand}] ");

                    ImGuiEx.Text(GradientColor.Get(ImGuiColors.DalamudRed, CustomComboNS.Functions.CustomComboFunctions.IsEnabled(conflict) ? ImGuiColors.HealerGreen : ImGuiColors.DalamudRed, 1500), $"- {conflictBuilder}");
                    conflictBuilder.Clear();
                }
                ImGui.Unindent();
                ImGui.Spacing();
            }

            if (blueAttr != null)
            {
                if (blueAttr.Actions.Count > 0)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, blueAttr.NoneSet ? ImGuiColors.DPSRed : ImGuiColors.DalamudOrange);
                    ImGui.Text($"{(blueAttr.NoneSet ? "No Required Spells Active:" : "Missing active spells:")} {string.Join(", ", blueAttr.Actions.Select(x => ActionWatching.GetActionName(x)))}");
                    ImGui.PopStyleColor();
                }

                else
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.HealerGreen);
                    ImGui.Text($"All required spells active!");
                    ImGui.PopStyleColor();
                }
            }

            VariantParentAttribute? varientparents = preset.GetAttribute<VariantParentAttribute>();
            if (varientparents is not null)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.HealerGreen);
                ImGui.TextWrapped($"Part of normal combo{(varientparents.ParentPresets.Length > 1 ? "s" : "")}:");
                StringBuilder builder = new();
                foreach (var par in varientparents.ParentPresets)
                {
                    builder.Insert(0, $"{par.GetAttribute<CustomComboInfoAttribute>().FancyName}");
                    var par2 = par;
                    while (PluginConfiguration.GetParent(par2) != null)
                    {
                        var subpar = PluginConfiguration.GetParent(par2);
                        builder.Insert(0, $"{subpar?.GetAttribute<CustomComboInfoAttribute>().FancyName} -> ");
                        par2 = subpar!.Value;

                    }

                    ImGui.TextWrapped($"- {builder}");
                    builder.Clear();
                }
                ImGui.PopStyleColor();
            }

            BozjaParentAttribute? bozjaparents = preset.GetAttribute<BozjaParentAttribute>();
            if (bozjaparents is not null)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.HealerGreen);
                ImGui.TextWrapped($"Part of normal combo{(varientparents.ParentPresets.Length > 1 ? "s" : "")}:");
                StringBuilder builder = new();
                foreach (var par in bozjaparents.ParentPresets)
                {
                    builder.Insert(0, $"{par.GetAttribute<CustomComboInfoAttribute>().FancyName}");
                    var par2 = par;
                    while (PluginConfiguration.GetParent(par2) != null)
                    {
                        var subpar = PluginConfiguration.GetParent(par2);
                        builder.Insert(0, $"{subpar?.GetAttribute<CustomComboInfoAttribute>().FancyName} -> ");
                        par2 = subpar!.Value;

                    }

                    ImGui.TextWrapped($"- {builder}");
                    builder.Clear();
                }
                ImGui.PopStyleColor();
            }

            EurekaParentAttribute? eurekaparents = preset.GetAttribute<EurekaParentAttribute>();
            if (eurekaparents is not null)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.HealerGreen);
                ImGui.TextWrapped($"Part of normal combo{(varientparents.ParentPresets.Length > 1 ? "s" : "")}:");
                StringBuilder builder = new();
                foreach (var par in eurekaparents.ParentPresets)
                {
                    builder.Insert(0, $"{par.GetAttribute<CustomComboInfoAttribute>().FancyName}");
                    var par2 = par;
                    while (PluginConfiguration.GetParent(par2) != null)
                    {
                        var subpar = PluginConfiguration.GetParent(par2);
                        builder.Insert(0, $"{subpar?.GetAttribute<CustomComboInfoAttribute>().FancyName} -> ");
                        par2 = subpar!.Value;

                    }

                    ImGui.TextWrapped($"- {builder}");
                    builder.Clear();
                }
                ImGui.PopStyleColor();
            }

            UserConfigItems.Draw(preset, enabled);

            if (preset == CustomComboPreset.NIN_ST_SimpleMode_BalanceOpener || preset == CustomComboPreset.NIN_ST_AdvancedMode_BalanceOpener)
            {
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + length.Length());
                if (ImGui.Button($"Image of rotation###ninrtn{i}"))
                {
                    Util.OpenLink("https://i.imgur.com/q3lXeSZ.png");
                }
            }

            i++;

            var hideChildren = Service.Configuration.HideChildren;
            var children = presetChildren[preset];

            if (children.Length > 0)
            {
                if (enabled || !hideChildren)
                {
                    ImGui.Indent();

                    foreach (var (childPreset, childInfo) in children)
                    {
                        if (Service.Configuration.HideConflictedCombos)
                        {
                            var conflictOriginals = PluginConfiguration.GetConflicts(childPreset);    // Presets that are contained within a ConflictedAttribute
                            var conflictsSource = PluginConfiguration.GetAllConflicts();              // Presets with the ConflictedAttribute

                            if (!conflictsSource.Where(x => x == childPreset || x == preset).Any() || conflictOriginals.Length == 0)
                            {
                                DrawPreset(childPreset, childInfo, ref i);
                                continue;
                            }

                            if (conflictOriginals.Any(x => Service.Configuration.IsEnabled(x)))
                            {
                                Service.Configuration.EnabledActions.Remove(childPreset);
                                Service.Configuration.Save();
                            }

                            else
                            {
                                DrawPreset(childPreset, childInfo, ref i);
                                continue;
                            }
                        }

                        else
                        {
                            DrawPreset(childPreset, childInfo, ref i);
                        }
                    }

                    ImGui.Unindent();
                }
                else
                {
                    i += AllChildren(presetChildren[preset]);

                }
            }
        }

        private static void DrawOpenerButtons(CustomComboPreset preset)
        {
            if (preset.GetReplaceAttribute() != null)
            {
                string skills = string.Join(", ", preset.GetReplaceAttribute().ActionNames);

                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.TextUnformatted($"Replaces: {skills}");
                    ImGui.EndTooltip();
                }
            }
        }

        internal static int AllChildren((CustomComboPreset Preset, CustomComboInfoAttribute Info)[] children)
        {
            var output = 0;

            foreach (var (Preset, Info) in children)
            {
                output++;
                output += AllChildren(presetChildren[Preset]);
            }

            return output;
        }



        /// <summary> Iterates up a preset's parent tree, enabling each of them. </summary>
        /// <param name="preset"> Combo preset to enabled. </param>
        private static void EnableParentPresets(CustomComboPreset preset)
        {
            var parentMaybe = PluginConfiguration.GetParent(preset);

            while (parentMaybe != null)
            {
                var parent = parentMaybe.Value;

                if (!Service.Configuration.EnabledActions.Contains(parent))
                {
                    Service.Configuration.EnabledActions.Add(parent);
                    foreach (var conflict in PluginConfiguration.GetConflicts(parent))
                    {
                        Service.Configuration.EnabledActions.Remove(conflict);
                    }
                }

                parentMaybe = PluginConfiguration.GetParent(parent);
            }
        }
    }
}
