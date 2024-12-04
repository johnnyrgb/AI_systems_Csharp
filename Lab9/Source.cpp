#include <iostream>
#include <windows.h>
#include <string>
using namespace std;


const int N = 30;           // размер игрового поля
const int Pmax = 35;           // предельное число растений
const int Amax = 36;           // предельное число агентов

const int Nin = 12;           // число входов
const int Nout = 4;            // число выходов
const int Nw = ((Nin * Nout) + Nout);  // число весов

const int EAmax = 60;           // энергия нового агента
const int EFmax = 15;           // энергия еды
const double Erep = 0.9;          // энергия репродукции

const int NORTH = 0;            // северное направление
const int SOUTH = 1;            // южное напрвление
const int EAST = 2;            // восточное направление
const int WEST = 3;            // западное направление

const int HERB_PLANE = 0;      // уровень травоядных
const int CARN_PLANE = 1;      // уровень хищника
const int PLANT_PLANE = 2;      // уровень еды

const int HERBIVORE = 0;       // тип травоядного
const int CARNIVORE = 1;       // тип хищника
const int DEAD = -1;       // тип съеденного

const int ACTION_LEFT = 0;     // влево
const int ACTION_RIGHT = 1;     // вправо
const int ACTION_MOVE = 2;     // вперед
const int ACTION_EAT = 3;     // кушать

const int HERB_FRONT = 0;      // травоядное впереди
const int CARN_FRONT = 1;      // хищник впереди
const int PLANT_FRONT = 2;      // еда впереди
const int HERB_LEFT = 3;      // травоядное слева
const int CARN_LEFT = 4;      // хищник слева
const int PLANT_LEFT = 5;      // еда слева
const int HERB_RIGHT = 6;      // травоядное справа
const int CARN_RIGHT = 7;      // хищник справа
const int PLANT_RIGHT = 8;      // еда справа
const int HERB_PROXIMITY = 9;   // травоядное вблизи
const int CARN_PROXIMITY = 10;  // хищник вблизи
const int PLANT_PROXIMITY = 11; // еда вблизи


// макрофункции
#define getSRand()  ((float)rand() / (float)RAND_MAX)  // случайное число 0..1
#define getRand(x)  (int)((x) * getSRand())            // случайное число 0..x
#define getWeight() (getRand(9)-1)                     // случайный вес/смещение


struct TXY {         // координаты
    int X, Y;
};

struct TAgent {      // описание агента
    int type;                // тип агента
    int energy;              // энергия агента
    int parent;
    int age;                 // возраст агента витерациях
    int generation;          // поколение
    TXY location;            // координаты агента
    int direction;           // направление
    int inputs[Nin];         // входы
    int weights[Nin * Nout]; // веса
    int biass[Nout];         // смещения
    int actions[Nout];       // действия агента
};

int map[3][N][N];            // карта
TXY plants[Pmax];            // координаты растений          
TAgent agents[Amax];         // агенты

const TXY northFront[] = { {-2,-2}, {-2,-1}, {-2, 0}, {-2, 1}, {-2, 2}, { 9, 9} };
const TXY northLeft[] = { { 0,-2}, {-1,-2}, { 9, 9} };
const TXY northRight[] = { { 0, 2}, {-1, 2}, { 9, 9} };
const TXY northProx[] = { { 0,-1}, {-1,-1}, {-1, 0}, {-1, 1}, { 0, 1}, { 9, 9} };
const TXY westFront[] = { { 2,-2}, { 1,-2}, { 0,-2}, {-1,-2}, {-2,-2}, { 9, 9} };
const TXY westLeft[] = { { 2, 0}, { 2,-1}, { 9, 9} };
const TXY westRight[] = { {-2, 0}, {-2,-1}, { 9, 9} };
const TXY westProx[] = { { 1, 0}, { 1,-1}, { 0,-1}, {-1,-1}, {-1, 0}, { 9, 9} };

