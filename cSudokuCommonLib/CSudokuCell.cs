using System;
using System.Collections.Generic;

namespace cSudokuCommonLib
{
    [Serializable]
    public class CSudokuCell
    {
        public const int ServerId = 0;

        public byte Value;
        public int X;
        public int Y;
        public int? Owner;
        public bool Error;

        [NonSerialized]
        private List<int> errorCellList = new List<int>();

        private int Index => X * 9 + Y;
        public bool IsTask => Value != 0 && Owner == ServerId;

        public static CSudokuCell NULL(int x, int y) => new CSudokuCell
        {
            Owner = ServerId,
            Value = 0,
            X = x,
            Y = y,
        };
        public override string ToString()
        {
            return $"{Value} {X}:{Y} I={Index} {(Error ? "E" : "")}";
        }
        public void TestError(CSudokuCell testingCell)
        {
            if (Value == 0)
            {
                // Пустые ячейки не проверяем
                return;
            }

            if (0 == testingCell.Value)
            {
                // В проверяемой ячейке стёрто значение. Уберём её из списка ошибочных, если к текущей ячейке относилась ошибка, вызванная предыдущим значением в проверяемой ячейке
                SetError(testingCell, false);
            }
            else
            {
                // Ошибка - когда значения в ячейках одинаковые
                SetError(testingCell, Value == testingCell.Value);
            }

            CalculateErrorField();
        }

        private void SetError(CSudokuCell testingCell, bool thisIsError)
        {
            int testingIndex = testingCell.Index;
            if (thisIsError)
            {
                if (!errorCellList.Contains(testingIndex))
                {
                    // Взаимное добавление ячеек в ошибочные списки
                    errorCellList.Add(testingIndex);
                    testingCell.errorCellList.Add(Index);
                }
            }
            else
            {
                if (errorCellList.Contains(testingIndex))
                {
                    // Взаимное убирание из ошибочных списков
                    errorCellList.Remove(testingIndex);
                    testingCell.errorCellList.Remove(Index);
                }
            }
            testingCell.CalculateErrorField();
        }
        private void CalculateErrorField()
        {
            Error = errorCellList.Count > 0;
        }
    }
}
