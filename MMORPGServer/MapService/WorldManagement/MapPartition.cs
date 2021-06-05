using Common.Protocol.Map;
using System.Numerics;

namespace MapService.WorldManagement
{

	public class MapPartition
    {
        public Vector2Int Vector;

        public MapPartition()
        {
            Vector = new Vector2Int();
        }

        public MapPartition(Vector2 vector) : this(vector.X, vector.Y)
        {

        }

        public MapPartition(float x, float y)
        {
            int xInt = (int)(x / MapConfiguration.MapAreaSize) * (int)MapConfiguration.MapAreaSize;
            int yInt = (int)(y / MapConfiguration.MapAreaSize) * (int)MapConfiguration.MapAreaSize;
            Vector = new Vector2Int(xInt, yInt);
        }

        public MapPartition(PlayerStateMessage msg) : this(msg.Position)
        {
        }

        public MapPartition(int x, int y)
        {
            Vector = new Vector2Int(x, y);
        }

        public override int GetHashCode()
        {
            return Vector.X.GetHashCode() ^ Vector.Y.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            var otherPartition = obj as MapPartition;
            if (otherPartition == null)
                return false;

            return Vector.X == otherPartition.Vector.X && Vector.Y == otherPartition.Vector.Y;
        }

        public static bool operator ==(MapPartition a, MapPartition b)
        {
            if ((object)a == null)
            {
                return (object)b == null;
            }

            return a.Equals(b);
        }

        public static bool operator !=(MapPartition a, MapPartition b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            return Vector.X + "-" + Vector.Y;
        }
    }
}
