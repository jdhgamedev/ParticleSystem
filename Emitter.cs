using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarForged.Content.Effects.Particles {
    public abstract class Emitter {

        // ==== Particle Values =========
        public int maxParticles;
        public float spawnRate; // number of particles spawned a frame. 0.5 = one spawn every 2 frames

        // ==== Movement ================
        public Vector2 position;
        public float rotation; 
        
        protected List<Particle2> emitterParticles = new List<Particle2>();  // Store references to particles
        protected List<float> emitterParticleScales = new List<float>();  // Store list of particle scales

        private float accumulator = 0f;

        public Emitter(int maxParticles, float spawnRate, Vector2 position) {
            this.maxParticles = maxParticles;
            this.spawnRate = spawnRate;
            this.position = position;
        }

        public virtual void Update() {
            if (emitterParticles.Count <= maxParticles)
                accumulator += spawnRate;

            while (accumulator >= 1f) {
                var particle = CreateParticle();
                emitterParticles.Add(particle);
                emitterParticleScales.Add(particle.scale);

                accumulator -= 1f;
            }

            OnUpdate();

            RespawnParticles();
        }

        // Particle Respawn, Override to alter what happens when particles are respawned
        public virtual void RespawnParticles() {
            // If a particle has expired, respawn it
            for (int i = 0; i <= emitterParticles.Count - 1; i++) {
                var particle = emitterParticles[i];

                if (particle.timeLeft <= 0) {
                    particle.position = position;
                    particle.velocity = Vector2.Zero;
                    particle.scale = emitterParticleScales[i];
                    DefaultParticleManager.AddParticle(particle);
                }
            }
        }

        // Override to add logic executed each frame
        public virtual void OnUpdate() { }

        // Must override CreateParticle method with particle creation to create particles
        protected abstract Particle2 CreateParticle();

        public virtual void OnDestroy() { }
    }
}
