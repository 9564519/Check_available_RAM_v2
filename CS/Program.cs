using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;

// Класс формы приложения
public class Form1 : Form
{
    // НАСТРАИВАЕМЫЕ ПАРАМЕТРЫ - можно менять под свои нужды
    private const double PERCENTAGE_THRESHOLD = 8.0; // Процент от общего ОЗУ (в %)
    private const double MIN_MEMORY_MB = 400.0; // Минимальный лимит в МБ
    private const double MAX_MEMORY_MB = 1512.0; // Максимальный лимит в МБ

    private Label statusLabel;
    private double totalMemoryMB = 0;
    private WarningForm currentWarningForm = null; // Текущее открытое окно предупреждения
    private bool isWarningShowing = false; // Флаг, показывающий, что предупреждение уже отображается

    // Конструктор формы
    public Form1()
    {
        InitializeComponent(); // Инициализация компонентов формы

        // Получаем общую память один раз при запуске
        totalMemoryMB = GetTotalMemoryMB();

        CheckMemory(); // Первая проверка доступной памяти сразу после запуска

        // Создание таймера для периодической проверки памяти
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        timer.Interval = 8000; // Таймер с интервалом в миллисекундах
        // Подписка на событие Tick таймера для вызова метода CheckMemory
        timer.Tick += (s, e) => CheckMemory();
        timer.Start(); // Запуск таймера
    }

