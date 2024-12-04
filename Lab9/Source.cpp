#include <iostream>
#include <windows.h>
#include <string>
using namespace std;


const int N = 30;           // ������ �������� ����
const int Pmax = 35;           // ���������� ����� ��������
const int Amax = 36;           // ���������� ����� �������

const int Nin = 12;           // ����� ������
const int Nout = 4;            // ����� �������
const int Nw = ((Nin * Nout) + Nout);  // ����� �����

const int EAmax = 60;           // ������� ������ ������
const int EFmax = 15;           // ������� ���
const double Erep = 0.9;          // ������� �����������

const int NORTH = 0;            // �������� �����������
const int SOUTH = 1;            // ����� ����������
const int EAST = 2;            // ��������� �����������
const int WEST = 3;            // �������� �����������

const int HERB_PLANE = 0;      // ������� ����������
const int CARN_PLANE = 1;      // ������� �������
const int PLANT_PLANE = 2;      // ������� ���

const int HERBIVORE = 0;       // ��� �����������
const int CARNIVORE = 1;       // ��� �������
const int DEAD = -1;       // ��� ����������

const int ACTION_LEFT = 0;     // �����
const int ACTION_RIGHT = 1;     // ������
const int ACTION_MOVE = 2;     // ������
const int ACTION_EAT = 3;     // ������

const int HERB_FRONT = 0;      // ���������� �������
const int CARN_FRONT = 1;      // ������ �������
const int PLANT_FRONT = 2;      // ��� �������
const int HERB_LEFT = 3;      // ���������� �����
const int CARN_LEFT = 4;      // ������ �����
const int PLANT_LEFT = 5;      // ��� �����
const int HERB_RIGHT = 6;      // ���������� ������
const int CARN_RIGHT = 7;      // ������ ������
const int PLANT_RIGHT = 8;      // ��� ������
const int HERB_PROXIMITY = 9;   // ���������� ������
const int CARN_PROXIMITY = 10;  // ������ ������
const int PLANT_PROXIMITY = 11; // ��� ������


// ������������
#define getSRand()  ((float)rand() / (float)RAND_MAX)  // ��������� ����� 0..1
#define getRand(x)  (int)((x) * getSRand())            // ��������� ����� 0..x
#define getWeight() (getRand(9)-1)                     // ��������� ���/��������


struct TXY {         // ����������
    int X, Y;
};

struct TAgent {      // �������� ������
    int type;                // ��� ������
    int energy;              // ������� ������
    int parent;
    int age;                 // ������� ������ ����������
    int generation;          // ���������
    TXY location;            // ���������� ������
    int direction;           // �����������
    int inputs[Nin];         // �����
    int weights[Nin * Nout]; // ����
    int biass[Nout];         // ��������
    int actions[Nout];       // �������� ������
};

int map[3][N][N];            // �����
TXY plants[Pmax];            // ���������� ��������          
TAgent agents[Amax];         // ������

const TXY northFront[] = { {-2,-2}, {-2,-1}, {-2, 0}, {-2, 1}, {-2, 2}, { 9, 9} };
const TXY northLeft[] = { { 0,-2}, {-1,-2}, { 9, 9} };
const TXY northRight[] = { { 0, 2}, {-1, 2}, { 9, 9} };
const TXY northProx[] = { { 0,-1}, {-1,-1}, {-1, 0}, {-1, 1}, { 0, 1}, { 9, 9} };
const TXY westFront[] = { { 2,-2}, { 1,-2}, { 0,-2}, {-1,-2}, {-2,-2}, { 9, 9} };
const TXY westLeft[] = { { 2, 0}, { 2,-1}, { 9, 9} };
const TXY westRight[] = { {-2, 0}, {-2,-1}, { 9, 9} };
const TXY westProx[] = { { 1, 0}, { 1,-1}, { 0,-1}, {-1,-1}, {-1, 0}, { 9, 9} };

int agentTypeCounts[2] = { 0,0 };         // ���������� ������� �� �����
int agentMaxAge[2] = { 0,0 };             // ������� ������� �� �����
int agentBirths[2] = { 0,0 };             // ���������� �������� �� �����
int agentDeaths[2] = { 0,0 };             // ���������� ������� �� �����
TAgent* agentMaxPtr[2];                   // ��������� ������ �� �����
int agentTypeReproductions[2] = { 0,0 };  // ���������� ����������� �� �����
TAgent bestAgent[2];                      // ��������� �������� ������
int agentMaxGen[2] = { 0,0 };             // ���������� ��������� �� �����

int bx;     // �������� �� ����� �� ��� X
int by;     // �������� �� ����� �� ��� Y
int dx;     // �������� ����� �� ��� X
int dy;     // �������� ����� �� ��� Y

HWND    HW = GetConsoleWindow();                      // ��������� �� ����
HDC     H1 = GetDC(HW);
HDC     H2 = CreateCompatibleDC(H1);
HBITMAP BM;                                           // ������

