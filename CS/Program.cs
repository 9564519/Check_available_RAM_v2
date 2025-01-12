using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows.Forms.Primitives;

// Класс формы приложения
public class Form1 : Form
{
    // Конструктор формы
    public Form1()
    {
        InitializeComponent(); // Инициализация компонентов формы
        CheckMemory(); // Первая проверка доступной памяти сразу после запуска

        // Создание таймера для периодической проверки памяти
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        timer.Interval = 30000; // Таймер с интервалом 30 секунд (30000) - тестирование 3000
        // Подписка на событие Tick таймера для вызова метода CheckMemory
        timer.Tick += (s, e) => CheckMemory();
        timer.Start(); // Запуск таймера
    }

    // Метод для проверки доступной памяти
    private void CheckMemory()
    {
        // Создание объекта PerformanceCounter для получения информации о доступной памяти
        var memoryCounter = new PerformanceCounter("Memory", "Available Bytes");
        // Получение значения доступной памяти в байтах
        float availableMemory = memoryCounter.NextValue();
        // Перевод байтов в мегабайты
        float availableMemoryInMB = availableMemory / (1024 * 1024);
        // Проверка, если доступная память меньше 100 МБ - тестирование 15360
        if (availableMemoryInMB < 100)
        {
            // Создаем и отображаем предупреждающее окно
            using (var warningForm = new WarningForm($"Внимание! Свободная память: {availableMemoryInMB:F2} МБ."))
            {
                warningForm.ShowDialog(this); // Показываем модально
            }
        }
        // Старый код:
        //{
        //    // Отображение предупреждающего сообщения
        //    MessageBox.Show($"Внимание! Свободная память: {availableMemoryInMB:F2} МБ.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //}
    }

    // Метод для инициализации компонентов формы
    private void InitializeComponent()
    {
        this.Text = "Проверка памяти"; // Заголовок окна
        this.ClientSize = new System.Drawing.Size(300, 200); // Размер окна
    }

    // Главный метод приложения
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles(); // Включение визуальных стилей
        Application.SetCompatibleTextRenderingDefault(false); // Совместимость с текстовым рендерингом
        Application.Run(new Form1()); // Запуск главного окна приложения
    }
}
