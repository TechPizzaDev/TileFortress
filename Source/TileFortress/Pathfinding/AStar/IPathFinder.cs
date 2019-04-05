using System.Collections.Generic;
using TileFortress.GameWorld;

namespace AStar
{
    public interface IPathFinder
    {
        List<PathFinderNode> FindPath(TilePosition start, TilePosition end);
    }
}
