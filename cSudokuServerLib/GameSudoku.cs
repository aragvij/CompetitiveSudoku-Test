using System;
using cSudokuCommonLib;

namespace cSudokuServerLib
{
    // Для генерации условия игры я использовал рассуждения, приведённые здесь: https://habr.com/ru/post/192102/

    /// <summary>
    /// Класс, реализующий логику игры
    /// </summary>
    internal class GameSudoku
    {
        /// <summary>
        /// Сумма всех цисел в строке или столбце в правильно разгаданном судоку
        /// </summary>
        private const int SumInLineIsWin = 45;

        /// <summary>
        /// Двумерный массив 9х9 клеток игры
        /// </summary>
        private CSudokuCell[,] cellGrid;

        /// <summary>
        /// Генератор псевдослучайных числел, исопльзуется для создания начального условия игры
        /// </summary>
        private readonly Random random;

        /// <summary>
        /// Уровень сложности, задаётся первым игроком
        /// </summary>
        internal SudokuLevel GameLevel { get; set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="level">Уровень сложности игры</param>
        public GameSudoku(SudokuLevel level)
        {
            /// <summary>
            /// Таблица для создания условия новой игры.
            /// Начальное значение всегда одинаково, оно меняется в методах Shuffle и ClearCells, и таким образом получается условие очередной игры.
            /// </summary>
            byte[,] grid =            {
            {1,2,3, 4,5,6, 7,8,9}, // +
            {4,5,6, 7,8,9, 1,2,3}, // | первый горизонтальный регион
            {7,8,9, 1,2,3, 4,5,6}, // _

            {2,3,4, 5,6,7, 8,9,1}, // + 
            {5,6,7, 8,9,1, 2,3,4}, // | второй горизонтальный регион
            {8,9,1, 2,3,4, 5,6,7}, // _

            {3,4,5, 6,7,8, 9,1,2}, // + 
            {6,7,8, 9,1,2, 3,4,5}, // | третий горизонтальный регион
            {9,1,2, 3,4,5, 6,7,8}  // _
            //   |  |   |  |___|- третий вертикальный регион
            //   |  |___|- второй вертикальный регион
            //___|- первый вертикальный регион
            };

            // Сохранение уровня сложности игры
            GameLevel = level;

            // Задание начального значения для счётчика псевдослучайных чисел
            random = new Random(DateTime.Now.Hour * DateTime.Now.Minute * DateTime.Now.Second);

            // Перемешивание начальной таблицы
            Shuffle(ref grid);

            // Очистка случайных ячеек в зависимости от уровня: чем он меньше, тем меньше клеток очищаем
            ClearCells(ref grid);

            // Перенос сформированного условия из массива байтов в ячейки игры
            FillSudokuCells(grid);
        }

        /// <summary>
        /// Возвращает текущее состояние игры, для передачи его игрокам.
        /// </summary>
        /// <param name="realGame">При false возвращает пустое поле. Используется для очистки поля у игрока, покинувшего турнир.</param>
        /// <returns>Текущее состояние игры</returns>
        public CSudokuState GetGameState(bool realGame = true)
        {
            CSudokuCell[] array = new CSudokuCell[cellGrid.Length];

            int i = 0;
            foreach (CSudokuCell cell in cellGrid)
            {
                array[i++] = realGame
                    ? cell
                    : CSudokuCell.NULL(cell.X, cell.Y);
            }

            return new CSudokuState
            {
                Level = realGame ? GameLevel : SudokuLevel.Unknown,
                Values = array
            };
        }

        /// <summary>
        /// Попытка хода игрока
        /// </summary>
        /// <param name="newCellValue">Ячейка с новым значением</param>
        /// <param name="winnerId">Если в результате этого хода определился игрок, возвращаем его Id</param>
        /// <returns>true, если этот ход может быть совершен, иначе false</returns>
        public bool TrySetValue(CSudokuCell newCellValue, out int? winnerId)
        {
            winnerId = null;
            if (newCellValue.X < 0 || newCellValue.X > cellGrid.GetUpperBound(0) ||
                newCellValue.Y < 0 || newCellValue.Y > cellGrid.GetUpperBound(1))
            {
                return false;
            }

            // Определение в серверной таблице изменяемой ячейки по координатам
            CSudokuCell serverCell = cellGrid[newCellValue.X, newCellValue.Y];

            if (serverCell.IsTask)
            {
                // Если эта ячейка является условием, игнорируем запрос на её изменение
                return false;
            }

            // Нельзя допустить, чтобы два хода произошли одновременно
            lock (cellGrid)
            {
                // Можно поставить значение в ошибочную ячейку, или в пустую или ту, в которой значение было установлено этим же игроком, в противном случае ход не происходит
                if (serverCell.Error ||
                    null == serverCell.Owner ||
                    newCellValue.Owner == serverCell.Owner)
                {
                    if (newCellValue.Value == 0)
                    {
                        // Если игрок стирает значение в ячейке, она вновь становится "ничьей"
                        serverCell.Owner = null;
                        serverCell.Value = 0;
                    }
                    else
                    {
                        // В ячейку поставлено реальное значение, она помечается занятой этим игроком
                        serverCell.Owner = newCellValue.Owner;
                        serverCell.Value = newCellValue.Value;

                        if (TestForWin())
                        {
                            // Игра окончена, победитель определён
                            winnerId = newCellValue.Owner;
                        }
                    }

                    // Пометка ошибочных ячеек: тех, которые имеют значение такое же, как и устанавливаемое значение, и находятся в той же строке, столбце или регионе. В ошибочную ячейку пожет поставить своё значение другой игрок.
                    TestForError(serverCell);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Перенос сформированного условия в ячейки игры
        /// </summary>
        /// <param name="grid"></param>
        private void FillSudokuCells(byte[,] grid)
        {
            cellGrid = new CSudokuCell[grid.GetLength(0), grid.GetLength(1)];
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    cellGrid[i, j] = new CSudokuCell
                    {
                        Value = grid[i, j],
                        X = i,
                        Y = j,
                    };

                    if (grid[i, j] > 0)
                    {
                        // Пометка ячейки со значением как "принадлежащей серверу". В неё не может поставить своё значение никакой игрок.
                        cellGrid[i, j].Owner = CSudokuCell.ServerId;
                    }
                }
            }
        }

        /// <summary>
        /// Очистка случайных ячеек.
        /// Количество очищаемых ячеек взято Level*10 (при пяти уровнях это кажется нормальным)
        /// </summary>
        private void ClearCells(ref byte[,] grid)
        {
            // Уровень сложности игры
            int delete10 = (int)GameLevel;
            if (delete10 > 7)
            {
                // Не хотим допустить, чтобы судоку каким-то образом представляло собой пустое поле
                GameLevel = SudokuLevel.простейший;
            }

            // Очистка случайной ячейки из всего набора
            int index, x, y;
            for (int i = 0; i < delete10 * 10; i++)
            {
                // Выбор случайной не пустой ячейки
                do
                {
                    index = random.Next(81);
                    x = index / 9;
                    y = index % 9;
                }
                while (grid[x, y] == 0);

                // Очистка ячейки
                grid[x, y] = 0;
            }
        }

        /// <summary>
        /// Перемешивание начальной таблицы
        /// </summary>
        private void Shuffle(ref byte[,] grid)
        {
            // Производим от 50 до 100 случайных итераций перемешивания
            int num = random.Next(51) + 50;
            for (int i = 0; i < num; i++)
            {
                switch (random.Next(5))
                {
                    case 0:
                        // Перестановка строк
                        SwapRows(ref grid);
                        continue;
                    case 1:
                        // Перестановка столбцов
                        SwapColums(ref grid);
                        continue;
                    case 2:
                        // Перестановка горизонтальных регионов
                        SwapRegionRow(ref grid);
                        continue;
                    case 3:
                        // Перестановка вертикальных регионов
                        SwapRegionCol(ref grid);
                        continue;
                    default:
                        // Транспонирование таблицы
                        Transposing(ref grid);
                        break;
                }
            }
        }

        /// <summary>
        /// Транспонирование всей таблицы: столбцы становятся строками
        /// </summary>
        private void Transposing(ref byte[,] grid)
        {
            byte[,] newGrid = new byte[9, 9];
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    // Обмен строк со столбцами
                    newGrid[i, j] = grid[j, i];
                }
            }
            grid = newGrid;
        }

