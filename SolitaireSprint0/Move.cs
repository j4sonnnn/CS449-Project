namespace SolitaireSprint0
{
    public readonly record struct Move(int FromRow, int FromCol, int ToRow, int ToCol)
    {
        public int MidRow => (FromRow + ToRow) / 2;
        public int MidCol => (FromCol + ToCol) / 2;
    }
}