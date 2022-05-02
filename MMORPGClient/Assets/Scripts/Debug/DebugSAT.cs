using Common.GameDesign.Skill;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Debug
{
	public class DebugSAT : MonoBehaviour
	{
		public SkillCollision sc = new SkillCollision();
		public SkillCollision sc2 = new SkillCollision();

		public bool ShowPolygonSc = true;
		public bool ShowPolygonSc2 = true;
		public bool ShowAxisSc = true;
		public bool ShowAxisSc2 = true;

		public bool ShowProjectionAxisSc = true;
		public bool ShowProjectionAxisSc2 = true;

		public bool MoveUp = false;
		public bool MoveDown = false;

		public GameObject IsHitNotifier;

		public void Start()
		{
			sc.Shapes.Add(new PolygonShape()
			{
				Points = new List<System.Numerics.Vector2>()
				{
					new System.Numerics.Vector2(0, 0),
					new System.Numerics.Vector2(100, 50),
					new System.Numerics.Vector2(50, 100),
					new System.Numerics.Vector2(-50, 100),
					new System.Numerics.Vector2(-100, 50)
				}
			});

			sc2.Shapes.Add(new PolygonShape()
			{
				Points = new List<System.Numerics.Vector2>()
				{
					new System.Numerics.Vector2(25, 0),
					new System.Numerics.Vector2(100, -50),
					new System.Numerics.Vector2(-50, -75)
				}
			});
		}

		private void OnDrawGizmos()
		{
			PrecalcAxis(sc);
			PrecalcAxis(sc2);
			Gizmos.color = Color.white;
			if (ShowAxisSc)
				RenderAxis(sc);
			Gizmos.color = Color.gray;
			if (ShowAxisSc2)
				RenderAxis(sc2);

			Gizmos.color = Color.green;
			if (ShowPolygonSc)
				RenderShape(sc);

			Gizmos.color = Color.blue;
			if (ShowPolygonSc2)
				RenderShape(sc2);

			bool isHit = SAT(sc.Shapes[0] as PolygonShape, sc2.Shapes[0] as PolygonShape);
			IsHitNotifier.SetActive(isHit);
		}

		private bool SAT(PolygonShape shape, PolygonShape shape2)
		{
			if (!ProjectPolygon(shape, shape2, ShowProjectionAxisSc, Color.green, Color.blue))
				return false;

			if (!ProjectPolygon(shape2, shape, ShowProjectionAxisSc2, Color.blue, Color.green))
				return false;

			return true;
		}

		private static bool ProjectPolygon(PolygonShape shape, PolygonShape shape2, bool render, Color col, Color col2)
		{
			var axis = shape.Axis;
			for (int i = 0; i < axis.Count; i++)
			{
				var projection = PolygonShape.ScalarProjection(axis[i], shape.Points);
				var projection2 = PolygonShape.ScalarProjection(axis[i], shape2.Points);

				if(render)
				{
					var outsideEdge = shape.Points[i] + (shape.Edges[i] * 4);

					Gizmos.color = col;
					Gizmos.DrawLine(ToVec3(outsideEdge + (axis[i] * projection.X)), ToVec3(outsideEdge + (axis[i] * projection.Y)));
					Gizmos.color = col2;
					Gizmos.DrawLine(ToVec3(outsideEdge + (axis[i] * projection2.X)), ToVec3(outsideEdge + (axis[i] * projection2.Y)));
				}

				if (!PolygonShape.IntervalIntersection(projection, projection2))
					return false;
			}

			return true;
		}

		private void RenderAxis(SkillCollision sc)
		{
			foreach (PolygonShape shape in sc.Shapes)
			{
				var axis = shape.Axis;
				var edges = shape.Edges;
				for (int i = 0; i < axis.Count; i++)
				{
					var outsideEdge = shape.Points[i] + (edges[i] * 4);
					var middleEdge = shape.Points[i] + (edges[i] / 2);
					Gizmos.DrawLine(ToVec3(middleEdge), ToVec3(middleEdge + axis[i] * 10));
					Gizmos.DrawLine(ToVec3(outsideEdge - axis[i] * 1000), ToVec3(outsideEdge + axis[i] * 1000));
				}
			}
		}

		private void PrecalcAxis(SkillCollision sc)
		{
			foreach (PolygonShape shape in sc.Shapes)
			{
				shape.PrecalculatePolygonValues();
			}
		}

		private void RenderShape(SkillCollision sc)
		{
			foreach (PolygonShape shape in sc.Shapes)
			{
				for (int i = 0; i < shape.Points.Count; i++)
				{
					int ii = (i + 1) % shape.Points.Count;
					Gizmos.DrawLine(ToVec3(shape.Points[i]), ToVec3(shape.Points[ii]));
				}
			}
		}

		public static Vector3 ToVec3(System.Numerics.Vector2 vec2)
		{
			return new Vector3(vec2.X, vec2.Y, 0);
		}

		public void Update()
		{
			if (MoveUp)
			{
				MoveShape(sc, 10 * Time.deltaTime);
			}
			if (MoveDown)
			{
				MoveShape(sc, -10 * Time.deltaTime);
			}
		}

		private void MoveShape(SkillCollision sc, float val)
		{
			foreach (PolygonShape shape in sc.Shapes)
			{
				for (int i = 0; i < shape.Points.Count; i++)
				{
					shape.Points[i] = new System.Numerics.Vector2(shape.Points[i].X, shape.Points[i].Y + val);
				}
			}
		}
	}
}