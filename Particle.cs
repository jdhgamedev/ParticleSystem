using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarForged.Content.Effects.Particles {

    public enum BlendMode { 
        AlphaBlend, 
        Additive, 
        NonPremultiplied
    }

    public abstract class Particle2 {

        protected Particle2() {
            Initialize();
        }

        public int timeLeft;

        // ==== Movement ================
        public Vector2 position;
        public Vector2 velocity;
        public float rotation = 0;
        public float scale;

        // ==== Drawing =================
        public virtual string texture => GetType().Namespace.Replace(".", "/") + "/" + GetType().Name;
        public virtual Texture2D sprite { get; set; }
        public Rectangle frame; // Coordinates of the sprite on the sprite sheet, Defaults to entire texture

        public float opacity = 1f;
        public Color color = Color.White; // Tint
        public Layer layer = Layer.BeforeProjectiles; // Default layer, check ParticleDrawInjection.cs for available Layers
        public BlendMode blendMode = BlendMode.AlphaBlend; // Default BlendState
        // WIP
        public bool animate = false; 
        public int totalFrames = 1;

        // ==== Misc ====================
        public bool priority = false; // Draw particle even if particle limit is surpassed (WIP)
        public bool collideWithTiles = false;

        // Sets the Particle's texture
        private void Initialize() {
            if (Main.netMode is not NetmodeID.Server && sprite is null) {
                try {
                    sprite = ModContent.Request<Texture2D>(texture).Value;
                    frame = new Rectangle(0, 0, (int)sprite.Width, (int)sprite.Height);
                }
                catch {
                    sprite = ModContent.Request<Texture2D>("StarForged/Content/Effects/Particles/DefaultParticle").Value;
                    Main.NewText("Texture Lookup Failure");
                }
            }
        }

        // Called once when the Particle is initialized 
        public virtual void OnSpawn() { }

        // Called once per frame while the particle is alive
        public virtual void Update() { }

        // Default Draw code, simply override for Custom Drawing
        public virtual void Draw(SpriteBatch spriteBatch) {
            opacity = (opacity < 0) ? 0 : (opacity > 1) ? 1 : opacity;
            spriteBatch.Draw(sprite, position - Main.screenPosition, frame, color * opacity, rotation, sprite.Size() * 0.5f, scale, SpriteEffects.None, 0f);
        }

        // Called once when the particle collides with a tile
        public virtual void OnTileCollide() { }

        // Called once when the Particle is Destroyed
        public virtual void OnDestroy() { }

        public void Destroy() { 
            OnDestroy();
            DefaultParticleManager._particlesToDestroy?.Add(this);
        }
    }
}
