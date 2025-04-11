﻿using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerrariaCells.Common.Items;

namespace TerrariaCells.Common.GlobalItems
{
    /// <summary>
    /// GlobalItem that applies a level to every damage-dealing item, handling relevant weapon scaling and
    /// </summary>
    public class TierSystemGlobalItem : GlobalItem
    {
        public const float damageLevelScaling = 1.10f;
        public const float knockbackLevelScaling = 1.125f;
        public const float attackSpeedLevelScaling = 0.075f;

        public int itemLevel = 1;

        public override bool InstancePerEntity => true;

        // Only apply item levels to weapons
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return lateInstantiation && entity.damage > 0
                || lateInstantiation && entity.shoot != ItemID.None;
        }

        public override void SetDefaults(Item item)
        {
            item.rare = itemLevel;
            Math.Clamp(item.rare, 0, 10);
        }

        public void AddLevels(Item item, int level)
        {
            itemLevel += level;
            SetDefaults(item);
        }

        public void SetLevel(Item item, int level)
        {
            itemLevel = level;
            SetDefaults(item);
        }

        // Modify overrides to set weapon stats based on item level
        public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
        {
            // Equation is found using Sorbet's example values.
            // Graph of tiers vs damage values: https://www.desmos.com/calculator/mz89u5adai
            damage *= MathF.Pow(damageLevelScaling, itemLevel - 2);
        }

        public override void ModifyWeaponKnockback(
            Item item,
            Player player,
            ref StatModifier knockback
        )
        {
            knockback *= 1 + (MathF.Sqrt(itemLevel - 1) * knockbackLevelScaling);
        }

        public override float UseSpeedMultiplier(Item item, Player player)
        {
            return 1 + (MathF.Sqrt(itemLevel - 1) * attackSpeedLevelScaling);
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            TerraCellsItemCategory id = InventoryManager.GetItemCategorization(item.netID);
            if (!(id == TerraCellsItemCategory.Weapon || id == TerraCellsItemCategory.Skill)) {
                return;
            }
            // Iterate through the list of tooltips so we can change vanilla tooltips
            foreach (TooltipLine tooltip in tooltips)
            {
                string[] numerals = [
                    "",
                    "I",
                    "II",
                    "III",
                    "IV",
                    "V",
                    "VI",
                    "VII",
                    "VIII",
                    "IX",
                    "X",
                ];
                // Alter vanilla tooltips here
                switch (tooltip.Name)
                {
                    case "ItemName":
                        // tooltip.Text += " [Tier " + itemLevel.ToString() + "]";
                        tooltip.Text += " " + numerals[itemLevel] + " ";
                        break;
                }
            }

            // Also add the tier at the end of the tooltip
            // tooltips.Add(new TooltipLine(Mod, "Tier", "[Tier " + itemLevel.ToString() + "]"));
        }

        public override void NetSend(Item item, BinaryWriter writer)
        {
            writer.Write(itemLevel);
        }

        public override void NetReceive(Item item, BinaryReader reader)
        {
            itemLevel = 0;
            AddLevels(item, reader.ReadInt32());
        }

        public override void SaveData(Item item, TagCompound tag)
        {
            tag["level"] = itemLevel;
        }

        public override void LoadData(Item item, TagCompound tag)
        {
            itemLevel = 0;
            AddLevels(item, tag.Get<int>("level"));
        }

        public override GlobalItem Clone(Item item, Item itemClone)
        {
            TierSystemGlobalItem myClone = (TierSystemGlobalItem)base.Clone(item, itemClone);

            myClone.itemLevel = itemLevel;

            return myClone;
        }
    }
}
