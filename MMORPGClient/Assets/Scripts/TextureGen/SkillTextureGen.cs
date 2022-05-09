using Common.GameDesign.Skill;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

public class SkillTextureGen : MonoBehaviour
{
	public void Start()
	{
		var sc = new SkillCollision();
		sc.CastType = Common.GameDesign.SkillCastType.Boss1Attack1;
		sc.Size = new Common.Vector2Int(24, 24);
		sc.Shapes.Add(new CircleShape()
		{
			IsExcluding = false,
			Position = new Vector2(0, 0),
			Radius = 10
		});
		sc.Shapes.Add(new CircleShape()
		{
			IsExcluding = true,
			Position = new Vector2(0, 0),
			Radius = 3
		});

		var tex = GenerateTexture("Boss1Attack1.png", sc, 512);
		GenerateMetadata("Boss1Attack1.json", sc);

		var sc2 = new SkillCollision();
		sc2.CastType = Common.GameDesign.SkillCastType.Boss1Attack2;
		sc2.Size = new Common.Vector2Int(24, 24);
		sc2.Shapes.Add(new CircleShape()
		{
			IsExcluding = false,
			Position = new Vector2(0, 0),
			Radius = 3
		});

		var tex2 = GenerateTexture("Boss1Attack2.png", sc2, 512);
		GenerateMetadata("Boss1Attack2.json", sc2);
	}

	private void GenerateMetadata(string filename, SkillCollision sc)
	{
		string path = Path.Combine(Application.streamingAssetsPath, "AoESkills", filename);
		File.WriteAllText(path, JsonConvert.SerializeObject(sc, new JsonSerializerSettings()
		{
			TypeNameHandling = TypeNameHandling.Auto,
			Formatting = Formatting.Indented
		}));
	}

	public Texture2D GenerateTexture(string filename, SkillCollision sc, int texSize = 256)
	{
		Texture2D tex = new Texture2D(texSize, texSize, TextureFormat.RGBA32, true);

		string path = Path.Combine(Application.streamingAssetsPath, "..", "Sprites", "BossSkills", filename);

		if (File.Exists(path))
		{
			tex.LoadImage(File.ReadAllBytes(path));
			tex.Apply();
			return tex;
		}

		Vector2 worldSizeV = new Vector2(sc.Size.X, sc.Size.Y);

		Fill(tex, new Color(0, 0, 0, 0));

		DrawShapeCollision(tex, worldSizeV, sc, new Color(0.5f, 0.5f, 0.5f, 0.5f), new Color(0, 0, 0, 0));

		DrawHighlightEdges(tex, new Color(1, 1, 1, 1));

		tex = DrawAA(tex);
		tex = DrawAA(tex);
		tex = DrawAA(tex);
		tex = DrawAA(tex);
		tex = DrawAA(tex);
		tex = DrawAA(tex);
		tex = DrawAA(tex);
		tex = DrawAA(tex);
		tex.Apply();

		var bytes = tex.EncodeToPNG();
		File.WriteAllBytes(path, bytes);

		return tex;
	}

	private Texture2D DrawAA(Texture2D tex)
	{
		Vector2 texSize = new Vector2(tex.width, tex.height);
		Texture2D texAA = new Texture2D(tex.width, tex.height, tex.format, true);
		for (int x = 0; x < texSize.X; x++)
		{
			for (int y = 0; y < texSize.Y; y++)
			{
				Vector4 avg = new Vector4(0, 0, 0, 0);
				int reads = 0;
				for (int offsetX = -1; offsetX <= 1; offsetX++)
				{
					for (int offsetY = -1; offsetY <= 1; offsetY++)
					{
						Color c;
						if(this.GetPixel(tex, x + offsetX, y + offsetY, out c))
						{
							avg += new Vector4(c.r, c.g, c.b, c.a);
							reads++;
						}
					}
				}

				avg /= reads;

				var p = tex.GetPixel(x, y);
				avg += new Vector4(p.r, p.g, p.b, p.a);

				avg /= 2;

				texAA.SetPixel(x, y, new Color(avg.x, avg.y, avg.z, avg.w));
			}
		}
		return texAA;
	}

	private bool GetPixel(Texture2D tex, int x, int y, out Color col)
	{
		col = Color.magenta;

		if (x < 0 || x >= tex.width || y < 0 || y >= tex.height)
			return false;

		col = tex.GetPixel(x, y);

		return true;
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

	private void DrawShapeCollision(Texture2D tex, Vector2 worldSize, SkillCollision sc, Color colHit, Color colNoHit)
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

				if (sc.IsHit(pixelWorld))
				{
					tex.SetPixel(x, y, colHit);
				}
				else
				{
					tex.SetPixel(x, y, colNoHit);
				}
			}
		}
	}
}
