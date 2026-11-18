using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Task1_ProgressBar;

public class Form1 : Form
{
    private readonly Button btnOpen;
    private readonly ProgressBar progressBar;
    private readonly Label lblCount;
    private readonly TextBox txtContent;

    public Form1()
    {
        Text = "Чтение текста из файла";
        StartPosition = FormStartPosition.CenterScreen;
        Width = 760;
        Height = 520;
        MinimumSize = new System.Drawing.Size(600, 400);

        btnOpen = new Button
        {
            Text = "Открыть файл и прочитать",
            Left = 20,
            Top = 20,
            Width = 220,
            Height = 35
        };
        btnOpen.Click += BtnOpen_Click;

        lblCount = new Label
        {
            Text = "Прочитано символов: 0",
            Left = 260,
            Top = 29,
            Width = 450,
            AutoSize = false
        };

        progressBar = new ProgressBar
        {
            Left = 20,
            Top = 70,
            Width = 700,
            Height = 25,
            Minimum = 0,
            Value = 0,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        txtContent = new TextBox
        {
            Left = 20,
            Top = 115,
            Width = 700,
            Height = 340,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Both,
            WordWrap = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };

        Controls.Add(btnOpen);
        Controls.Add(lblCount);
        Controls.Add(progressBar);
        Controls.Add(txtContent);
    }

    private async void BtnOpen_Click(object? sender, EventArgs e)
    {
        using OpenFileDialog openFileDialog = new OpenFileDialog
        {
            Title = "Выберите текстовый файл",
            Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*"
        };

        if (openFileDialog.ShowDialog() != DialogResult.OK)
            return;

        btnOpen.Enabled = false;
        txtContent.Clear();
        progressBar.Value = 0;
        lblCount.Text = "Подготовка к чтению...";

        try
        {
            string fileName = openFileDialog.FileName;

            // Первый проход нужен, чтобы узнать общее количество символов.
            int totalCharacters = 0;
            char[] countBuffer = new char[4096];
            using (StreamReader counter = new StreamReader(fileName))
            {
                int charsRead;
                while ((charsRead = counter.Read(countBuffer, 0, countBuffer.Length)) > 0)
                    totalCharacters += charsRead;
            }

            progressBar.Maximum = Math.Max(totalCharacters, 1);
            lblCount.Text = $"Прочитано символов: 0 из {totalCharacters}";

            // Второй проход читает файл частями и обновляет ProgressBar.
            int readCharacters = 0;
            char[] buffer = new char[256];
            using (StreamReader reader = new StreamReader(fileName))
            {
                int charsRead;
                while ((charsRead = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    txtContent.AppendText(new string(buffer, 0, charsRead));
                    readCharacters += charsRead;
                    progressBar.Value = Math.Min(readCharacters, progressBar.Maximum);
                    lblCount.Text = $"Прочитано символов: {readCharacters} из {totalCharacters}";
                    await Task.Delay(10); // небольшая задержка, чтобы было видно изменение индикатора
                }
            }

            lblCount.Text = $"Чтение завершено. Всего символов: {readCharacters}";
        }
        catch (Exception ex)
        {
            MessageBox.Show("Не удалось прочитать файл: " + ex.Message,
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            lblCount.Text = "Ошибка чтения файла";
        }
        finally
        {
            btnOpen.Enabled = true;
        }
    }
}
