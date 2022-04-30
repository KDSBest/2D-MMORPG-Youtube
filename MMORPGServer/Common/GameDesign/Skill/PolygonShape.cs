using System.Collections.Generic;
using System.Numerics;

namespace Common.GameDesign.Skill
{
	public class PolygonShape : SkillIndicatorShape
	{
		public List<Vector2> Points = new List<Vector2>();
        private List<Vector2> edges = new List<Vector2>();
        private List<Vector2> axis = new List<Vector2>();

        public void PrecalculatePolygonValues()
		{
            if (edges.Count == Points.Count)
                return;

            axis.Clear();
            edges.Clear();

            for (int i = 0; i < Points.Count; i++)
            {
                Vector2 a = Points[i];
                Vector2 b = Points[(i + 1) % Points.Count];
                Vector2 edge = b - a;
                Vector2 axis = new Vector2(-edge.Y, edge.X);
                axis = Vector2.Normalize(axis);

                edges.Add(edge);
                this.axis.Add(axis);
            }
        }

        public override bool IsHit(Vector2 position)
		{
            List<Vector2> positionPoints = new List<Vector2>()
            {
                position
            };

            this.PrecalculatePolygonValues();

            for (int i = 0; i < this.axis.Count; i++)
			{
                Vector2 projection = this.Project(this.axis[i], this.Points);
                Vector2 projection2 = this.Project(this.axis[i], positionPoints);

                if (!IntervalIntersection(projection, projection2))
                    return false;
			}

            return true;
		}
        
        public Vector2 Project(Vector2 axis, List<Vector2> points)
        {
            float dotProduct = Vector2.Dot(points[0], axis);
            Vector2 result = new Vector2(dotProduct, dotProduct);

            for (int i = 1; i < points.Count; i++)
            {

                dotProduct = Vector2.Dot(points[i], axis);

                if (dotProduct < result.X)
                {
                    result.X = dotProduct;
                }
                else
                {
                    if (dotProduct > result.Y)
                    {
                        result.Y = dotProduct;
                    }
                }
            }

            return result;
        }

        public bool IntervalIntersection(Vector2 a, Vector2 b)
        {
            if (a.X < b.X)
            {
                return b.X - a.Y < 0;
            }
            else
            {
                return a.X - b.Y < 0;
            }
        }
    }
}
