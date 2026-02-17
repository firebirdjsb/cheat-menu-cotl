using System;
using System.Collections.Generic;
using UnityEngine;

namespace CheatMenu
{
	public class TextureHelper
	{
		[Init]
		public static void Init()
		{
			TextureHelper.s_textureHelper = new Dictionary<string, Texture2D>();
		}

		[Unload]
		public static void Unload()
		{
			foreach (Texture2D texture2D in TextureHelper.s_textureHelper.Values)
			{
				global::UnityEngine.Object.Destroy(texture2D);
			}
			TextureHelper.s_textureHelper.Clear();
		}

		private static void SaveTexture(Texture2D tex, Color color)
		{
			TextureHelper.s_textureHelper[TextureHelper.GetTextureKey(color)] = tex;
		}

		private static Texture2D GetCachedTexture(Color color)
		{
			string textureKey = TextureHelper.GetTextureKey(color);
			Texture2D texture2D;
			if (!TextureHelper.s_textureHelper.TryGetValue(textureKey, out texture2D))
			{
				return null;
			}
			return texture2D;
		}

		private static string GetTextureKey(Color color)
		{
			return string.Format("{0}-{1}-{2}-${3}", new object[] { color.r, color.g, color.b, color.a });
		}

		public static Texture2D GetSolidTexture(Color color, bool withHideFlags)
		{
			Texture2D cachedTexture = TextureHelper.GetCachedTexture(color);
			if (cachedTexture != null)
			{
				return cachedTexture;
			}
			Texture2D texture2D = new Texture2D(100, 100);
			Color[] pixels = texture2D.GetPixels();
			for (int i = 0; i < pixels.Length; i++)
			{
				pixels[i] = color;
			}
			texture2D.SetPixels(pixels);
			texture2D.Apply();
			if (withHideFlags)
			{
				texture2D.hideFlags = HideFlags.DontUnloadUnusedAsset;
			}
			TextureHelper.SaveTexture(texture2D, color);
			return texture2D;
		}

		private static Dictionary<string, Texture2D> s_textureHelper;
	}
}
