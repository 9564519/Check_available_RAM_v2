using System;
using System.Windows.Forms;

public class WarningForm : Form
{
    private Label messageLabel;
    private Button okButton;
    private Button exitButton;

    public WarningForm(string message)
    {
        // Инициализация компонентов
        messageLabel = new Label()
        {
            Text = message,
            Dock = DockStyle.Top,
            Height = 60,
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        };

        okButton = new Button()
        {
            Text = "ОК",
            DialogResult = DialogResult.OK,
            Dock = DockStyle.Left
        };

        exitButton = new Button()
        {
            Text = "Выход",
            Dock = DockStyle.Right
        };

        exitButton.Click += (sender, e) => Application.Exit(); // Завершает приложение при нажатии

        // Добавляем элементы управления в форму
        this.Controls.Add(messageLabel);
        this.Controls.Add(okButton);
        this.Controls.Add(exitButton);

        // Настройка формы
        this.Text = "Предупреждение";
        this.StartPosition = FormStartPosition.CenterParent;
        this.ClientSize = new System.Drawing.Size(400, 120);
    }
}
