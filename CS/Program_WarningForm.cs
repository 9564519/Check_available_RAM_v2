using System;
using System.Windows.Forms;
using System.Drawing;

public class WarningForm : Form
{
    private System.Windows.Forms.Timer autoCloseTimer;

    public WarningForm(string message, double availableMemoryMB, double limitMB, double totalMemoryMB)
    {
        // Основное сообщение
        Label messageLabel = new Label()
        {
            Text = message,
            Dock = DockStyle.Top,
            Height = 60,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Arial", 10, FontStyle.Bold),
            ForeColor = Color.DarkRed,
            BackColor = Color.LightYellow
        };

        // Детальная информация
        Label detailsLabel = new Label()
        {
            Text = $"Доступно: {availableMemoryMB:F2} МБ\n" +
                   $"Лимит: {limitMB:F2} МБ\n" +
                   $"Всего ОЗУ: {totalMemoryMB:F2} МБ ({totalMemoryMB / 1024:F1} ГБ)\n" +
                   $"Процент свободной памяти: {(availableMemoryMB / totalMemoryMB * 100):F1}%",
            Dock = DockStyle.Top,
            Height = 80,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Arial", 9),
            BorderStyle = BorderStyle.FixedSingle
        };

        // Панель для кнопок
        Panel buttonPanel = new Panel()
        {
            Dock = DockStyle.Bottom,
            Height = 70
        };

        Button okButton = new Button()
        {
            Text = "Продолжить",
            Size = new Size(120, 35),
            Location = new Point(20, 10)
        };

        Button exitButton = new Button()
        {
            Text = "Выход из программы",
            Size = new Size(160, 35),
            Location = new Point(160, 10),
            BackColor = Color.LightCoral
        };

        // Метка с подсказкой
        Label hintLabel = new Label()
        {
            Text = "Окно закроется автоматически через 4 секунд",
            Location = new Point(20, 50),
            Size = new Size(300, 20),
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Arial", 8),
            ForeColor = Color.DarkGray
        };

        okButton.Click += (sender, e) => this.Close();
        exitButton.Click += (sender, e) => Application.Exit();

        // Добавляем кнопки на панель
        buttonPanel.Controls.Add(okButton);
        buttonPanel.Controls.Add(exitButton);
        buttonPanel.Controls.Add(hintLabel);

        // Добавляем элементы управления в форму
        this.Controls.Add(detailsLabel);
        this.Controls.Add(messageLabel);
        this.Controls.Add(buttonPanel);

        // Настройка формы
        this.Text = "⚠️ Критическое предупреждение о памяти";
        this.StartPosition = FormStartPosition.CenterScreen;
        this.ClientSize = new Size(500, 230);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.TopMost = true; // Всегда поверх других окон

        // Добавляем таймер для автоматического закрытия в милесек
        autoCloseTimer = new System.Windows.Forms.Timer();
        autoCloseTimer.Interval = 4000; // милесек закрытия
        autoCloseTimer.Tick += (s, e) =>
        {
            autoCloseTimer.Stop();
            this.Close();
        };
        autoCloseTimer.Start();

        // Останавливаем таймер при закрытии формы
        this.FormClosed += (s, e) =>
        {
            if (autoCloseTimer != null)
            {
                autoCloseTimer.Stop();
                autoCloseTimer.Dispose();
            }
        };
    }
}
