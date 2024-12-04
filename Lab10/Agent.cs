namespace Lab10;

public class Agent
{
    public int Type { get; set; } // Тип агента
    public int Energy { get; set; } // Энергия агента
    public int Parent { get; set; } // Родитель агента
    public int Age { get; set; } // Возраст агента (в итерациях)
    public int Generation { get; set; } // Поколение агента
    public Coordinates Coordinates { get; set; } // Координаты агента
    public int Direction { get; set; } // Направление агента
    public List<int> Inputs = new List<int>(Constants.InputNumber); // Входы агента
    public List<int> Weights = new List<int>(Constants.InputNumber * Constants.OutputNumber); // Веса агента
    public List<int> Biass = new List<int>(Constants.OutputNumber); // Смещения агента
    public List<int> Actions = new List<int>(Constants.OutputNumber); // Действия агента
}

public struct Coordinates
{
    public int X, Y;
}