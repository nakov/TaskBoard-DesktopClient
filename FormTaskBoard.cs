using System;
using System.Net;
using System.Collections.Generic;
using System.Windows.Forms;
using RestSharp;
using RestSharp.Serialization.Json;
using System.Drawing;
using System.Linq;

namespace TaskBoard_DesktopClient
{
    public partial class FormTaskBoard : Form
    {
        private string apiBaseUrl;

        public FormTaskBoard()
        {
            InitializeComponent();
        }

        private void TaskBoardForm_Shown(object sender, EventArgs e)
        {
            var formConnect = new FormConnect();
            if (formConnect.ShowDialog() == DialogResult.OK)
            {
                this.apiBaseUrl = formConnect.ApiUrl;
                LoadTasks();
            }
            else
            {
                this.Close();
            }
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            LoadTasks(this.textBoxSearchText.Text);
        }

        private void buttonReload_Click(object sender, EventArgs e)
        {
            this.textBoxSearchText.Text = "";
            LoadTasks();
        }

        private async void LoadTasks(string searchKeyword = "")
        {
            try
            {
                var restClient = new RestClient(this.apiBaseUrl) { Timeout = 3000 };
                var request = new RestRequest("/tasks", Method.GET);
                if (searchKeyword == "")
                    ShowMsg("Loading tasks ...");
                else
                {
                    ShowMsg($"Searching for tasks by keyword: {searchKeyword} ...");
                    request.Resource = "/tasks/search/{:keyword}";
                    request.AddUrlSegment(":keyword", searchKeyword);
                }
                var response = await restClient.ExecuteAsync(request);
                if (response.IsSuccessful & response.StatusCode == HttpStatusCode.OK)
                {
                    // Visualize the returned tasks
                    var tasks = new JsonDeserializer().Deserialize<List<Task>>(response);
                    if (tasks.Count > 0)
                        ShowSuccessMsg($"Search successful: {tasks.Count} tasks loaded.");
                    else
                        ShowSuccessMsg($"No tasks match your search.");
                    DisplayTasksInListView(tasks);
                }
                else
                    ShowError(response);
            }
            catch (Exception ex)
            {
                ShowErrorMsg(ex.Message);
            }
        }

        private void DisplayTasksInListView(List<Task> tasks)
        {
            this.listViewTasks.Clear();

            // Create column headers
            var headers = new ColumnHeader[] {
                new ColumnHeader { Text = "Id", Width = 50 },
                new ColumnHeader { Text = "Tile", Width = 200 },
                new ColumnHeader { Text = "Description", Width = 400 },
                new ColumnHeader { Text = "Date", Width = 200 }
            };
            this.listViewTasks.Columns.AddRange(headers);

            // Add items and groups to the ListView control
            var groups = new Dictionary<string, ListViewGroup>();
            foreach (var task in tasks)
            {
                var item = new ListViewItem(new string[] {
                    "" + task.Id, task.Title, task.Description, task.DateModified });
                if (!groups.ContainsKey(task.Board.Name))
                {
                    var newGroup = new ListViewGroup("ddd") {
                        Header = task.Board.Name,
                        Tag = task.Board.Id 
                    };
                    groups[task.Board.Name] = newGroup;
                }
                item.Group = groups[task.Board.Name];
                this.listViewTasks.Items.Add(item);
            }

            var sortedGroups = groups.Values.OrderBy(g => (int)g.Tag).ToArray();
            this.listViewTasks.Groups.AddRange(sortedGroups);
        }

        private async void buttonAdd_Click(object sender, EventArgs e)
        {
            var formCreateTask = new FormCreateTask();
            if (formCreateTask.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var restClient = new RestClient(this.apiBaseUrl) { Timeout = 3000 };
                    var request = new RestRequest("/tasks", Method.POST);
                    request.AddJsonBody(new {
                        title = formCreateTask.Title,
                        description = formCreateTask.Description
                    });
                    ShowMsg($"Creating new task ...");
                    var response = await restClient.ExecuteAsync(request);
                    if (response.IsSuccessful & response.StatusCode == HttpStatusCode.Created)
                    {
                        ShowSuccessMsg($"Task created.");
                        LoadTasks();
                    }
                    else
                        ShowError(response);
                }
                catch (Exception ex)
                {
                    ShowErrorMsg(ex.Message);
                }
            }
        }

        private void ShowError(IRestResponse response)
        {
            if (string.IsNullOrWhiteSpace(response.ErrorMessage))
            {
                string errText = $"HTTP error `{response.StatusCode}`.";
                if (!string.IsNullOrWhiteSpace(response.Content))
                    errText += $" Details: {response.Content}";
                ShowErrorMsg(errText);
            }
            else
                ShowErrorMsg($"HTTP error `{response.ErrorMessage}`.");
        }

        private void ShowMsg(string msg)
        {
            toolStripStatusLabel.Text = msg;
            toolStripStatusLabel.ForeColor = SystemColors.ControlText;
            toolStripStatusLabel.BackColor = SystemColors.Control;
        }

        private void ShowSuccessMsg(string msg)
        {
            toolStripStatusLabel.Text = msg;
            toolStripStatusLabel.ForeColor = Color.White;
            toolStripStatusLabel.BackColor = Color.Green;
        }

        private void ShowErrorMsg(string errMsg)
        {
            toolStripStatusLabel.Text = $"Error: {errMsg}";
            toolStripStatusLabel.ForeColor = Color.White;
            toolStripStatusLabel.BackColor = Color.Red;
        }
    }
}
