using System.Diagnostics;
using System.Drawing;

namespace FileExplorer;

public sealed class ExplorerForm : Form
{
    private readonly MenuStrip menuStrip = new();
    private readonly ToolStrip toolStrip = new();
    private readonly ToolStripTextBox addressBox = new() { AutoSize = false, Width = 520 };
    private readonly TreeView directoryTree = new();
    private readonly ListView contentList = new();
    private readonly SplitContainer splitContainer = new();
    private string currentPath = string.Empty;
    private bool isUpdatingTree;

    public ExplorerForm()
    {
        Text = "Проводник";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1100, 700);
        MinimumSize = new Size(750, 450);
        KeyPreview = true;

        BuildMenu();
        BuildToolBar();
        BuildMainArea();
        BuildContextMenus();

        MainMenuStrip = menuStrip;
        Controls.Add(splitContainer);
        Controls.Add(toolStrip);
        Controls.Add(menuStrip);

        Load += (_, _) => LoadDrives();
        KeyDown += ExplorerForm_KeyDown;
    }

    private void BuildMenu()
    {
        var fileMenu = new ToolStripMenuItem("Файл");
        fileMenu.DropDownItems.Add(CreateMenuItem("Открыть адрес", Keys.Control | Keys.L, (_, _) => NavigateToAddress()));
        fileMenu.DropDownItems.Add(CreateMenuItem("На уровень вверх", Keys.Alt | Keys.Up, (_, _) => GoUp()));
        fileMenu.DropDownItems.Add(CreateMenuItem("Обновить", Keys.F5, (_, _) => RefreshCurrentFolder()));
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add(CreateMenuItem("Выход", Keys.Alt | Keys.F4, (_, _) => Close()));

        var editMenu = new ToolStripMenuItem("Правка");
        editMenu.DropDownItems.Add(CreateMenuItem("Копировать адрес", Keys.Control | Keys.Shift | Keys.C, (_, _) => CopyCurrentPath()));

        var helpMenu = new ToolStripMenuItem("Справка");
        helpMenu.DropDownItems.Add(new ToolStripMenuItem("О программе", null, (_, _) =>
            MessageBox.Show("Учебное приложение «Проводник». Просмотр дисков, папок и файлов.", "О программе", MessageBoxButtons.OK, MessageBoxIcon.Information)));

        menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, editMenu, helpMenu });
        menuStrip.Dock = DockStyle.Top;
    }

    private static ToolStripMenuItem CreateMenuItem(string text, Keys shortcut, EventHandler handler)
    {
        var item = new ToolStripMenuItem(text)
        {
            ShortcutKeys = shortcut,
            ShowShortcutKeys = true
        };
        item.Click += handler;
        return item;
    }

    private void BuildToolBar()
    {
        toolStrip.Dock = DockStyle.Top;
        toolStrip.GripStyle = ToolStripGripStyle.Hidden;
        toolStrip.Items.Add(CreateButton("Вверх", (_, _) => GoUp()));
        toolStrip.Items.Add(CreateButton("Обновить", (_, _) => RefreshCurrentFolder()));
        toolStrip.Items.Add(new ToolStripSeparator());
        toolStrip.Items.Add(new ToolStripLabel("Адрес:"));
        addressBox.ToolTipText = "Введите путь к папке и нажмите Enter";
        addressBox.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                NavigateToAddress();
                e.SuppressKeyPress = true;
            }
        };
        toolStrip.Items.Add(addressBox);
        toolStrip.Items.Add(CreateButton("Перейти", (_, _) => NavigateToAddress()));
    }

    private static ToolStripButton CreateButton(string text, EventHandler handler)
    {
        var button = new ToolStripButton(text) { DisplayStyle = ToolStripItemDisplayStyle.Text };
        button.Click += handler;
        return button;
    }

    private void BuildMainArea()
    {
        splitContainer.Dock = DockStyle.Fill;
        splitContainer.SplitterDistance = 300;
        splitContainer.Panel1MinSize = 180;
        splitContainer.Panel2MinSize = 300;

        directoryTree.Dock = DockStyle.Fill;
        directoryTree.HideSelection = false;
        directoryTree.BeforeExpand += DirectoryTree_BeforeExpand;
        directoryTree.AfterSelect += (_, e) =>
        {
            if (e.Node.Tag is string path && Directory.Exists(path))
                ShowDirectory(path);
        };
        directoryTree.NodeMouseDoubleClick += (_, e) =>
        {
            if (e.Node.Tag is string path && Directory.Exists(path))
                ShowDirectory(path);
        };

        contentList.Dock = DockStyle.Fill;
        contentList.View = View.Details;
        contentList.FullRowSelect = true;
        contentList.GridLines = true;
        contentList.HideSelection = false;
        contentList.Columns.Add("Имя", 330);
        contentList.Columns.Add("Тип", 120);
        contentList.Columns.Add("Размер", 120);
        contentList.Columns.Add("Дата изменения", 160);
        contentList.DoubleClick += ContentList_DoubleClick;

        splitContainer.Panel1.Controls.Add(directoryTree);
        splitContainer.Panel2.Controls.Add(contentList);
    }

    private void BuildContextMenus()
    {
        var treeMenu = new ContextMenuStrip();
        treeMenu.Items.Add("Открыть", null, (_, _) =>
        {
            if (directoryTree.SelectedNode?.Tag is string path && Directory.Exists(path))
                ShowDirectory(path);
        });
        treeMenu.Items.Add("Обновить", null, (_, _) => RefreshCurrentFolder());
        treeMenu.Items.Add(new ToolStripSeparator());
        treeMenu.Items.Add("Копировать путь", null, (_, _) => CopyCurrentPath());
        directoryTree.ContextMenuStrip = treeMenu;

        var listMenu = new ContextMenuStrip();
        listMenu.Items.Add("Открыть", null, (_, _) => OpenSelectedItem());
        listMenu.Items.Add("Копировать путь", null, (_, _) => CopySelectedPath());
        listMenu.Items.Add(new ToolStripSeparator());
        listMenu.Items.Add("Обновить", null, (_, _) => RefreshCurrentFolder());
        contentList.ContextMenuStrip = listMenu;
    }

    private void LoadDrives()
    {
        isUpdatingTree = true;
        directoryTree.BeginUpdate();
        directoryTree.Nodes.Clear();

        foreach (var driveName in DriveInfo.GetDrives().Select(d => d.Name).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            var driveNode = new TreeNode(driveName) { Tag = driveName };
            AddDummyChildIfHasFolders(driveNode, driveName);
            directoryTree.Nodes.Add(driveNode);
        }

        directoryTree.EndUpdate();
        isUpdatingTree = false;

        if (directoryTree.Nodes.Count > 0)
        {
            directoryTree.SelectedNode = directoryTree.Nodes[0];
            directoryTree.Nodes[0].Expand();
            if (directoryTree.Nodes[0].Tag is string firstDrive)
                ShowDirectory(firstDrive);
        }
        else
        {
            addressBox.Text = string.Empty;
            contentList.Items.Clear();
        }
    }

    private void AddDummyChildIfHasFolders(TreeNode parent, string path)
    {
        try
        {
            if (Directory.GetDirectories(path).Length > 0)
                parent.Nodes.Add(new TreeNode("...") { Tag = null });
        }
        catch (UnauthorizedAccessException) { }
        catch (IOException) { }
        catch (System.Security.SecurityException) { }
    }

    private void DirectoryTree_BeforeExpand(object? sender, TreeViewCancelEventArgs e)
    {
        if (e.Node.Tag is not string path) return;
        if (e.Node.Nodes.Count != 1 || e.Node.Nodes[0].Tag is not null) return;

        e.Node.Nodes.Clear();
        try
        {
            foreach (var directory in Directory.GetDirectories(path).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            {
                var info = new DirectoryInfo(directory);
                var child = new TreeNode(info.Name) { Tag = directory };
                AddDummyChildIfHasFolders(child, directory);
                e.Node.Nodes.Add(child);
            }
        }
        catch (UnauthorizedAccessException) { }
        catch (IOException) { }
        catch (System.Security.SecurityException) { }
    }

    private void ShowDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            MessageBox.Show("Папка не найдена или к ней нет доступа.", "Проводник", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        currentPath = Path.GetFullPath(path);
        addressBox.Text = currentPath;
        Text = $"{currentPath} — Проводник";
        contentList.BeginUpdate();
        contentList.Items.Clear();

        try
        {
            foreach (var directory in Directory.GetDirectories(currentPath).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            {
                var info = new DirectoryInfo(directory);
                var item = new ListViewItem(info.Name) { Tag = directory };
                item.SubItems.Add("Папка");
                item.SubItems.Add(string.Empty);
                item.SubItems.Add(info.LastWriteTime.ToString("dd.MM.yyyy HH:mm"));
                contentList.Items.Add(item);
            }
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or System.Security.SecurityException)
        {
            contentList.EndUpdate();
            MessageBox.Show($"Не удалось прочитать папки.\n{ex.Message}", "Ошибка доступа", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            foreach (var file in Directory.GetFiles(currentPath).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            {
                var info = new FileInfo(file);
                var item = new ListViewItem(info.Name) { Tag = file };
                item.SubItems.Add(string.IsNullOrEmpty(info.Extension) ? "Файл" : info.Extension);
                item.SubItems.Add(FormatSize(info.Length));
                item.SubItems.Add(info.LastWriteTime.ToString("dd.MM.yyyy HH:mm"));
                contentList.Items.Add(item);
            }
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or System.Security.SecurityException)
        {
            MessageBox.Show($"Не удалось прочитать файлы.\n{ex.Message}", "Ошибка доступа", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            contentList.EndUpdate();
        }
    }

    private static string FormatSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} Б";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} КБ";
        if (bytes < 1024L * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F1} МБ";
        return $"{bytes / (1024.0 * 1024 * 1024):F1} ГБ";
    }

    private void NavigateToAddress()
    {
        var path = addressBox.Text.Trim().Trim('"');
        if (string.IsNullOrWhiteSpace(path)) return;
        if (!Directory.Exists(path))
        {
            MessageBox.Show("Указанный путь не существует или не является папкой.", "Проводник", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        ShowDirectory(path);
        SelectTreeNodeByPath(path);
    }

    private void SelectTreeNodeByPath(string path)
    {
        var normalized = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        foreach (TreeNode driveNode in directoryTree.Nodes)
        {
            var found = FindNode(driveNode, normalized);
            if (found is not null)
            {
                directoryTree.SelectedNode = found;
                found.EnsureVisible();
                return;
            }
        }
    }

    private TreeNode? FindNode(TreeNode node, string normalizedPath)
    {
        if (node.Tag is string nodePath &&
            string.Equals(Path.GetFullPath(nodePath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), normalizedPath, StringComparison.OrdinalIgnoreCase))
            return node;

        if (node.Tag is string path && normalizedPath.StartsWith(path, StringComparison.OrdinalIgnoreCase))
        {
            if (node.Nodes.Count == 1 && node.Nodes[0].Tag is null)
                node.Expand();

            foreach (TreeNode child in node.Nodes)
            {
                var found = FindNode(child, normalizedPath);
                if (found is not null) return found;
            }
        }

        return null;
    }

    private void GoUp()
    {
        if (string.IsNullOrWhiteSpace(currentPath)) return;
        var parent = Directory.GetParent(currentPath);
        if (parent is not null)
        {
            ShowDirectory(parent.FullName);
            SelectTreeNodeByPath(parent.FullName);
        }
    }

    private void RefreshCurrentFolder()
    {
        if (!string.IsNullOrWhiteSpace(currentPath) && Directory.Exists(currentPath))
            ShowDirectory(currentPath);
        else
            LoadDrives();
    }

    private void ContentList_DoubleClick(object? sender, EventArgs e) => OpenSelectedItem();

    private void OpenSelectedItem()
    {
        if (contentList.SelectedItems.Count == 0) return;
        var path = contentList.SelectedItems[0].Tag as string;
        if (string.IsNullOrWhiteSpace(path)) return;

        if (Directory.Exists(path))
        {
            ShowDirectory(path);
            SelectTreeNodeByPath(path);
        }
        else if (File.Exists(path))
        {
            try
            {
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть файл.\n{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void CopyCurrentPath()
    {
        if (!string.IsNullOrWhiteSpace(currentPath))
            Clipboard.SetText(currentPath);
    }

    private void CopySelectedPath()
    {
        if (contentList.SelectedItems.Count == 0) return;
        if (contentList.SelectedItems[0].Tag is string path)
            Clipboard.SetText(path);
    }

    private void ExplorerForm_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.F5)
        {
            RefreshCurrentFolder();
            e.Handled = true;
        }
        else if (e.Control && e.KeyCode == Keys.L)
        {
            addressBox.Focus();
            addressBox.SelectAll();
            e.Handled = true;
        }
        else if (e.Alt && e.KeyCode == Keys.Up)
        {
            GoUp();
            e.Handled = true;
        }
        else if (e.Control && e.Shift && e.KeyCode == Keys.C)
        {
            CopyCurrentPath();
            e.Handled = true;
        }
    }
}
