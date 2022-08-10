using Common.Protocol.Map;
using Common.Protocol.Map.Interfaces;
using System;
using System.Numerics;

namespace Common
{
	public class Vector2Int
	{
        public int X;
        public int Y;

		public Vector2Int() : this(0, 0)
		{

		}

		public Vector2Int(int x, int y)
		{
			X = x;
			Y = y;
		}

        public Vector2Int(Vector2 vector) : this(vector.X, vector.Y)
        {

        }

        public Vector2Int(float x, float y)
        {
            if (x < 0)
                x -= MapConfiguration.MapAreaSize;

            if (y < 0)
                y -= MapConfiguration.MapAreaSize;

            X = (int)(x / MapConfiguration.MapAreaSize) * (int)MapConfiguration.MapAreaSize;
            Y = (int)(y / MapConfiguration.MapAreaSize) * (int)MapConfiguration.MapAreaSize;
        }

        public Vector2Int(IPartitionMessage msg) : this(msg.Position)
        {
        }

        public override int GetHashCode()
        {
            return (X << 2) ^ Y;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            var otherPartition = obj as Vector2Int;
            if (otherPartition == null)
                return false;

            return X == otherPartition.X && Y == otherPartition.Y;
        }

        public static bool operator ==(Vector2Int a, Vector2Int b)
        {
            if ((object)a == null)
            {
                return (object)b == null;
            }

            return a.Equals(b);
        }

        public static bool operator !=(Vector2Int a, Vector2Int b)
        {
            return !(a == b);
        }
    }
}