int agentTypeCounts[2] = { 0,0 };         // количество агентов по типам
int agentMaxAge[2] = { 0,0 };             // возраст агентов по типам
int agentBirths[2] = { 0,0 };             // количество рождений по типам
int agentDeaths[2] = { 0,0 };             // количество гибелей по типам
TAgent* agentMaxPtr[2];                   // старейшие агенты по типам
int agentTypeReproductions[2] = { 0,0 };  // количество репродукций по типам
TAgent bestAgent[2];                      // старейшие погибшие агенты
int agentMaxGen[2] = { 0,0 };             // наибольшие поколения по типам

int bx;     // смещение до сетки по оси X
int by;     // смещение до сетки по оси Y
int dx;     // смещение сетки по оси X
int dy;     // смещение сетки по оси Y

HWND    HW = GetConsoleWindow();                      // указатель на окно
HDC     H1 = GetDC(HW);
HDC     H2 = CreateCompatibleDC(H1);
HBITMAP BM;                                           // битмап

HBRUSH  Hc = CreateSolidBrush(0x000000FF);            // кисть хищника
HBRUSH  Hh = CreateSolidBrush(0x00FF0000);            // кисть травоядного
HBRUSH  Hp = CreateSolidBrush(0x0000FF00);            // кисть травки
HBRUSH  Hs = CreateSolidBrush(0x00FFFFFF);            // кисть свободной ячейки

RECT    R;                                            // рамка окна
int     W;                                            // ширина окна
int     H;                                            // высота окна


void CursorOff()  // отключение курсора
{
    HANDLE H = GetStdHandle(STD_OUTPUT_HANDLE);
    CONSOLE_CURSOR_INFO CI;
    GetConsoleCursorInfo(H, &CI);
    CI.bVisible = FALSE;
    SetConsoleCursorInfo(H, &CI);
}

void GraphInit()  // инициализация графики
{
    GetClientRect(HW, &R);                             // размеры рамки
    W = R.right - R.left;                              // ширина окна
    H = R.bottom - R.top;                              // высота окна
    BM = CreateCompatibleBitmap(H1, W, H);             // битмап
    SelectObject(H2, BM);
    dx = ((R.right - R.left + 1) / N);                 // шаг по оси X
    bx = ((R.right - R.left + 1 - dx * N) / 2);        // левая граница
    dy = ((R.bottom - R.top + 1) / N);                 // шаг по оси Y
    by = ((R.bottom - R.top + 1 - dy * N) / 2);        // верхняя граница
    CursorOff();                                       // отключение курсора
}


void ShowCell(int x, int y, HBRUSH B)  // вывод ячейки с координатами x,y кистью B
{
    SelectObject(H2, B);
    Rectangle(H2, bx + x * dx, by + y * dy, bx + (x + 1) * dx + 1, by + (y + 1) * dy + 1);
}

void Screen()  // вывод карты на экран
{
    PatBlt(H2, 0, 0, W, H, WHITENESS);  // очистка окна  
    for (int y = 0; y < N; y++) {
        for (int x = 0; x < N; x++) {
            if (map[CARN_PLANE][y][x] != 0) ShowCell(x, y, Hc);
            else if (map[HERB_PLANE][y][x] != 0) ShowCell(x, y, Hh);
            else if (map[PLANT_PLANE][y][x] != 0) ShowCell(x, y, Hp);
            else ShowCell(x, y, Hs);
        }
    }
    BitBlt(H1, R.left, R.top, R.right, R.bottom, H2, 0, 0, SRCCOPY);  // отрисовка
    Sleep(100);                                                       // задержка
}

void CloseGraph()  // закрытие графики
{
    PatBlt(H1, 0, 0, W, H, BLACKNESS);  // очистка окна 
    ReleaseDC(HW, H1);
    DeleteDC(H2);
    DeleteObject(BM);
    DeleteObject(Hc);
    DeleteObject(Hh);
    DeleteObject(Hp);
    DeleteObject(Hc);
}


TXY AddInEmptyCell(int Level)  // добавление в пустую ячейку
{
    TXY res;
    do {
        res.X = getRand(N);   res.Y = getRand(N);
    } while (map[Level][res.Y][res.X] != 0);
    map[Level][res.Y][res.X]++;
    return res;
}


void AgentToMap(TAgent* agent)  // установка агента на карту
{
    agent->location = AddInEmptyCell(agent->type);
    agent->direction = getRand(4);
}


