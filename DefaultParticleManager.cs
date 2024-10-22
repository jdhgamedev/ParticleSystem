using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarForged.Content.Effects.Particles {
    
    public class DefaultParticleManager : ModSystem {

        /*
        * Planned Features:
        * 
        * Calamity-like priority particle system which will draw particles marked as "priority" over particles not marked when at the particle limit (defined in a config)
        * 
        * Animated Textures
        * 
        */

        // All instances of particles/emitters
        internal static List<Particle2> _particles;
        internal static List<Emitter> _emitters;
        // Particles/emitters to be destroyed next frame
        internal static List<Particle2> _particlesToDestroy;
        internal static List<Emitter> _emittersToDestroy;

        public static int totalParticleCount { get; internal set; }

        // Lists for Drawing
        private static List<Particle2> _additiveParticles;
        private static List<Particle2> _alphaBlendParticles;
        private static List<Particle2> _nonPremultipliedParticles;

        public override void OnModLoad() {
            _particles = new();
            _emitters = new();
            _particlesToDestroy = new();
            _emittersToDestroy = new();

            _additiveParticles = new();
            _alphaBlendParticles = new();
            _nonPremultipliedParticles = new();

            // Injection
            ParticleDrawInjection.OnUpdateDust += UpdateWorldParticles;
            Main.QueueMainThreadAction(() => {
                ParticleDrawInjection.OnLayerBeforeBackground += DrawParticles;
                ParticleDrawInjection.OnLayerBeforeWalls += DrawParticles;
                ParticleDrawInjection.OnLayerBeforeNonSolidTiles += DrawParticles;
                ParticleDrawInjection.OnLayerBeforePlayersBehindNPCs += DrawParticles;
                ParticleDrawInjection.OnLayerBeforeNPCs += DrawParticles;
                ParticleDrawInjection.OnLayerBeforeSolidTiles += DrawParticles;
                ParticleDrawInjection.OnLayerBeforeProjectiles += DrawParticles;
                ParticleDrawInjection.OnLayerBeforePlayers += DrawParticles;
                ParticleDrawInjection.OnLayerBeforeItems += DrawParticles;
                ParticleDrawInjection.OnLayerBeforeRain += DrawParticles;
                ParticleDrawInjection.OnLayerBeforeGore += DrawParticles;
                ParticleDrawInjection.OnLayerBeforeDust += DrawParticles;
                ParticleDrawInjection.OnLayerBeforeInterface += DrawParticles;
                ParticleDrawInjection.OnLayerAfterInterface += DrawParticles;
            });
        }

        public override void Unload() {
            _particles = null;
            _emitters = null;
            _particlesToDestroy = null;
            _emittersToDestroy = null;

            _additiveParticles = null;
            _alphaBlendParticles = null;
            _nonPremultipliedParticles = null;
        }

        public override void OnWorldLoad() { 
            ResetParticles();
        }

        private void UpdateWorldParticles() {
            Update();
        }

        public override void OnWorldUnload() {
            ResetParticles();
        }

        internal void ResetParticles() {
            _particles.Clear();
            _emitters.Clear();
            _particlesToDestroy.Clear();
            _emittersToDestroy.Clear();

            _additiveParticles.Clear();
            _alphaBlendParticles.Clear();
            _nonPremultipliedParticles.Clear();
        }

        public static void Update() {
            // Don't update particles if the game is paused or running as a server
            if (Main.gamePaused || Main.dedServ)
                return;

            // Remove particles/emitters that need to be destroyed
            if (_particlesToDestroy.Count > 0) {
                foreach (Particle2 particle in _particlesToDestroy) {
                    if (particle != null) {
                        _particles.Remove(particle);
                        totalParticleCount--;
                        //DEBUG:
                        //Main.NewText($"Particle Created: p,{position} : v,{velocity} : c, {color} : s,{scale} : l, {layer} : b,{blendMode}");
                    }
                }
                _particlesToDestroy.Clear();
            }
            if (_emittersToDestroy.Count > 0) {
                foreach (Emitter emitter in _emittersToDestroy) {
                    if (emitter != null) { 
                        _emitters.Remove(emitter);
                    }
                }
            }

            
            for (int i = _emitters.Count - 1; i >= 0; i--) {
                Emitter emitter = _emitters[i];
                if (emitter == null)
                    continue;

                emitter.Update();
            }

            for (int i = _particles.Count - 1; i >= 0; i--) {
                Particle2 particle = _particles[i];
                if (particle == null)
                    continue;

                // Update particle properties
                particle.position += particle.velocity;
                particle.timeLeft--;
                particle.Update();

                // Tile Collision
                if (particle.collideWithTiles) {
                    Vector2 scaledSize = particle.sprite.Size() * particle.scale;
                    if (Collision.SolidCollision(particle.position, (int)scaledSize.X, (int)scaledSize.Y, true)) {
                        particle.OnTileCollide();
                    }
                }

                // Flag expired particles to be destroyed
                if (particle.timeLeft <= 0) {
                    particle.OnDestroy();
                    _particlesToDestroy.Add(particle);
                }
            }
            //DEBUG:
            //Main.NewText($"Particle Count: {_particles.Count}");
        }

        public static void AddParticle(Particle2 particle) {
            if (Main.gamePaused || Main.dedServ || _particles == null)
                return;

            // TODO: Check for particle limit and don't add if not marked Priority
            
            particle.OnSpawn();
            totalParticleCount++;
            _particles.Add(particle);
        }

        public static void AddEmitter(Emitter emitter) {
            if (Main.gamePaused || Main.dedServ || _emitters == null)
                return;
            
            //DEBUG:
            //Main.NewText($"Emitter Added");
            _emitters.Add(emitter);
        }

        public static void DestroyEmitter(Emitter emitter) {
            if (Main.gamePaused || Main.dedServ || _emitters == null)
                return;
            
            //DEBUG:
            //Main.NewText($"Emitter Removed");
            emitter.OnDestroy();
            _emitters.Remove(emitter);
        }

        private void DrawParticles(Layer layer) {
            if (Main.dedServ || _particles.Count == 0)
                return;

            _additiveParticles.Clear();
            _alphaBlendParticles.Clear();
            _nonPremultipliedParticles.Clear();

            foreach (Particle2 particle in _particles) {
                if (particle == null || particle.layer != layer)
                    continue;

                // Categorize particles by blend mode
                switch (particle.blendMode) {
                    case BlendMode.Additive:
                        _additiveParticles.Add(particle);
                        break;
                    case BlendMode.AlphaBlend:
                        _alphaBlendParticles.Add(particle);
                        break;
                    case BlendMode.NonPremultiplied:
                        _nonPremultipliedParticles.Add(particle);
                        break;
                }
            }

            SpriteBatch spriteBatch = Main.spriteBatch;

            var rasterizer = Main.Rasterizer;
            rasterizer.ScissorTestEnable = true;
            Main.instance.GraphicsDevice.RasterizerState = rasterizer;
            Main.instance.GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);

            // Draw all particles by blend mode
            DrawParticlesByBlendMode(spriteBatch, _additiveParticles, BlendState.Additive, rasterizer);
            DrawParticlesByBlendMode(spriteBatch, _alphaBlendParticles, BlendState.AlphaBlend, rasterizer);
            DrawParticlesByBlendMode(spriteBatch, _nonPremultipliedParticles, BlendState.NonPremultiplied, rasterizer);
        }

        private void DrawParticlesByBlendMode(SpriteBatch spriteBatch, List<Particle2> particles, BlendState blendState, RasterizerState rasterizer) {
            if (particles.Count == 0)
                return;

            spriteBatch.Begin(SpriteSortMode.Deferred, blendState, Main.DefaultSamplerState, DepthStencilState.None, rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            foreach (Particle2 particle in particles) {
                particle.Draw(spriteBatch);
            }

            spriteBatch.End();
        }

        public static Particle2 NewParticle<T>(Vector2 position, Vector2 velocity, Color color, float scale, Layer layer = Layer.BeforeProjectiles, BlendMode blendMode = BlendMode.Additive) where T : Particle2 {
            Particle2 particle = Activator.CreateInstance<T>();
            return NewParticle(position, velocity, particle, color, scale, layer, blendMode);
        }

        public static Particle2 NewParticle(Vector2 position, Vector2 velocity, Particle2 particle, Color color, float scale, Layer layer = Layer.BeforeProjectiles, BlendMode blendMode = BlendMode.Additive) {
            if (particle is null)
                throw new ArgumentNullException(nameof(particle));

            /* TODO:
            if (ParticleCount >= Config.Instance.MaxParticles)
                return null;
            */

            particle.position = position;
            particle.velocity = velocity;
            particle.color = color;
            particle.scale = scale;
            particle.layer = layer;
            particle.blendMode = blendMode;
            
            particle.OnSpawn();

            _particles.Add(particle);
            totalParticleCount++;

            return particle;
        }
    }
}