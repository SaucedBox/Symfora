using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TripleS.Animations {

    /// <summary>
    /// Updates and handles animations.
    /// </summary>
    public class Animator {
        public List<Animation> Animations { get; set; }
        public bool Paused { get; set; }
        public float PlaySpeed { get; set; }
        public Animation CurrentAnimation { get; protected set; }
        public int CurrentFrame { get; protected set; }
        public Texture2D OutputTexture { get; protected set; }
        public Rectangle OutputSource { get; protected set; }
        public bool PlayedAnim { get; protected set; }

        private string defaultAnim;
        float timer;

        public Animator(Animation[] anims, string defName)
        {
            Animations = anims.ToList();
            ChangeDefaultAnim(defName);
            Paused = false;
            PlaySpeed = 1;
            PlayAnimation(defaultAnim);
        }

        /// <summary>
        /// Starts a registered animation.
        /// </summary>
        public void PlayAnimation(string id)
        {
            if (CurrentAnimation.ID != id)
            {
                CurrentAnimation = Animations.Where(x => x.ID == id).First();
                CurrentFrame = 0;
                timer = 1;
                OutputTexture = CurrentAnimation.Frames[0];
                OutputSource = GetSource();

                if (!CurrentAnimation.Loop)
                    PlayedAnim = false;
            }
        }

        /// <summary>
        /// Stops current animation.
        /// </summary>
        public void StopAnimation()
        {
            PlayAnimation(defaultAnim);
        }

        /// <summary>
        /// Sets the animation that will play when no others are playing.
        /// </summary>
        public void ChangeDefaultAnim(string id, bool play = false)
        {
            if (Animations.Where(x => x.ID == id).Count() != 0)
            {
                defaultAnim = id;
                if (play)
                    PlayAnimation(defaultAnim);
            }
        }

        /// <summary>
        /// Updates the animation.
        /// </summary>
        public void UpdateFrame(float deltaTime)
        {
            if (!Paused)
            {
                if (timer <= 0)
                {
                    timer = 1;
                    CurrentFrame++;
                    if (CurrentFrame >= CurrentAnimation.Length)
                    {
                        PlayedAnim = true;
                        CurrentFrame = 0;
                        if (!CurrentAnimation.Loop)
                            PlayAnimation(defaultAnim);
                    }

                    CurrentFrame = Math.Clamp(CurrentFrame, 0, CurrentAnimation.Length);

                    if (CurrentAnimation.Type == AnimationType.Images)
                    {
                        OutputTexture = CurrentAnimation.Frames[CurrentFrame];
                        OutputSource = new Rectangle(0, 0, OutputTexture.Width, OutputTexture.Height);
                    }
                    else
                    {
                        OutputTexture = CurrentAnimation.Frames[0];
                        OutputSource = GetSource();
                    }
                }
                else
                {
                    if(!CurrentAnimation.Loop)
                        PlayedAnim = false;
                    else
                        PlayedAnim = true;

                    timer -= deltaTime * CurrentAnimation.Speed * PlaySpeed;
                }
            }
        }

        private Rectangle GetSource()
        {
            return new Rectangle(0, CurrentAnimation.FrameHeight * CurrentFrame, CurrentAnimation.FrameWidth, CurrentAnimation.FrameHeight);
        }

        public Texture2D[] LoadTextures(string location, string[] fileNames, ContentManager content)
        {
            List<Texture2D> textures = new List<Texture2D>();
            for(int i = 0; i < fileNames.Length; i++)
            {
                Texture2D t = content.Load<Texture2D>(location + "/" + fileNames[i]);
                textures.Add(t);
            }
            return textures.ToArray();
        }
    }
}