        /// <summary>
        /// Обмен двух случайных строк в пределах случайного региона
        /// </summary>
        private void SwapRows(ref byte[,] grid)
        {
            // Горизонтальный регион 0..2
            int region = random.Next(3);

            // 1-я строка в регионе
            int N1 = random.Next(3);

            // 2-я строка в регионе, отличная от первой
            int N2 = random.Next(3);
            while (N1 == N2)
            {
                N2 = random.Next(3);
            }

            // Номера строк в таблице
            N1 = region * 3 + N1;
            N2 = region * 3 + N2;

            // Цикл обмена строк по столбцам таблицы
            SwapTwoLine(N1, N2, grid);
        }

        /// <summary>
        /// Обмен местами двух строк
        /// </summary>
        /// <param name="N1">Номер первой строки</param>
        /// <param name="N2">Номер второй строки</param>
        private void SwapTwoLine(int N1, int N2, byte[,] grid)
        {
            for (int j = 0; j < 9; j++)
            {
                // Обмен местами каждого элемента двух строк
                SwapTwoByte(ref grid[N1, j], ref grid[N2, j]);
            }
        }

        /// <summary>
        /// Обмен местами двух ячеек таблицы
        /// </summary>
        /// <param name="x">Номер столбца</param>
        /// <param name="y">Номер строки</param>
        private void SwapTwoByte(ref byte x, ref byte y)
        {
            // Обмен значениями
            x = (byte)(x ^ y);
            y = (byte)(x ^ y);
            x = (byte)(x ^ y);
        }

