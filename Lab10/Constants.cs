using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab10
{
    public static class Constants
    {
        public const int N = 30; // Размер игрового поля
        public const int PlantMaxNumber = 35;
        public const int AgentMaxNumber = 36;
        public const int InputNumber = 12;
        public const int OutputNumber = 4;
        public const int AgentEnergy = 60;
        public const int FoodEnergy = 15;
        public const double ReproductionEnergy = 0.9;

        #region Компас
        public const int North = 0;
        public const int South = 1;
        public const int East = 2;
        public const int West = 3;
        #endregion

        #region Уровни
        public const int HerbivoreLevel = 0;
        public const int CarnivoreLevel = 1;
        public const int PlantLevel = 2;
        #endregion

        #region Действия
        public const int ActionLeft = 0;     // влево
        public const int ActionRight = 1;     // вправо
        public const int ActionMove = 2;     // вперед
        public const int ActionEat = 3;     // кушать
        #endregion

        #region Чувства
        public const int HerbivoreIsAhead = 0;      // травоядное впереди
        public const int CarnivoreIsAhead = 1;      // хищник впереди
        public const int PlantIsAhead = 2;      // еда впереди
        public const int HerbivoreOnTheLeft = 3;      // травоядное слева
        public const int CarnivoreOnTheLeft = 4;      // хищник слева
        public const int PlantOnTheLeft= 5;      // еда слева
        public const int HerbivoreOnTheRight = 6;      // травоядное справа
        public const int CarnivoreOnTheRight = 7;      // хищник справа
        public const int PlantOnThRight = 8;      // еда справа
        public const int HerbivoreNear = 9;   // травоядное вблизи
        public const int CarnivoreIsNear = 10;  // хищник вблизи
        public const int PlantIsNear = 11; // еда вблизи
        #endregion

    }
}
