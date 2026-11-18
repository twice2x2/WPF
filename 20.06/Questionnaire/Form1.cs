using System;
using System.Linq;
using System.Windows.Forms;

namespace Task2_Questionnaire;

public class Form1 : Form
{
    private readonly TextBox txtName;
    private readonly NumericUpDown numAge;
    private readonly ComboBox cmbEducation;
    private readonly RadioButton rbMale;
    private readonly RadioButton rbFemale;
    private readonly TextBox txtCity;
    private readonly CheckedListBox clbHobbies;
    private readonly TextBox txtComment;
    private readonly TextBox txtResult;
    private readonly Button btnShow;
    private readonly Button btnClear;

    public Form1()
    {
        Text = "Анкета пользователя";
        StartPosition = FormStartPosition.CenterScreen;
        Width = 820;
        Height = 760;
        MinimumSize = new System.Drawing.Size(700, 650);
        AutoScroll = true;

        Label title = new Label
        {
            Text = "АНКЕТА",
            Left = 20,
            Top = 15,
            Width = 740,
            Height = 35,
            Font = new System.Drawing.Font("Segoe UI", 16, System.Drawing.FontStyle.Bold),
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        };

        Controls.Add(title);

        AddLabel("Фамилия, имя:", 25, 70);
        txtName = new TextBox { Left = 190, Top = 66, Width = 300 };

        AddLabel("Возраст:", 25, 112);
        numAge = new NumericUpDown
        {
            Left = 190,
            Top = 108,
            Width = 100,
            Minimum = 1,
            Maximum = 100,
            Value = 18
        };

        AddLabel("Образование:", 25, 154);
        cmbEducation = new ComboBox
        {
            Left = 190,
            Top = 150,
            Width = 300,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbEducation.Items.AddRange(new object[]
        {
            "Среднее",
            "Среднее специальное",
            "Неоконченное высшее",
            "Высшее"
        });
        cmbEducation.SelectedIndex = 0;

        GroupBox genderGroup = new GroupBox
        {
            Text = "Пол",
            Left = 20,
            Top = 195,
            Width = 470,
            Height = 60
        };
        rbMale = new RadioButton { Text = "Мужской", Left = 15, Top = 25, AutoSize = true, Checked = true };
        rbFemale = new RadioButton { Text = "Женский", Left = 130, Top = 25, AutoSize = true };
        genderGroup.Controls.Add(rbMale);
        genderGroup.Controls.Add(rbFemale);

        AddLabel("Город:", 25, 272);
        txtCity = new TextBox { Left = 190, Top = 268, Width = 300 };

        AddLabel("Увлечения:", 25, 314);
        clbHobbies = new CheckedListBox
        {
            Left = 190,
            Top = 310,
            Width = 300,
            Height = 105,
            CheckOnClick = true
        };
        clbHobbies.Items.AddRange(new object[]
        {
            "Спорт",
            "Музыка",
            "Чтение",
            "Компьютеры",
            "Путешествия"
        });

        AddLabel("О себе:", 25, 430);
        txtComment = new TextBox
        {
            Left = 190,
            Top = 426,
            Width = 520,
            Height = 65,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical
        };

        btnShow = new Button
        {
            Text = "Показать результат",
            Left = 190,
            Top = 505,
            Width = 160,
            Height = 35
        };
        btnShow.Click += BtnShow_Click;

        btnClear = new Button
        {
            Text = "Очистить",
            Left = 365,
            Top = 505,
            Width = 125,
            Height = 35
        };
        btnClear.Click += BtnClear_Click;

        AddLabel("Результат заполнения анкеты:", 25, 555);
        txtResult = new TextBox
        {
            Left = 25,
            Top = 585,
            Width = 750,
            Height = 115,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        Controls.Add(txtName);
        Controls.Add(numAge);
        Controls.Add(cmbEducation);
        Controls.Add(genderGroup);
        Controls.Add(txtCity);
        Controls.Add(clbHobbies);
        Controls.Add(txtComment);
        Controls.Add(btnShow);
        Controls.Add(btnClear);
        Controls.Add(txtResult);
    }

    private void AddLabel(string text, int left, int top)
    {
        Controls.Add(new Label
        {
            Text = text,
            Left = left,
            Top = top,
            Width = 155,
            Height = 25,
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        });
    }

    private void BtnShow_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            MessageBox.Show("Введите фамилию и имя.", "Проверка анкеты",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            txtName.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(txtCity.Text))
        {
            MessageBox.Show("Введите город.", "Проверка анкеты",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            txtCity.Focus();
            return;
        }

        string gender = rbMale.Checked ? "Мужской" : "Женский";
        string hobbies = clbHobbies.CheckedItems.Count > 0
            ? string.Join(", ", clbHobbies.CheckedItems.Cast<object>())
            : "Не указаны";

        txtResult.Text =
            "АНКЕТА ПОЛЬЗОВАТЕЛЯ" + Environment.NewLine +
            "Фамилия, имя: " + txtName.Text.Trim() + Environment.NewLine +
            "Возраст: " + numAge.Value + Environment.NewLine +
            "Образование: " + cmbEducation.Text + Environment.NewLine +
            "Пол: " + gender + Environment.NewLine +
            "Город: " + txtCity.Text.Trim() + Environment.NewLine +
            "Увлечения: " + hobbies + Environment.NewLine +
            "О себе: " + (string.IsNullOrWhiteSpace(txtComment.Text) ? "Не указано" : txtComment.Text.Trim());
    }

    private void BtnClear_Click(object? sender, EventArgs e)
    {
        txtName.Clear();
        numAge.Value = 18;
        cmbEducation.SelectedIndex = 0;
        rbMale.Checked = true;
        txtCity.Clear();
        for (int i = 0; i < clbHobbies.Items.Count; i++)
            clbHobbies.SetItemChecked(i, false);
        txtComment.Clear();
        txtResult.Clear();
        txtName.Focus();
    }
}
