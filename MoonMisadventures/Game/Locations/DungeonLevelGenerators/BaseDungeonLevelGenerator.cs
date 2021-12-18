using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MoonMisadventures.Game.Monsters;
using SpaceShared;
using StardewValley;

namespace MoonMisadventures.Game.Locations.DungeonLevelGenerators
{
    public abstract class BaseDungeonLevelGenerator
    {
        public abstract void Generate( AsteroidsDungeon location, ref Vector2 warpFromPrev, ref Vector2 warpFromNext );

        protected string GetNextLocationName( AsteroidsDungeon location )
        {
            return AsteroidsDungeon.BaseLocationName + ( location.level.Value + 1 );
        }

        protected string GetPreviousLocationName( AsteroidsDungeon location )
        {
            if ( location.level.Value == 1 )
                return "Custom_MP_MoonAsteroidsEntrance";
            return AsteroidsDungeon.BaseLocationName + ( location.level.Value - 1 );
        }

        protected void PlacePreviousWarp( AsteroidsDungeon location, int centerX, int groundY )
        {
            Log.Debug( "Placing previous warp @ " + centerX + ", " + groundY );
            location.setMapTile( centerX + -1, groundY - 2, 503, "Front", null, 2 );
            location.setMapTile( centerX +  0, groundY - 2, 504, "Front", null, 2 );
            location.setMapTile( centerX +  1, groundY - 2, 505, "Front", null, 2 );
            location.setMapTile( centerX + -1, groundY - 1, 532, "Front", null, 2 );
            location.setMapTile( centerX +  0, groundY - 1, 533, "Front", null, 2 );
            location.setMapTile( centerX +  1, groundY - 1, 534, "Front", null, 2 );
            location.setMapTile( centerX + -1, groundY - 0, 561, "Buildings", "AsteroidsWarpPrevious", 2 );
            location.setMapTile( centerX +  0, groundY - 0, 562, "Buildings", "AsteroidsWarpPrevious", 2 );
            location.setMapTile( centerX +  1, groundY - 0, 563, "Buildings", "AsteroidsWarpPrevious", 2 );
        }

        protected void PlaceNextWarp( AsteroidsDungeon location, int centerX, int groundY )
        {
            Log.Debug( "Placing next warp @ " + centerX + ", " + groundY );
            location.setMapTile( centerX + -1, groundY - 2, 503+9, "Front", null, 2 );
            location.setMapTile( centerX + 0, groundY - 2, 504+9, "Front", null, 2 );
            location.setMapTile( centerX + 1, groundY - 2, 505+9, "Front", null, 2 );
            location.setMapTile( centerX + -1, groundY - 1, 532+9, "Front", null, 2 );
            location.setMapTile( centerX + 0, groundY - 1, 533+9, "Front", null, 2 );
            location.setMapTile( centerX + 1, groundY - 1, 534+9, "Front", null, 2 );
            location.setMapTile( centerX + -1, groundY - 0, 561+9, "Buildings", "AsteroidsWarpNext", 2 );
            location.setMapTile( centerX + 0, groundY - 0, 562+9, "Buildings", "AsteroidsWarpNext", 2 );
            location.setMapTile( centerX + 1, groundY - 0, 563+9, "Buildings", "AsteroidsWarpNext", 2 );
        }

        protected void PlaceRandomTeleporterPair( AsteroidsDungeon location, Random rand, int centerX1, int groundY1, int centerX2, int groundY2, bool canInactive = true )
        {
            int num = rand.Next( 3 );
            bool active = true;
            if ( canInactive && rand.NextDouble() < 0.5 )
                active = false;

            PlaceTeleporter( location, rand, num, active, centerX1, groundY1, centerX2, groundY2 + 1 );
            PlaceTeleporter( location, rand, num, true, centerX2, groundY2, centerX1, groundY1 + 1 );
        }
        protected void PlaceTeleporter( AsteroidsDungeon location, Random rand, int num, bool active, int centerX, int groundY, int targetX, int targetY )
        {
            int offset = num * 27;

            Log.Debug( "Placing teleporter " + location.teleports.Count + " @ " + centerX + ", " + groundY + " to " + targetX + " " + targetY );
            if ( !active )
            {
                location.setMapTile( centerX, groundY - 2, offset + 0 + 8, "Front", null, 3 );
                location.setMapTile( centerX, groundY - 1, offset + 9 + 8, "Front", null, 3 );
                location.setMapTile( centerX, groundY - 0, offset + 18 + 8, "Buildings", "LunarTeleporterOffline " + location.teleports.Count, 3 );
            }
            else
            {
                int[] a = new int[] { 0, 1, 2, 3, 4, 5, 6, 7 };
                int[] b = new int[] { 9, 10, 11, 12, 13, 14, 15, 16 };
                int[] c = new int[] { 18, 19, 20, 21, 22, 23, 24, 25 };

                a = a.Select( x => x + offset ).ToArray();
                b = b.Select( x => x + offset ).ToArray();
                c = c.Select( x => x + offset ).ToArray();

                location.setMapTile( centerX, groundY - 2, 8, "Paths", null, 1 );
                location.setAnimatedMapTile( centerX, groundY - 2, a, 300, "Front", null, 3 );
                location.setAnimatedMapTile( centerX, groundY - 1, b, 300, "Front", null, 3 );
                location.setAnimatedMapTile( centerX, groundY - 0, c, 300, "Buildings", "LunarTeleporter " + location.teleports.Count, 3 );
            }
            location.teleports.Add( new Vector2( targetX * Game1.tileSize, targetY * Game1.tileSize ) );
        }

