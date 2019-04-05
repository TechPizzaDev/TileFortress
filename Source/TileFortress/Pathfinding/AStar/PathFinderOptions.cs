namespace AStar
{
    public class PathFinderOptions
    {
        public HeuristicFormula Formula { get; set; }
        public bool Diagonals { get; set; }
        public bool HeavyDiagonals { get; set; }
        public int HeuristicEstimate { get; set; }
        public bool PunishChangeDirection { get; set; }
        public bool TieBreaker { get; set; }
        public int SearchLimit { get; set; }

        public PathFinderOptions()
        {
            Formula = HeuristicFormula.DiagonalShortCut;
            HeuristicEstimate = 2;
            SearchLimit = 50000;
            Diagonals = true;
            HeavyDiagonals = true;
        }
    }
}
