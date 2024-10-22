using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarForged.Content.Effects.Particles {

    // Every Available Draw Layer
    public enum Layer { 
        BeforeBackground,
        BeforeWalls,
        BeforeNonSolidTiles,
        BeforeNPCsBehindTiles,
        BeforeSolidTiles,
        BeforePlayersBehindNPCs,
        BeforeNPCs,
        BeforeProjectiles,
        BeforePlayers,
        BeforeItems,
        BeforeRain,
        BeforeGore,
        BeforeDust,
        BeforeInterface,
        AfterInterface,
    }

    public class ParticleDrawInjection : ModSystem {

        public delegate void Update();
        public static event Update OnUpdateDust;

        public delegate void Draw(Layer layer);

		public static event Draw OnLayerBeforeBackground;
		public static event Draw OnLayerBeforeWalls; // After background
		public static event Draw OnLayerBeforeNonSolidTiles; // After walls
		public static event Draw OnLayerBeforeNPCsBehindTiles; // After non-solid tiles
		public static event Draw OnLayerBeforeSolidTiles; // After NPCs behind tiles, like worms
		public static event Draw OnLayerBeforePlayersBehindNPCs; // After solid tiles
		public static event Draw OnLayerBeforeNPCs; // After player details behind NPCs
		public static event Draw OnLayerBeforeProjectiles; // After NPCS
		public static event Draw OnLayerBeforePlayers; // After projectiles
		public static event Draw OnLayerBeforeItems; // After players
		public static event Draw OnLayerBeforeRain; // After all items in world
		public static event Draw OnLayerBeforeGore; // After Rain
		public static event Draw OnLayerBeforeDust; // After Gore
		public static event Draw OnLayerBeforeInterface;
		public static event Draw OnLayerAfterInterface;

        public override void Load() {
            if (Main.dedServ)
                return;
            On_Dust.UpdateDust += UpdateAllBeforeDust;

            Main.QueueMainThreadAction(() => {
                
                On_Main.DrawSurfaceBG += LayerBeforeBackground;
                On_Main.DoDraw_WallsAndBlacks += LayerBeforeWalls;
                On_Main.DoDraw_Tiles_NonSolid += LayerBeforeNonSolidTiles;
                On_Main.DrawPlayers_BehindNPCs += LayerBeforePlayersBehindNPCs;
                On_Main.DrawNPCs += LayerBeforeNPCs;
                On_Main.DoDraw_Tiles_Solid += LayerBeforeSolidTiles;
                On_Main.DrawProjectiles += LayerBeforeProjectiles;
                On_Main.DrawPlayers_AfterProjectiles += LayerBeforePlayers;
                On_Main.DrawItems += LayerBeforeItems;
                On_Main.DrawRain += LayerBeforeRain;
                On_Main.DrawGore += LayerBeforeGore;
                On_Main.DrawDust += LayerBeforeDust;
                On_Main.DrawInterface += LayerOnInterface;
                
            });
        }

        public override void Unload() {
            OnUpdateDust = null;
            OnLayerBeforeBackground = null;
            OnLayerBeforeWalls = null;
            OnLayerBeforeNonSolidTiles = null;
            OnLayerBeforeSolidTiles = null;
            OnLayerBeforePlayersBehindNPCs = null;
            OnLayerBeforeNPCsBehindTiles = null;
            OnLayerBeforeNPCs = null;
            OnLayerBeforeProjectiles = null;
            OnLayerBeforePlayers = null;
            OnLayerBeforeItems = null;
            OnLayerBeforeRain = null;
            OnLayerBeforeGore = null;
            OnLayerBeforeDust = null;
            OnLayerBeforeInterface = null;
            OnLayerAfterInterface = null;
        }

        public static void GetClip(out Rectangle rectangle, out RasterizerState rasterizer) {
            rectangle = Main.graphics.GraphicsDevice.ScissorRectangle;
            rasterizer = Main.graphics.GraphicsDevice.RasterizerState;
        }

        public static void SetClip(Rectangle rectangle, RasterizerState rasterizer) {
            Main.graphics.GraphicsDevice.ScissorRectangle = rectangle;
            Main.graphics.GraphicsDevice.RasterizerState = rasterizer;
        }

        private void UpdateAllBeforeDust(On_Dust.orig_UpdateDust orig) {
            OnUpdateDust?.Invoke();

            orig();
        }

        private void LayerBeforeBackground(On_Main.orig_DrawSurfaceBG orig, Main self) {
            Main.spriteBatch.End();

            Matrix matrix = Main.BackgroundViewMatrix.TransformationMatrix;
            matrix.Translation -= Main.BackgroundViewMatrix.ZoomMatrix.Translation * new Vector3(1f, Main.BackgroundViewMatrix.Effects.HasFlag(SpriteEffects.FlipVertically) ? (-1f) : 1f, 1f);

            GetClip(out Rectangle rectangle, out RasterizerState rasterizer);

            OnLayerBeforeBackground?.Invoke(Layer.BeforeBackground);

            SetClip(rectangle, rasterizer);

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, matrix);

            orig(self);
        }

        private void LayerBeforeWalls(On_Main.orig_DoDraw_WallsAndBlacks orig, Main self) {
            Main.spriteBatch.End();

            GetClip(out Rectangle rectangle, out RasterizerState rasterizer);

            OnLayerBeforeWalls?.Invoke(Layer.BeforeWalls);

            SetClip(rectangle, rasterizer);

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            orig(self);
        }

        private void LayerBeforeNonSolidTiles(On_Main.orig_DoDraw_Tiles_NonSolid orig, Main self) {
            Main.spriteBatch.End();

            GetClip(out Rectangle rectangle, out RasterizerState rasterizer);

            OnLayerBeforeNonSolidTiles?.Invoke(Layer.BeforeNonSolidTiles);

            SetClip(rectangle, rasterizer);

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            orig(self);
        }

        private void LayerBeforeSolidTiles(On_Main.orig_DoDraw_Tiles_Solid orig, Main self) {
            GetClip(out Rectangle rectangle, out RasterizerState rasterizer);

            OnLayerBeforeSolidTiles?.Invoke(Layer.BeforeSolidTiles);

            SetClip(rectangle, rasterizer);

            orig(self);
        }

        private void LayerBeforePlayersBehindNPCs(On_Main.orig_DrawPlayers_BehindNPCs orig, Main self) {
            GetClip(out Rectangle rectangle, out RasterizerState rasterizer);

            OnLayerBeforePlayersBehindNPCs?.Invoke(Layer.BeforePlayersBehindNPCs);

            SetClip(rectangle, rasterizer);

            orig(self);
        }

        private void LayerBeforeNPCs(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles) {
            if (behindTiles) {
                Main.spriteBatch.End();

                GetClip(out Rectangle rectangle, out RasterizerState rasterizer);

                OnLayerBeforeNPCsBehindTiles?.Invoke(Layer.BeforeNPCsBehindTiles);

                SetClip(rectangle, rasterizer);

                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            else {
                Main.spriteBatch.End();

                GetClip(out Rectangle rectangle, out RasterizerState rasterizer);

                OnLayerBeforeNPCs?.Invoke(Layer.BeforeNPCs);

                SetClip(rectangle, rasterizer);

                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            orig(self, behindTiles);
        }

        private void LayerBeforeProjectiles(On_Main.orig_DrawProjectiles orig, Main self) {
            GetClip(out Rectangle rectangle, out RasterizerState rasterizer);

            OnLayerBeforeProjectiles?.Invoke(Layer.BeforeProjectiles);

            SetClip(rectangle, rasterizer);

            orig(self);
        }

        private void LayerBeforePlayers(On_Main.orig_DrawPlayers_AfterProjectiles orig, Main self) {
            GetClip(out Rectangle rectangle, out RasterizerState rasterizer);

            OnLayerBeforePlayers?.Invoke(Layer.BeforePlayers);

            SetClip(rectangle, rasterizer);

            orig(self);
        }

        private void LayerBeforeItems(On_Main.orig_DrawItems orig, Main self) {
            Main.spriteBatch.End();

            GetClip(out Rectangle rectangle, out RasterizerState rasterizer);

            OnLayerBeforeItems?.Invoke(Layer.BeforeItems);

            SetClip(rectangle, rasterizer);

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            orig(self);
        }

        private void LayerBeforeRain(On_Main.orig_DrawRain orig, Main self) {
            Main.spriteBatch.End();

            GetClip(out Rectangle rectangle, out RasterizerState rasterizer);

            OnLayerBeforeRain?.Invoke(Layer.BeforeRain);

            SetClip(rectangle, rasterizer);

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            orig(self);
        }

        private void LayerBeforeGore(On_Main.orig_DrawGore orig, Main self) {
            Main.spriteBatch.End();

            GetClip(out Rectangle rectangle, out RasterizerState rasterizer);

            OnLayerBeforeGore?.Invoke(Layer.BeforeGore);

            SetClip(rectangle, rasterizer);

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            orig(self);
        }

        private void LayerBeforeDust(On_Main.orig_DrawDust orig, Main self) {
            GetClip(out Rectangle rectangle, out RasterizerState rasterizer);

            OnLayerBeforeDust?.Invoke(Layer.BeforeDust);

            SetClip(rectangle, rasterizer);

            orig(self);
        }

        private void LayerOnInterface(On_Main.orig_DrawInterface orig, Main self, GameTime gameTime) {
            GetClip(out Rectangle rectangle, out RasterizerState rasterizer);

            OnLayerBeforeInterface?.Invoke(Layer.BeforeInterface);

            SetClip(rectangle, rasterizer);

            orig(self, gameTime);

            GetClip(out rectangle, out rasterizer);

            OnLayerAfterInterface?.Invoke(Layer.AfterInterface);

            SetClip(rectangle, rasterizer);
        }
    }
}