void InitAgent(TAgent* agent)  // инициализация агента
{
    agent->energy = (EAmax / 2);
    agent->age = 0;
    agent->generation = 1;
    agentTypeCounts[agent->type]++;
    AgentToMap(agent);
    for (int i = 0; i < (Nin * Nout); i++) agent->weights[i] = getWeight();
    for (int i = 0; i < Nout; i++) agent->biass[i] = getWeight();
}


void Init()  // инициализация модели
{
    for (int l = 0; l < 3; l++)                    // очистка карты
        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++) map[l][y][x] = 0;
    for (int p = 0; p < Pmax; p++)                 // посадка растений
        plants[p] = AddInEmptyCell(PLANT_PLANE);
    //GrowPlant(p);   
    for (int a = 0; a < Amax; a++) {               // инициализация агентов
        agents[a].type = CARNIVORE;
        if (a < (Amax / 2)) agents[a].type = HERBIVORE;
        InitAgent(&agents[a]);
    }
}


int Clip(int z)  // переход через границу
{
    if (z > N - 1) z = (z % N);
    else if (z < 0) z = (N + z);
    return z;
}


void Percept(int x, int y, int* inputs, const TXY* offsets, int neg)  // восприятие
{
    for (int p = HERB_PLANE; p <= PLANT_PLANE; p++) {
        int i = 0;  inputs[p] = 0;
        while (offsets[i].X != 9) {
            int xoff = Clip(x + (offsets[i].X * neg));
            int yoff = Clip(y + (offsets[i].Y * neg));
            if (map[p][yoff][xoff] != 0) inputs[p]++;
            i++;
        }
    }
}


void Turn(int action, TAgent* agent)  // поворот
{
    if (agent->direction == NORTH)
        if (action == ACTION_LEFT) agent->direction = WEST; else agent->direction = EAST;
    if (agent->direction == SOUTH)
        if (action == ACTION_LEFT) agent->direction = EAST; else agent->direction = WEST;
    if (agent->direction == EAST)
        if (action == ACTION_LEFT) agent->direction = NORTH; else agent->direction = SOUTH;
    if (agent->direction == WEST)
        if (action == ACTION_LEFT) agent->direction = SOUTH; else agent->direction = NORTH;
}


void Move(TAgent* agent)  // движение
{
    const TXY offsets[4] = { {-1,0},{1,0},{0,1},{0,-1} };
    map[agent->type][agent->location.Y][agent->location.X]--;
    agent->location.X = Clip(agent->location.X + offsets[agent->direction].X);
    agent->location.Y = Clip(agent->location.Y + offsets[agent->direction].Y);
    map[agent->type][agent->location.Y][agent->location.X]++;
}


void KillAgent(TAgent* agent)  // гибель агента
{
    agentDeaths[agent->type]++;
    map[agent->type][agent->location.Y][agent->location.X]--;
    agentTypeCounts[agent->type]--;
    if (agent->age > bestAgent[agent->type].age) {  // сохраняем лучшего
        memcpy((void*)&bestAgent[agent->type], (void*)agent, sizeof(TAgent));
    }
    if (agentTypeCounts[agent->type] < (Amax / 4)) InitAgent(agent);  // инициализация агента
    else {                                                            // конец агента
        agent->location.X = -1;
        agent->location.Y = -1;
        agent->type = DEAD;
    }
}


void ReproduceAgent(TAgent* agent)  // рождение потомства
{
    TAgent* child;
    int i;

    if (agentTypeCounts[agent->type] < (Amax / 2)) {
        for (i = 0; i < Amax; i++) {
            if (agents[i].type == DEAD) break;
        }
        if (i < Amax) {
            child = &agents[i];
            memcpy((void*)child, (void*)agent, sizeof(TAgent));
            AgentToMap(child);
            if (getSRand() <= 0.2) {
                child->weights[getRand(Nw)] = getWeight();
            }
            child->generation = child->generation + 1;
            child->age = 0;
            if (agentMaxGen[child->type] < child->generation) agentMaxGen[child->type] = child->generation;
            child->energy = agent->energy = (EAmax / 2);  // энергия
            agentTypeCounts[child->type]++;
            agentTypeReproductions[child->type]++;
        }
    }
}


