using System;

namespace cSudokuCommonLib
{
    [Serializable]
    public enum SudokuLevel : byte
    {
        Unknown = 0,
        простейший,
        простой,
        средний,
        сложный,
        сложнейший
    }

    [Serializable]
    public class CSudokuState
    {
        public SudokuLevel Level;
        public CSudokuCell[] Values;

        public CSudokuCell GetValue(int i, int j)
        {
            if (i > 9 || j > 9)
            {
                return null;
            }
            return Values[j * 9 + i];
        }
    }
}