HBRUSH  Hc = CreateSolidBrush(0x000000FF);            // ����� �������
HBRUSH  Hh = CreateSolidBrush(0x00FF0000);            // ����� �����������
HBRUSH  Hp = CreateSolidBrush(0x0000FF00);            // ����� ������
HBRUSH  Hs = CreateSolidBrush(0x00FFFFFF);            // ����� ��������� ������

RECT    R;                                            // ����� ����
int     W;                                            // ������ ����
int     H;                                            // ������ ����


void CursorOff()  // ���������� �������
{
    HANDLE H = GetStdHandle(STD_OUTPUT_HANDLE);
    CONSOLE_CURSOR_INFO CI;
    GetConsoleCursorInfo(H, &CI);
    CI.bVisible = FALSE;
    SetConsoleCursorInfo(H, &CI);
}

void GraphInit()  // ������������� �������
{
    GetClientRect(HW, &R);                             // ������� �����
    W = R.right - R.left;                              // ������ ����
    H = R.bottom - R.top;                              // ������ ����
    BM = CreateCompatibleBitmap(H1, W, H);             // ������
    SelectObject(H2, BM);
    dx = ((R.right - R.left + 1) / N);                 // ��� �� ��� X
    bx = ((R.right - R.left + 1 - dx * N) / 2);        // ����� �������
    dy = ((R.bottom - R.top + 1) / N);                 // ��� �� ��� Y
    by = ((R.bottom - R.top + 1 - dy * N) / 2);        // ������� �������
    CursorOff();                                       // ���������� �������
}


void ShowCell(int x, int y, HBRUSH B)  // ����� ������ � ������������ x,y ������ B
{
    SelectObject(H2, B);
    Rectangle(H2, bx + x * dx, by + y * dy, bx + (x + 1) * dx + 1, by + (y + 1) * dy + 1);
}

void Screen()  // ����� ����� �� �����
{
    PatBlt(H2, 0, 0, W, H, WHITENESS);  // ������� ����  
    for (int y = 0; y < N; y++) {
        for (int x = 0; x < N; x++) {
            if (map[CARN_PLANE][y][x] != 0) ShowCell(x, y, Hc);
            else if (map[HERB_PLANE][y][x] != 0) ShowCell(x, y, Hh);
            else if (map[PLANT_PLANE][y][x] != 0) ShowCell(x, y, Hp);
            else ShowCell(x, y, Hs);
        }
    }
    BitBlt(H1, R.left, R.top, R.right, R.bottom, H2, 0, 0, SRCCOPY);  // ���������
    Sleep(100);                                                       // ��������
}

void CloseGraph()  // �������� �������
{
    PatBlt(H1, 0, 0, W, H, BLACKNESS);  // ������� ���� 
    ReleaseDC(HW, H1);
    DeleteDC(H2);
    DeleteObject(BM);
    DeleteObject(Hc);
    DeleteObject(Hh);
    DeleteObject(Hp);
    DeleteObject(Hc);
}


TXY AddInEmptyCell(int Level)  // ���������� � ������ ������
{
    TXY res;
    do {
        res.X = getRand(N);   res.Y = getRand(N);
    } while (map[Level][res.Y][res.X] != 0);
    map[Level][res.Y][res.X]++;
    return res;
}


void AgentToMap(TAgent* agent)  // ��������� ������ �� �����
{
    agent->location = AddInEmptyCell(agent->type);
    agent->direction = getRand(4);
}


void InitAgent(TAgent* agent)  // ������������� ������
{
    agent->energy = (EAmax / 2);
    agent->age = 0;
    agent->generation = 1;
    agentTypeCounts[agent->type]++;
    AgentToMap(agent);
    for (int i = 0; i < (Nin * Nout); i++) agent->weights[i] = getWeight();
    for (int i = 0; i < Nout; i++) agent->biass[i] = getWeight();
}


void Init()  // ������������� ������
{
    for (int l = 0; l < 3; l++)                    // ������� �����
        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++) map[l][y][x] = 0;
    for (int p = 0; p < Pmax; p++)                 // ������� ��������
        plants[p] = AddInEmptyCell(PLANT_PLANE);
    //GrowPlant(p);   
    for (int a = 0; a < Amax; a++) {               // ������������� �������
        agents[a].type = CARNIVORE;
        if (a < (Amax / 2)) agents[a].type = HERBIVORE;
        InitAgent(&agents[a]);
    }
}


int Clip(int z)  // ������� ����� �������
{
    if (z > N - 1) z = (z % N);
    else if (z < 0) z = (N + z);
    return z;
}


void Percept(int x, int y, int* inputs, const TXY* offsets, int neg)  // ����������
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


void Turn(int action, TAgent* agent)  // �������
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