    // Метод для получения общего объема ОЗУ в МБ
    private double GetTotalMemoryMB()
    {
        try
        {
            // Попробуем получить информацию о памяти через более надежный способ
            try
            {
                // Если у вас .NET Framework 4.0+
                var ciType = Type.GetType("Microsoft.VisualBasic.Devices.ComputerInfo, Microsoft.VisualBasic");
                if (ciType != null)
                {
                    dynamic computerInfo = Activator.CreateInstance(ciType);
                    ulong totalPhysicalMemory = computerInfo.TotalPhysicalMemory;
                    return totalPhysicalMemory / (1024.0 * 1024.0);
                }
            }
            catch
            {
                // Если не сработало
            }

            // Если все способы не сработали, используем статическое значение
            // ИЗМЕНИТЕ ЭТО ЗНАЧЕНИЕ НА РЕАЛЬНЫЙ ОБЪЕМ ВАШЕЙ ПАМЯТИ
            return 16384; // По умолчанию 16 ГБ
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Не удалось получить информацию об общей памяти: {ex.Message}\n" +
                           $"Используется значение по умолчанию: 16384 МБ (16 ГБ)",
                           "Предупреждение",
                           MessageBoxButtons.OK,
                           MessageBoxIcon.Warning);
            return 16384; // Значение по умолчанию: 16 ГБ
        }
    }

    // Метод для расчета динамического лимита
    private double CalculateDynamicLimit()
    {
        if (totalMemoryMB <= 0)
        {
            totalMemoryMB = GetTotalMemoryMB();
        }

        // Рассчитываем процент от общего объема памяти
        double percentageLimit = totalMemoryMB * (PERCENTAGE_THRESHOLD / 100.0);

        // Применяем границы
        if (percentageLimit < MIN_MEMORY_MB)
        {
            return MIN_MEMORY_MB;
        }
        else if (percentageLimit > MAX_MEMORY_MB)
        {
            return MAX_MEMORY_MB;
        }
        else
        {
            return percentageLimit;
        }
    }

    // Метод для проверки доступной памяти
    private void CheckMemory()
    {
        try
        {
            // Если уже показывается предупреждение, не проверяем снова
            if (isWarningShowing)
            {
                return;
            }

            // Создание объекта PerformanceCounter для получения информации о доступной памяти
            var memoryCounter = new PerformanceCounter("Memory", "Available Bytes");
            // Получение значения доступной памяти в байтах
            float availableMemory = memoryCounter.NextValue();

            // Даем счетчику время на инициализацию
            System.Threading.Thread.Sleep(100);
            availableMemory = memoryCounter.NextValue();

            // Перевод байтов в мегабайты
            float availableMemoryInMB = availableMemory / (1024 * 1024);

            // Динамически рассчитываем лимит для текущей системы
            double dynamicLimit = CalculateDynamicLimit();

            // Обновляем статус на главной форме
            UpdateStatus(availableMemoryInMB, dynamicLimit, totalMemoryMB);

            // Проверка с использованием динамического лимита
            if (availableMemoryInMB < dynamicLimit)
            {
                // Показываем предупреждение только если оно еще не показано
                if (!isWarningShowing)
                {
                    ShowWarningForm(availableMemoryInMB, dynamicLimit, totalMemoryMB);
                }
            }
            else
            {
                // Если память восстановилась до нормального уровня, сбрасываем флаг
                if (isWarningShowing && currentWarningForm != null)
                {
                    // Можно автоматически закрыть окно предупреждения, если память восстановилась
                    // currentWarningForm.Close();
                    // isWarningShowing = false;
                    // currentWarningForm = null;
                }
            }
        }
        catch (Exception ex)
        {
            // Обработка возможных ошибок при получении информации о памяти
            UpdateStatus(-1, -1, totalMemoryMB, $"Ошибка при проверке памяти: {ex.Message}");
        }
    }

    // Метод для показа окна предупреждения
    private void ShowWarningForm(double availableMemoryMB, double limitMB, double totalMemoryMB)
    {
        if (isWarningShowing)
        {
            return; // Уже показывается окно
        }

        isWarningShowing = true;

        // Создаем окно предупреждения
        currentWarningForm = new WarningForm(
            $"Внимание! Свободная память ниже критического уровня!",
            availableMemoryMB,
            limitMB,
            totalMemoryMB);

        // Подписываемся на событие закрытия окна
        currentWarningForm.FormClosed += (s, e) =>
        {
            isWarningShowing = false;
            currentWarningForm = null;
        };

        // Показываем окно немодально
        currentWarningForm.Show();

        // Мигаем в панели задач для привлечения внимания
        FlashWindowEx(this.Handle);
    }

    // Импорт функции для мигания окна в панели задач
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct FLASHWINFO
    {
        public uint cbSize;
        public IntPtr hwnd;
        public uint dwFlags;
        public uint uCount;
        public uint dwTimeout;
    }

    private const uint FLASHW_ALL = 3;
    private const uint FLASHW_TIMERNOFG = 12;

    private void FlashWindowEx(IntPtr handle)
    {
        FLASHWINFO fInfo = new FLASHWINFO();
        fInfo.cbSize = Convert.ToUInt32(System.Runtime.InteropServices.Marshal.SizeOf(fInfo));
        fInfo.hwnd = handle;
        fInfo.dwFlags = FLASHW_ALL | FLASHW_TIMERNOFG;
        fInfo.uCount = uint.MaxValue;
        fInfo.dwTimeout = 0;
        FlashWindowEx(ref fInfo);
    }

    // Метод для обновления статуса на главной форме
    private void UpdateStatus(double availableMB, double limitMB, double totalMB, string error = null)
    {
        if (this.InvokeRequired)
        {
            this.Invoke(new Action(() => UpdateStatus(availableMB, limitMB, totalMB, error)));
            return;
        }

        if (statusLabel == null) return;

        if (error != null)
        {
            statusLabel.Text = $"Ошибка:\n{error}\n\nПроверка будет продолжена через 30 секунд";
            return;
        }

        // Добавляем индикатор состояния предупреждения
        string warningStatus = isWarningShowing ? "АКТИВНО" : "НЕТ";
        Color statusColor = isWarningShowing ? Color.LightCoral : Color.LightGreen;

        statusLabel.BackColor = statusColor;

        statusLabel.Text =
            $"? Статус памяти:\n" +
            $"────────────────────\n" +
            $"Доступно: {availableMB:F2} МБ\n" +
            $"Лимит: {limitMB:F2} МБ\n" +
            $"Всего ОЗУ: {totalMB:F2} МБ ({totalMB / 1024:F1} ГБ)\n" +
            $"Используется: {(totalMB - availableMB):F2} МБ\n" +
            $"Процент свободной: {(availableMB / totalMB * 100):F1}%\n" +
            $"Процент лимита: {PERCENTAGE_THRESHOLD}%\n" +
            $"Предупреждение: {warningStatus}\n" +
            $"────────────────────\n" +
            $"Время: {DateTime.Now:HH:mm:ss}\n" +
            $"Проверка каждые 8 секунды";
    }

    // Метод для инициализации компонентов формы
    private void InitializeComponent()
    {
        this.Text = "Проверка памяти"; // Заголовок окна
        this.ClientSize = new Size(450, 410);
        this.StartPosition = FormStartPosition.CenterScreen;

        // Основной лейбл со статусом
        statusLabel = new Label();
        statusLabel.Text = "Загрузка информации о памяти...";
        statusLabel.Dock = DockStyle.Fill;
        statusLabel.TextAlign = ContentAlignment.MiddleCenter;
        statusLabel.Font = new Font("Consolas", 9);
        statusLabel.BorderStyle = BorderStyle.FixedSingle;

        // Панель с кнопками
        Panel buttonPanel = new Panel()
        {
            Dock = DockStyle.Bottom,
            Height = 155
        };

        Button checkButton = new Button()
        {
            Text = "Проверить сейчас (F1)",
            Size = new Size(180, 35),
            Location = new Point(10, 10)
        };

        Button closeWarningButton = new Button()
        {
            Text = "Закрыть предупреждение",
            Size = new Size(180, 35),
            Location = new Point(200, 10),
            Enabled = false
        };

        Button settingsButton = new Button()
        {
            Text = "Параметры",
            Size = new Size(180, 35),
            Location = new Point(10, 55)
        };

        Label paramsLabel = new Label();
        paramsLabel.Text = $"Текущие параметры:\n" +
                          $"(Изменить можно в сборке SC)\n" +  // <-- ДОБАВЛЕНА ЭТА СТРОКА
                          $"Процент от ОЗУ: {PERCENTAGE_THRESHOLD}%\n" +
                          $"Мин.лимит: {MIN_MEMORY_MB} МБ\n" +
                          $"Макс.лимит: {MAX_MEMORY_MB} МБ";
        paramsLabel.Size = new Size(180, 75); // <-- X горисонталь, Y вертикаль
        paramsLabel.Location = new Point(200, 55);
        paramsLabel.TextAlign = ContentAlignment.MiddleCenter;
        paramsLabel.Font = new Font("Arial", 8);
        paramsLabel.BackColor = Color.LightGray;
        paramsLabel.BorderStyle = BorderStyle.FixedSingle;

        checkButton.Click += (s, e) => CheckMemory();
        closeWarningButton.Click += (s, e) =>
        {
            if (currentWarningForm != null && !currentWarningForm.IsDisposed)
            {
                currentWarningForm.Close();
                isWarningShowing = false;
                currentWarningForm = null;
            }
        };

        settingsButton.Click += (s, e) =>
        {
            MessageBox.Show(
                $"Для изменения параметров необходимо отредактировать код программы:\n\n" +
                $"PERCENTAGE_THRESHOLD = {PERCENTAGE_THRESHOLD}%\n" +
                $"MIN_MEMORY_MB = {MIN_MEMORY_MB} МБ\n" +
                $"MAX_MEMORY_MB = {MAX_MEMORY_MB} МБ",
                "Параметры программы",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        };

        // Обновляем состояние кнопки закрытия предупреждения по таймеру
        System.Windows.Forms.Timer uiTimer = new System.Windows.Forms.Timer();
        uiTimer.Interval = 1000;
        uiTimer.Tick += (s, e) =>
        {
            closeWarningButton.Enabled = isWarningShowing;
            closeWarningButton.BackColor = isWarningShowing ? Color.LightCoral : SystemColors.Control;
        };
        uiTimer.Start();

        buttonPanel.Controls.Add(checkButton);
        buttonPanel.Controls.Add(closeWarningButton);
        buttonPanel.Controls.Add(settingsButton);
        buttonPanel.Controls.Add(paramsLabel);

        // Добавляем горячие клавиши
        this.KeyPreview = true;
        this.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.F1) CheckMemory();
            if (e.KeyCode == Keys.F2 && isWarningShowing && currentWarningForm != null)
                currentWarningForm.Close();
        };

        this.Controls.Add(statusLabel);
        this.Controls.Add(buttonPanel);
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
