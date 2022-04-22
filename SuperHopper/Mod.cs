using Microsoft.Xna.Framework;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;
using SuperHopper.Patches;
using SObject = StardewValley.Object;
using System.Collections.Generic;
using System;


namespace SuperHopper
{
    internal class Mod : StardewModdingAPI.Mod
    {
        /*********
        ** Fields
        *********/
        /// <summary>The <see cref="Item.modData"/> flag which indicates a hopper is a super hopper.</summary>
        private readonly string ModDataFlag = "spacechase0.SuperHopper";
        private static List<Chest> junimoHoppersPush;
        private static List<Chest> junimoHoppersPull;

        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            Log.Monitor = this.Monitor;

            helper.Events.Input.ButtonPressed += this.OnButtonPressed;

            HarmonyPatcher.Apply(this,
                new ObjectPatcher(this.OnMachineMinutesElapsed)
            );
            
            junimoHoppersPush = new List<Chest>();
            junimoHoppersPull = new List<Chest>();
        }


        /*********
        ** Private methods
        *********/
        /// <inheritdoc cref="IInputEvents.ButtonPressed"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsPlayerFree)
                return;

            Game1.currentLocation.objects.TryGetValue(e.Cursor.GrabTile, out SObject obj);
            if (this.TryGetHopper(obj, out Chest hopper) && e.Button.IsActionButton())
            {
                if (hopper.heldObject.Value == null)
                {
                    if (Utility.IsNormalObjectAtParentSheetIndex(Game1.player.ActiveObject, SObject.iridiumBar))
                    {
                        hopper.Tint = Color.DarkViolet;
                        hopper.heldObject.Value = (SObject)Game1.player.ActiveObject.getOne();
                        hopper.modData[this.ModDataFlag] = "1";

                        if (Game1.player.ActiveObject.Stack > 1)
                            Game1.player.ActiveObject.Stack--;
                        else
                            Game1.player.ActiveObject = null;

                        Game1.playSound("furnace");
                    }
                }
                else if (Game1.player.CurrentItem == null)
                {
                    hopper.Tint = Color.White;
                    hopper.heldObject.Value = null;
                    hopper.modData.Remove(this.ModDataFlag);
                    
                    if (junimoHoppersPush.Contains(hopper)) {
                        junimoHoppersPush.Remove(hopper);
                    }
                    if (junimoHoppersPull.Contains(hopper)) {
                        junimoHoppersPull.Remove(hopper);
                    }

                    Game1.player.addItemToInventory(new SObject(SObject.iridiumBar, 1));

                    Game1.playSound("shiny4");
                }
            }
        }

        /// <summary>Called after a machine updates on time change.</summary>
        /// <param name="machine">The machine that updated.</param>
        /// <param name="location">The location containing the machine.</param>
        private void OnMachineMinutesElapsed(SObject machine, GameLocation location)
        {
            // not super hopper
            if (!this.TryGetHopper(machine, out Chest hopper) || hopper.heldObject.Value == null || !Utility.IsNormalObjectAtParentSheetIndex(hopper.heldObject.Value, SObject.iridiumBar))
                return;
            if (junimoHoppersPull.Contains(hopper))
            {
                hopper = junimoHoppersPull[0];
            }else if (junimoHoppersPush.Contains(hopper)){
                hopper = junimoHoppersPush[0];
            }
            bool turnPull = junimoHoppersPull.Count == 0 || !junimoHoppersPull.Contains(hopper) || junimoHoppersPull[0] == hopper;
            bool turnPush = junimoHoppersPush.Count == 0 || !junimoHoppersPush.Contains(hopper) || junimoHoppersPush[0] == hopper;

            Log.Info($"Starting {hopper} {hopper.TileLocation} turnPull:{turnPull} turnPush:{turnPush}");
            // fix flag if needed
            if (!hopper.modData.ContainsKey(this.ModDataFlag))
                hopper.modData[this.ModDataFlag] = "1";

            // no chests to transfer
            if (!location.objects.TryGetValue(hopper.TileLocation - new Vector2(0, 1), out SObject objAbove) ||
                objAbove is not Chest chestAbove)
            {
                if (junimoHoppersPull.Contains(hopper))
                {
                    junimoHoppersPull.Remove(hopper);
                }else if (junimoHoppersPush.Contains(hopper))
                {
                    junimoHoppersPush.Remove(hopper);
                }
                return;
            }
            if (!location.objects.TryGetValue(hopper.TileLocation + new Vector2(0, 1), out SObject objBelow) || objBelow is not Chest chestBelow)
            {
                if (junimoHoppersPull.Contains(hopper))
                {
                    junimoHoppersPull.Remove(hopper);
                }else if (junimoHoppersPush.Contains(hopper))
                {
                    junimoHoppersPush.Remove(hopper);
                }
                return;
            }
            
            bool PullingJunimo = objAbove is Chest { SpecialChestType: Chest.SpecialChestTypes.JunimoChest };
            bool PushingJunimo = objBelow is Chest { SpecialChestType: Chest.SpecialChestTypes.JunimoChest };
            
            Log.Info($"Junimo? PullingJunimo:{PullingJunimo} PushingJunimo:{PushingJunimo}");
            // transfer items
            chestAbove.clearNulls();

            bool moved = false;

            if ((turnPull || !PullingJunimo) && (turnPush || !PushingJunimo)) {
                for (int i = chestAbove.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Count - 1; (i >= 0 && ((!(PullingJunimo || PushingJunimo)) || !moved)); i--)
                {
                    Item item = chestAbove.GetItemsForPlayer(Game1.player.UniqueMultiplayerID)[i];
                    if (chestBelow.addItem(item) == null) { 
                        chestAbove.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).RemoveAt(i);
                        moved = true;
                    }
                }
            }
            if (PullingJunimo && turnPull && (!moved) && (!junimoHoppersPull.Contains(hopper))) {
                junimoHoppersPull.Add(hopper);
                Log.Info($"Add to pull");
            }
            if (PushingJunimo && !chestAbove.isEmpty() && turnPush && (!moved) && (!junimoHoppersPush.Contains(hopper))) {
                junimoHoppersPush.Add(hopper);
                Log.Info($"Add to push");
            }
            else if (moved){
                if (junimoHoppersPush.Contains(hopper)) junimoHoppersPush.Remove(hopper);
                if (PushingJunimo && !chestAbove.isEmpty()) junimoHoppersPush.Add(hopper);
                if (junimoHoppersPull.Contains(hopper)) junimoHoppersPull.Remove(hopper);
                if (PullingJunimo) junimoHoppersPull.Add(hopper);
                Log.Info("Add Bot");
            }else if (!moved && PullingJunimo && turnPull && !chestAbove.isEmpty())
            {
                if (junimoHoppersPull.Contains(hopper)) junimoHoppersPull.Remove(hopper);
                if (PullingJunimo) junimoHoppersPull.Add(hopper);
            }
        Log.Info("Push Queue");
        junimoHoppersPush.ForEach(i => Log.Info($"{i.TileLocation}"));
        Log.Info("Pull Queue");
        junimoHoppersPull.ForEach(i => Log.Info($"{i.TileLocation}"));
        Log.Info("===");


        }

        /// <summary>Get the hopper instance if the object is a hopper.</summary>
        /// <param name="obj">The object to check.</param>
        /// <param name="hopper">The hopper instance.</param>
        /// <returns>Returns whether the object is a hopper.</returns>
        private bool TryGetHopper(SObject obj, out Chest hopper)
        {
            if (obj is Chest { SpecialChestType: Chest.SpecialChestTypes.AutoLoader } chest)
            {
                hopper = chest;
                return true;
            }

            hopper = null;
            return false;
        }
    }
}
