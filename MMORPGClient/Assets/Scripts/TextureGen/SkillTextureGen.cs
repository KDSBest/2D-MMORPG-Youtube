using Common.GameDesign.Skill;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

public class SkillTextureGen : MonoBehaviour
{
	public SpriteRenderer SpriteRenderer;

	public void Start()
	{
		SkillIndicator si = new SkillIndicator();
		si.Shapes.Add(new CircleShape()
		{
			IsExcluding = false,
			Position = new Vector2(0, 0),
			Radius = 100
		});
		si.Shapes.Add(new PolygonShape()
		{
			IsExcluding = true,
			Points = new List<Vector2>()
			{
				new Vector2(0, 90),
				new Vector2(100, 0),
				new Vector2(50, -90),
				new Vector2(-50, -90),
				new Vector2(-100, 0)
			}
		});

		var tex = GenerateTexture("test.png", si);

		SkillIndicator si2 = new SkillIndicator();
		si2.Shapes.Add(new PolygonShape()
		{
			IsExcluding = false,
			Points = new List<Vector2>()
			{
				new Vector2(-100, -100),
				new Vector2(-100, 100),
				new Vector2(100, 100),
				new Vector2(100, -100),
			}
		});

		var tex2 = GenerateTexture("test2.png", si2, 200, 1024);

		SpriteRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new UnityEngine.Vector2(0.5f, 0.5f));
	}

	public Texture2D GenerateTexture(string filename, SkillIndicator si, float worldSize = 256, int texSize = 256)
	{
		Texture2D tex = new Texture2D(texSize, texSize, TextureFormat.RGBA32, true);

		string path = Path.Combine(Application.streamingAssetsPath, "Skills", filename);

		if (File.Exists(path))
		{
			tex.LoadImage(File.ReadAllBytes(path));
			tex.Apply();
			return tex;
		}

		Vector2 worldSizeV = new Vector2(worldSize, worldSize);

		Fill(tex, new Color(0, 0, 0, 0));

		foreach (var shape in si.Shapes)
		{
			DrawShape(tex, worldSizeV, shape, shape.IsExcluding ? new Color(0, 0, 0, 0) : new Color(0.5f, 0.5f, 0.5f, 0.5f));
		}

		DrawHighlightEdges(tex, new Color(1, 1, 1, 1));

		tex.Apply();

		var bytes = tex.EncodeToPNG();
		File.WriteAllBytes(path, bytes);

		return tex;
	}

	private void DrawHighlightEdges(Texture2D tex, Color col, int borderthickness = 2)
	{
		Vector2 texSize = new Vector2(tex.width, tex.height);
		for (int x = 0; x < texSize.X; x++)
		{
			for (int y = 0; y < texSize.Y; y++)
			{
				if (tex.GetPixel(x, y).a < float.Epsilon)
					continue;

				if (IsBorder(tex, x, y, borderthickness))
					tex.SetPixel(x, y, col);
			}
		}
	}

	private bool IsBorder(Texture2D tex, int x, int y, int borderthickness)
	{
		for (int offsetX = -borderthickness; offsetX <= borderthickness; offsetX++)
		{
			for (int offsetY = -borderthickness; offsetY <= borderthickness; offsetY++)
			{
				if (offsetX == 0 && offsetY == 0)
					continue;

				int checkX = x + offsetX;
				int checkY = y + offsetY;

				if (checkX < 0 || checkX >= tex.width ||
					checkY < 0 || checkY >= tex.height)
					return true;

				if (tex.GetPixel(checkX, checkY).a < float.Epsilon)
					return true;
			}
		}

		return false;
	}

	private void Fill(Texture2D tex, Color col)
	{
		Vector2 texSize = new Vector2(tex.width, tex.height);
		for (int x = 0; x < texSize.X; x++)
		{
			for (int y = 0; y < texSize.Y; y++)
			{
				tex.SetPixel(x, y, col);
			}
		}
	}

	private void DrawShape(Texture2D tex, Vector2 worldSize, SkillIndicatorShape shape, Color col)
	{
		Vector2 texSize = new Vector2(tex.width, tex.height);
		Vector2 texToWorld = worldSize / texSize;
		Vector2 texOffset = new Vector2(-tex.width / 2, -tex.height / 2);
		for (int x = 0; x < texSize.X; x++)
		{
			for (int y = 0; y < texSize.Y; y++)
			{
				Vector2 pixel = new Vector2(x, y);
				Vector2 pixelWorld = texToWorld * (pixel + texOffset);

				if (shape.IsHit(pixelWorld))
				{
					tex.SetPixel(x, y, col);
				}
			}
		}
	}
}