int ChooseObject(int plane, int ax, int ay, const TXY* offsets, int neg, int* ox, int* oy)  // выбор объекта
{
    int xoff, yoff, i = 0;

    while (offsets[i].X != 9) {
        xoff = Clip(ax + (offsets[i].X * neg));
        yoff = Clip(ay + (offsets[i].Y * neg));
        if (map[plane][yoff][xoff] != 0) {
            *ox = xoff; *oy = yoff;
            return 1;
        }
        i++;
    }
    return 0;
}


void Eat(TAgent* agent)  // питание
{
    int plane = 0, ox, oy, ret = 0, i;

    if (agent->type == CARNIVORE) plane = HERB_PLANE;
    else if (agent->type == HERBIVORE) plane = PLANT_PLANE;

    int ax = agent->location.X;
    int ay = agent->location.Y;

    if (agent->direction == NORTH) ret = ChooseObject(plane, ax, ay, northProx, 1, &ox, &oy);
    if (agent->direction == SOUTH) ret = ChooseObject(plane, ax, ay, northProx, -1, &ox, &oy);
    if (agent->direction == WEST)  ret = ChooseObject(plane, ax, ay, westProx, 1, &ox, &oy);
    if (agent->direction == EAST)  ret = ChooseObject(plane, ax, ay, westProx, -1, &ox, &oy);

    if (ret) {

        if (plane == PLANT_PLANE) {

            for (i = 0; i < Pmax; i++) {
                if ((plants[i].X == ox) && (plants[i].Y == oy))
                    break;
            }

            if (i < Pmax) {
                agent->energy += EFmax;
                if (agent->energy > EAmax) agent->energy = EAmax;
                map[PLANT_PLANE][oy][ox]--;
                plants[i] = AddInEmptyCell(PLANT_PLANE);
            }

        }
        else if (plane == HERB_PLANE) {

            for (i = 0; i < Amax; i++) {
                if ((agents[i].location.X == ox) && (agents[i].location.Y == oy))
                    break;
            }

            if (i < Amax) {
                agent->energy += (EFmax * 2);
                if (agent->energy > EAmax) agent->energy = EAmax;
                KillAgent(&agents[i]);
            }

        }

        if (agent->energy > (Erep * EAmax)) {
            ReproduceAgent(agent);
            agentBirths[agent->type]++;
        }

    }
}


void Simulate(TAgent* agent)  // симуляция агента
{
    int x = agent->location.X;
    int y = agent->location.Y;

    switch (agent->direction) {  // восприятие по направлению

    case NORTH:
        Percept(x, y, &agent->inputs[HERB_FRONT], northFront, 1);
        Percept(x, y, &agent->inputs[HERB_LEFT], northLeft, 1);
        Percept(x, y, &agent->inputs[HERB_RIGHT], northRight, 1);
        Percept(x, y, &agent->inputs[HERB_PROXIMITY], northProx, 1);
        break;

    case SOUTH:
        Percept(x, y, &agent->inputs[HERB_FRONT], northFront, -1);
        Percept(x, y, &agent->inputs[HERB_LEFT], northLeft, -1);
        Percept(x, y, &agent->inputs[HERB_RIGHT], northRight, -1);
        Percept(x, y, &agent->inputs[HERB_PROXIMITY], northProx, -1);
        break;

    case WEST:
        Percept(x, y, &agent->inputs[HERB_FRONT], westFront, 1);
        Percept(x, y, &agent->inputs[HERB_LEFT], westLeft, 1);
        Percept(x, y, &agent->inputs[HERB_RIGHT], westRight, 1);
        Percept(x, y, &agent->inputs[HERB_PROXIMITY], westProx, 1);
        break;

    case EAST:
        Percept(x, y, &agent->inputs[HERB_FRONT], westFront, -1);
        Percept(x, y, &agent->inputs[HERB_LEFT], westLeft, -1);
        Percept(x, y, &agent->inputs[HERB_RIGHT], westRight, -1);
        Percept(x, y, &agent->inputs[HERB_PROXIMITY], westProx, -1);
        break;

    }

    for (int out = 0; out < Nout; out++) {   // расчёт решений
        agent->actions[out] = agent->biass[out];    // инициализация выхода смещением
        for (int in = 0; in < Nin; in++) {          // расчёт по взвешенным входам
            agent->actions[out] += (agent->inputs[in] * agent->weights[(out * Nin) + in]);
        }
    }

    int largest = -9;
    int winner = -1;
    for (int out = 0; out < Nout; out++) {   // принятие решения
        if (agent->actions[out] >= largest) {
            largest = agent->actions[out];
            winner = out;
        }
    }

    // выполнение решения
    if (winner == ACTION_LEFT) Turn(winner, agent);
    if (winner == ACTION_RIGHT) Turn(winner, agent);
    if (winner == ACTION_MOVE) Move(agent);
    if (winner == ACTION_EAT) Eat(agent);

    // затраты энергии
    if (agent->type == HERBIVORE) agent->energy -= 2; else agent->energy -= 1;

    if (agent->energy <= 0) KillAgent(agent);   // гибель и жизнь агента
    else {
        agent->age++;
        if (agent->age > agentMaxAge[agent->type]) {  // фиксируем старейшего агента
            agentMaxAge[agent->type] = agent->age;
            agentMaxPtr[agent->type] = agent;
        }
    }
}


