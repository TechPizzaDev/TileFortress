using System.Collections.Generic;
using TileFortress.GameWorld;

namespace AStar
{
    internal class ComparePfNodeMatrix : IComparer<TilePosition>
    {
        readonly PathFinderNodeFast[,] _matrix;

        public ComparePfNodeMatrix(PathFinderNodeFast[,] matrix)
        {
            _matrix = matrix;
        }

        public int Compare(TilePosition a, TilePosition b)
        {
            if (_matrix[a.X, a.Y].F_Gone_Plus_Heuristic > _matrix[b.X, b.Y].F_Gone_Plus_Heuristic)
                return 1;

            if (_matrix[a.X, a.Y].F_Gone_Plus_Heuristic < _matrix[b.X, b.Y].F_Gone_Plus_Heuristic)
                return -1;

            return 0;
        }
    }
}