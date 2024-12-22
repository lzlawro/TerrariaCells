﻿using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Common.GlobalTiles
{
    public class VanillaReworksGlobalTile : GlobalTile
    {
        public override void Load()
        {
            On_Projectile.ExplodeTiles += On_Projectile_ExplodeTiles;
			On_Player.TileInteractionsUse += On_Player_TileInteractionsUse;
			On_Player.InInteractionRange += On_Player_InInteractionRange;
        }

		private bool On_Player_InInteractionRange(On_Player.orig_InInteractionRange orig, Player self, int interactX, int interactY, Terraria.DataStructures.TileReachCheckSettings settings)
		{
			Tile tile = Framing.GetTileSafely(interactX, interactY);
			if (ValidTiles.Contains(tile.TileType))
			{
				if (tile.TileType == TileID.TeleportationPylon)
				{
					settings.OverrideXReach = Systems.WorldPylonSystem.MAX_PYLON_RANGE;
					settings.OverrideYReach = Systems.WorldPylonSystem.MAX_PYLON_RANGE;
				}
				return orig.Invoke(self, interactX, interactY, settings);
			}
			return false;
		}

		private readonly ushort[] ValidTiles = new ushort[] {
			TileID.TeleportationPylon,
			TileID.GemLocks,
		};
		private void On_Player_TileInteractionsUse(On_Player.orig_TileInteractionsUse orig, Player self, int myX, int myY)
		{
			Tile tile = Framing.GetTileSafely(myX, myY);
			if (ValidTiles.Contains(tile.TileType))
			{
				orig.Invoke(self, myX, myY);
			}
		}

		// Prevent explosions from destroying tiles
		private void On_Projectile_ExplodeTiles(On_Projectile.orig_ExplodeTiles orig, Projectile self, Vector2 compareSpot, int radius, int minI, int maxI, int minJ, int maxJ, bool wallSplode)
        {
            return;
        }

        // Stop tiles from dropping any items when destroyed
        public override bool CanDrop(int i, int j, int type)
        {
            return false;
        }

    }
}