void ShowStat()  // отображение статистики
{
    SetConsoleCP(1251);
    SetConsoleOutputCP(1251);
    cout << "Результаты:" << endl;
    cout << "Травоядных всего                - " << agentTypeCounts[HERBIVORE] << endl;
    cout << "Хищников всего                  - " << agentTypeCounts[CARNIVORE] << endl;
    cout << "Возраст травоядных              - " << agentMaxAge[HERBIVORE] << endl;
    cout << "Возраст хищников                - " << agentMaxAge[CARNIVORE] << endl;
    cout << "Рождений травоядных             - " << agentBirths[HERBIVORE] << endl;
    cout << "Рождений хищников               - " << agentBirths[CARNIVORE] << endl;
    cout << "Гибелей травоядных              - " << agentDeaths[HERBIVORE] << endl;
    cout << "Гибелей хищников                - " << agentDeaths[CARNIVORE] << endl;
    cout << "Репродукций травоядных          - " << agentTypeReproductions[HERBIVORE] << endl;
    cout << "Репродукций хищников            - " << agentTypeReproductions[CARNIVORE] << endl;
    cout << "Наибольшие поколения травоядных - " << agentMaxGen[HERBIVORE] << endl;
    cout << "Наибольшие поколения хищников   - " << agentMaxGen[CARNIVORE] << endl;

    cout << endl << "Веса лучшего травоядного:" << endl;
    for (int i = 0; i < Nout; i++) {
        cout.width(4);  cout << bestAgent[HERBIVORE].biass[i] << " ";
    }
    cout << endl << endl;
    for (int o = 0; o < Nout; o++) {
        for (int i = 0; i < Nin; i++) {
            cout.width(4);  cout << bestAgent[HERBIVORE].weights[(o * Nin) + i] << " ";
        }
        cout << endl;
    }

    cout << endl << "Веса лучшего хищника:" << endl;
    for (int i = 0; i < Nout; i++) {
        cout.width(4);  cout << bestAgent[CARNIVORE].biass[i] << " ";
    }
    cout << endl << endl;
    for (int o = 0; o < Nout; o++) {
        for (int i = 0; i < Nin; i++) {
            cout.width(4);  cout << bestAgent[CARNIVORE].weights[(o * Nin) + i] << " ";
        }
        cout << endl;
    }
}


int main()
{
    Init();                             // инициализация модели   
    GraphInit();                        // инициализация графики
    for (int i = 0; i < 600; i++) {     // главный цикл симуляции
        for (int t = HERBIVORE; t <= CARNIVORE; t++)
            for (int i = 0; i < Amax; i++)
                if (agents[i].type == t) Simulate(&agents[i]);
        Screen();                       // визуализация
    }
    CloseGraph();                       // закрытие графики
    ShowStat();                         // отображение статистики
    system("pause");                    // пауза
    return 0;
}