        /// <summary>
        /// Обмен двух горизонтальных регионов
        /// </summary>
        private void SwapRegionRow(ref byte[,] grid)
        {
            // Первый регион
            int R1 = random.Next(3);

            // Второй регион, отличный от первого
            int R2 = random.Next(3);
            while (R1 == R2)
            {
                R2 = random.Next(3);
            }

            // Обмен местами каждой из трёх строк в регионах
            for (int i = 0; i < 3; i++)
            {
                SwapTwoLine(R1 * 3 + i, R2 * 3 + i, grid);
            }
        }

        /// <summary>
        /// Обмен двух столбцов в пределах региона
        /// </summary>
        private void SwapColums(ref byte[,] grid)
        {
            // Транспонирование
            Transposing(ref grid);

            // Обмен строк
            SwapRows(ref grid);

            // Транспонирование
            Transposing(ref grid);
        }

        /// <summary>
        /// Обмен двух вертикальных регионов
        /// </summary>
        private void SwapRegionCol(ref byte[,] grid)
        {
            // Транспонирование
            Transposing(ref grid);

            // Обмен горизонтальных регионов
            SwapRegionRow(ref grid);

            // Транспонирование
            Transposing(ref grid);
        }

        /// <summary>
        /// Проверка нового значения в ячейки на то, что оно - ошибочно. Ошибочные ячейки помечаются.
        /// </summary>
        /// <param name="testingCell">Проверяемая ячейка</param>
        private void TestForError(CSudokuCell testingCell)
        {
            for (int i = 0; i < 9; i++)
            {
                // Проверка по строке
                if (i != testingCell.Y)
                {
                    cellGrid[testingCell.X, i].TestError(testingCell);
                }

                // Проверка по столбцу
                if (i != testingCell.X)
                {
                    cellGrid[i, testingCell.Y].TestError(testingCell);
                }
            }

            // Проверка по региону 3х3

            // Определение "координат" региона
            int regX = testingCell.X / 3;
            int regY = testingCell.Y / 3;

            for (int x = 0; x < 3; x++)
            {
                // Координата Х проверяемой ячейки в регионе
                int realX = regX * 3 + x;

                for (int y = 0; y < 3; y++)
                {
                    // Координата Y проверяемой ячейки в регионе
                    int realY = regY * 3 + y;

                    // Не сравниваем ячейку саму с собой
                    if (realX != testingCell.X || realY != testingCell.Y)
                    {
                        cellGrid[realX, realY].TestError(testingCell);
                    }
                }
            }
        }

        /// <summary>
        /// Проверка факта успешного окончания игры (решения судоку)
        /// </summary>
        /// <returns></returns>
        private bool TestForWin()
        {
            int sumX, sumY;

            for (int x = 0; x < 9; x++)
            {
                sumX = 0;
                sumY = 0;
                for (int y = 0; y < 9; y++)
                {
                    // Сумма цифр в столбце
                    sumX += cellGrid[x, y].Value;
                    // Сумма цифр в строке
                    sumY += cellGrid[y, x].Value;
                }

                // Если сумма цифр не равна 45, то судоку не решено правильно
                if (sumX != SumInLineIsWin ||
                    sumY != SumInLineIsWin)
                {
                    return false;
                }
            }

            // Судоку решено
            return true;
        }
    }
}