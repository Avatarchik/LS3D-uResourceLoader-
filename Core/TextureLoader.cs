using UnityEngine;

using System.Drawing;
using System.IO;
using System;

public class TextureLoader
{
    public static Texture2D LoadTexture(string texture)
    {
		if (texture.Contains (".TGA")) {
			Debug.Log("Unsupported Texture Format.");
			return null;
		}
			
		try
		{
	        Bitmap bmp = new Bitmap(File.OpenRead(WorldController.GamePath + "\\MAPS\\" + texture));
	        Texture2D texture2D = new Texture2D(bmp.Width, bmp.Height, TextureFormat.RGBA32, true);

	        for (int x = 0; x < bmp.Width; x++)
	        {
	            for (int y = 0; y < bmp.Height; y++)
	            {
	                System.Drawing.Color bmpPixel = bmp.GetPixel(x, y);
	                texture2D.SetPixel(x, y, new Color32(bmpPixel.R, bmpPixel.G, bmpPixel.B, bmpPixel.A));
	            }
	        }

	        texture2D.Apply(true);
	        texture2D.Compress(false);

	        bmp.Dispose();
	        return texture2D;
		}
		catch (Exception e)
		{
			Debug.Log(e.ToString());
			return null;
		}
    }
}
