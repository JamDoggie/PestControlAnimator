using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PestControlAnimator.monogame.content
{
    public static class ContentManager
    {
        public static Dictionary<string, Texture2D> Textures { get; } = new Dictionary<string, Texture2D>();

        /// <summary>
        /// Grabs texture from Dictionary, returns null if no texture with the given key exists.
        /// to load a texture, refer to ContentManager.LoadTexture() and ContentManager.UnloadTexture() to unload a texture.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static Texture2D GetTexture(string key)
        {
            Texture2D tex;
            Textures.TryGetValue(key.Replace('\\', '/'), out tex);

            return tex;
        }

        /// <summary>
        /// Loads the texture into the dictionary and in turn the memory.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="texture"></param>
        public static void LoadTexture(string key, Texture2D texture)
        {
            Textures[key] = texture;
        }

        /// <summary>
        /// Unloads the texture from the dictionary and in turn the memory.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="texture"></param>
        public static void UnloadTexture(string key, Texture2D texture)
        {
            Textures.Remove(key);
        }

        public static void UnloadAllTextures()
        {
            Textures.Clear();
        }
    }
}