void Move(TAgent* agent)  // ��������
{
    const TXY offsets[4] = { {-1,0},{1,0},{0,1},{0,-1} };
    map[agent->type][agent->location.Y][agent->location.X]--;
    agent->location.X = Clip(agent->location.X + offsets[agent->direction].X);
    agent->location.Y = Clip(agent->location.Y + offsets[agent->direction].Y);
    map[agent->type][agent->location.Y][agent->location.X]++;
}


void KillAgent(TAgent* agent)  // ������ ������
{
    agentDeaths[agent->type]++;
    map[agent->type][agent->location.Y][agent->location.X]--;
    agentTypeCounts[agent->type]--;
    if (agent->age > bestAgent[agent->type].age) {  // ��������� �������
        memcpy((void*)&bestAgent[agent->type], (void*)agent, sizeof(TAgent));
    }
    if (agentTypeCounts[agent->type] < (Amax / 4)) InitAgent(agent);  // ������������� ������
    else {                                                            // ����� ������
        agent->location.X = -1;
        agent->location.Y = -1;
        agent->type = DEAD;
    }
}


void ReproduceAgent(TAgent* agent)  // �������� ���������
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
            child->energy = agent->energy = (EAmax / 2);  // �������
            agentTypeCounts[child->type]++;
            agentTypeReproductions[child->type]++;
        }
    }
}


int ChooseObject(int plane, int ax, int ay, const TXY* offsets, int neg, int* ox, int* oy)  // ����� �������
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


void Eat(TAgent* agent)  // �������
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


void Simulate(TAgent* agent)  // ��������� ������
{
    int x = agent->location.X;
    int y = agent->location.Y;

    switch (agent->direction) {  // ���������� �� �����������

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

    for (int out = 0; out < Nout; out++) {   // ������ �������
        agent->actions[out] = agent->biass[out];    // ������������� ������ ���������
        for (int in = 0; in < Nin; in++) {          // ������ �� ���������� ������
            agent->actions[out] += (agent->inputs[in] * agent->weights[(out * Nin) + in]);
        }
    }

    int largest = -9;
    int winner = -1;
    for (int out = 0; out < Nout; out++) {   // �������� �������
        if (agent->actions[out] >= largest) {
            largest = agent->actions[out];
            winner = out;
        }
    }

    // ���������� �������
    if (winner == ACTION_LEFT) Turn(winner, agent);
    if (winner == ACTION_RIGHT) Turn(winner, agent);
    if (winner == ACTION_MOVE) Move(agent);
    if (winner == ACTION_EAT) Eat(agent);

    // ������� �������
    if (agent->type == HERBIVORE) agent->energy -= 2; else agent->energy -= 1;

    if (agent->energy <= 0) KillAgent(agent);   // ������ � ����� ������
    else {
        agent->age++;
        if (agent->age > agentMaxAge[agent->type]) {  // ��������� ���������� ������
            agentMaxAge[agent->type] = agent->age;
            agentMaxPtr[agent->type] = agent;
        }
    }
}


void ShowStat()  // ����������� ����������
{
    SetConsoleCP(1251);
    SetConsoleOutputCP(1251);
    cout << "����������:" << endl;
    cout << "���������� �����                - " << agentTypeCounts[HERBIVORE] << endl;
    cout << "�������� �����                  - " << agentTypeCounts[CARNIVORE] << endl;
    cout << "������� ����������              - " << agentMaxAge[HERBIVORE] << endl;
    cout << "������� ��������                - " << agentMaxAge[CARNIVORE] << endl;
    cout << "�������� ����������             - " << agentBirths[HERBIVORE] << endl;
    cout << "�������� ��������               - " << agentBirths[CARNIVORE] << endl;
    cout << "������� ����������              - " << agentDeaths[HERBIVORE] << endl;
    cout << "������� ��������                - " << agentDeaths[CARNIVORE] << endl;
    cout << "����������� ����������          - " << agentTypeReproductions[HERBIVORE] << endl;
    cout << "����������� ��������            - " << agentTypeReproductions[CARNIVORE] << endl;
    cout << "���������� ��������� ���������� - " << agentMaxGen[HERBIVORE] << endl;
    cout << "���������� ��������� ��������   - " << agentMaxGen[CARNIVORE] << endl;

    cout << endl << "���� ������� �����������:" << endl;
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

    cout << endl << "���� ������� �������:" << endl;
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
    Init();                             // ������������� ������   
    GraphInit();                        // ������������� �������
    for (int i = 0; i < 600; i++) {     // ������� ���� ���������
        for (int t = HERBIVORE; t <= CARNIVORE; t++)
            for (int i = 0; i < Amax; i++)
                if (agents[i].type == t) Simulate(&agents[i]);
        Screen();                       // ������������
    }
    CloseGraph();                       // �������� �������
    ShowStat();                         // ����������� ����������
    system("pause");                    // �����
    return 0;
}