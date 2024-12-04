using SFML.Graphics;
using SFML.Window;

namespace Lab10;

public class Window
{
    private RenderWindow _window;

    private void OnWindowClosed(object? sender, EventArgs e)
    {
        _window.Close();
    }

    protected virtual void Draw()
    {
        // Здесь можно добавлять логику отрисовки
    }

    // Конструктор для создания окна
    public Window(uint width, uint height, string title)
    {
        _window = new RenderWindow(new VideoMode(width, height), title);
        _window.Closed += OnWindowClosed;
    }

    // Запуск основного цикла работы окна
    public void Run()
    {
        while (_window.IsOpen)
        {
            // Обрабатываем события
            _window.DispatchEvents();

            // Очищаем экран
            _window.Clear(Color.Black);

            // Вызываем метод для отрисовки содержимого
            Draw();

            // Отображаем содержимое окна
            _window.Display();
        }
    }

}