        protected List< Vector2 > MakeSmallAsteroid( Random r, int centerX, int centerY, int size )
        {
            // This isn't completely accurate or efficient, but good enough for now
            List< Vector2 > ret = new();

            List< Vector2 > outerTiles = new();
            ret.Add( new Vector2( centerX, centerY ) );
            outerTiles.Add( new Vector2( centerX, centerY ) );
            List< Vector2 > open = new();
            for ( --size; size > 0; )
            {
                var tile = outerTiles[ r.Next( outerTiles.Count ) ];
                var check = tile + new Vector2( -1, 0 );
                if ( !ret.Contains( check ) )
                    open.Add( check );
                check = tile + new Vector2( 1, 0 );
                if ( !ret.Contains( check ) )
                    open.Add( check );
                check = tile + new Vector2( 0, -1 );
                if ( !ret.Contains( check ) )
                    open.Add( check );
                check = tile + new Vector2( 0, 1 );
                if ( !ret.Contains( check ) )
                    open.Add( check );

                if ( open.Count == 0 )
                {
                    outerTiles.Remove( tile );
                    ++size;
                    continue;
                }

                ret.AddRange( open );
                size -= open.Count;
                outerTiles.AddRange( open );
                open.Clear();
            }

            return ret;
        }

        protected void PlaceMinableAt( AsteroidsDungeon location, Random rand, int sx, int sy )
        {
            double r = rand.NextDouble();
            if ( r < 0.6 )
            {
                location.netObjects.Add( new Vector2( sx, sy ), new StardewValley.Object( new Vector2( sx, sy ), rand.NextDouble() < 0.5 ? 846 : 847, 1 )
                {
                    Name = "Stone",
                    MinutesUntilReady = 12
                } );
            }
            else if ( r < 0.85 )
            {
                int[] ores = new int[] { 95, 849, 850, 764, 765, int.MaxValue, int.MaxValue, int.MaxValue };
                int[] breaks = new int[] { 15, 6, 8, 10, 12 };
                int ore_ = rand.Next( ores.Length );
                int ore = ores[ ore_ ];
                if ( ore == int.MaxValue )
                {
                    var obj = new DynamicGameAssets.Game.CustomObject( ( DynamicGameAssets.PackData.ObjectPackData ) DynamicGameAssets.Mod.Find( Items.MythiciteOreMinableId ) );
                    obj.Name = "Stone";
                    obj.MinutesUntilReady = 24;
                    location.netObjects.Add( new Vector2( sx, sy ), obj );
                }
                else
                {
                    location.netObjects.Add( new Vector2( sx, sy ), new StardewValley.Object( new Vector2( sx, sy ), ore, 1 )
                    {
                        Name = "Stone",
                        MinutesUntilReady = breaks[ ore_ ]
                    } );
                }
            }
            else if ( r < 0.95 )
            {
                int[] gems = new int[] { 2, 4, 6, 8, 10, 12, 14, 44, 44, 44, 46, 46 };
                int gem_ = rand.Next( gems.Length );
                int gem = gems[ gem_ ];
                location.netObjects.Add( new Vector2( sx, sy ), new StardewValley.Object( new Vector2( sx, sy ), gem, 1 )
                {
                    Name = "Stone",
                    MinutesUntilReady = 10
                } );
            }
            else
            {
                location.netObjects.Add( new Vector2( sx, sy ), new StardewValley.Object( new Vector2( sx, sy ), 819, 1 )
                {
                    Name = "Stone",
                    MinutesUntilReady = 10
                } );
            }
        }

        protected void PlaceMonsterAt( AsteroidsDungeon location, Random rand, int tx, int ty )
        {
            location.characters.Add( new BoomEye( new Vector2( tx * Game1.tileSize, ty * Game1.tileSize ) ) );
            // TODO: Place random assortment
        }
    }
}
