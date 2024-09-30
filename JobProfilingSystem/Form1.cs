using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace JobProfilingSystem
{
    public partial class Form1 : Form
    {
        private JobScheduler scheduler;
        private TextBox txtJobName;
        private ComboBox cmbPriority;
        private Button btnAddJob;
        private Button btnStartScheduler;
        private Button btnStopScheduler;
        private ListView lstJobs;
        private Label lblJobCount;
        private Label lblCompletedJobs;
        private RichTextBox txtLog;

        public Form1()
        {   
            InitializeComponent();
            InitializCustomeComponents();
            InitializeScheduler();
        }

        private void LogMessage(string message)
        {
            // Ensure that we're updating the RichTextBox on the UI thread.
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action<string>(LogMessage), message);
            }
            else
            {
                txtLog.AppendText($"{DateTime.Now}: {message}\n");
                txtLog.ScrollToCaret();  // Automatically scroll to the bottom
            }
        }

        private void InitializCustomeComponents()
        {
            this.Size = new Size(600, 600);
            this.Text = "Job Scheduler Demo";

            txtJobName = new TextBox { Location = new Point(10, 10), Size = new Size(150, 20) };
            cmbPriority = new ComboBox { Location = new Point(170, 10), Size = new Size(100, 20) };
            cmbPriority.Items.AddRange(Enum.GetNames(typeof(JobPriority)));
            cmbPriority.SelectedIndex = 0;
            txtLog = new RichTextBox
            {
                Location = new Point(10, 370),
                Size = new Size(570, 200),
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };
            btnAddJob = new Button { Text = "Add Job", Location = new Point(280, 10), Size = new Size(100, 20) };
            btnAddJob.Click += BtnAddJob_Click;

            btnStartScheduler = new Button { Text = "Start Scheduler", Location = new Point(10, 40), Size = new Size(120, 20) };
            btnStartScheduler.Click += BtnStartScheduler_Click;

            btnStopScheduler = new Button { Text = "Stop Scheduler", Location = new Point(140, 40), Size = new Size(120, 20) };
            btnStopScheduler.Click += BtnStopScheduler_Click;
            btnStopScheduler.Enabled = false;

            lstJobs = new ListView { Location = new Point(10, 70), Size = new Size(570, 250) };
            lstJobs.View = View.Details;
            lstJobs.Columns.Add("Name", 150);
            lstJobs.Columns.Add("Priority", 80);
            lstJobs.Columns.Add("Status", 80);
            lstJobs.Columns.Add("Execution Time", 100);
            // Add context menu for job cancellation
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            ToolStripMenuItem cancelJobItem = new ToolStripMenuItem("Cancel Job");
            cancelJobItem.Click += CancelJob_Click;
            contextMenu.Items.Add(cancelJobItem);
            lstJobs.ContextMenuStrip = contextMenu;
            lstJobs.FullRowSelect = true;

            lblJobCount = new Label { Text = "Jobs in queue: 0", Location = new Point(10, 330), Size = new Size(150, 20) };
            lblCompletedJobs = new Label { Text = "Completed jobs: 0", Location = new Point(170, 330), Size = new Size(150, 20) };

            this.Controls.AddRange(new Control[] { txtJobName, cmbPriority, btnAddJob, btnStartScheduler, btnStopScheduler, lstJobs, lblJobCount, lblCompletedJobs });

            

            

            this.Controls.AddRange(new Control[] { txtJobName, cmbPriority, btnAddJob, btnStartScheduler, btnStopScheduler, lstJobs, lblJobCount, lblCompletedJobs, txtLog });

        }

        private void InitializeScheduler()
        {
            scheduler = new JobScheduler(LogMessage);
            scheduler.JobStarted += Scheduler_JobStarted;
            scheduler.JobCompleted += Scheduler_JobCompleted;
        }

        private void BtnAddJob_Click(object sender, EventArgs e)
        {
            string jobName = txtJobName.Text;
            if (string.IsNullOrWhiteSpace(jobName))
            {
                MessageBox.Show("Please enter a job name.");
                return;
            }

            JobPriority priority = (JobPriority)Enum.Parse(typeof(JobPriority), cmbPriority.SelectedItem.ToString());
            Random random = new Random();
            List<int> data = new List<int>();
            int dataSize = random.Next(15, 50); // Generate random list size between 5 and 15 elements
            for (int i = 0; i < dataSize; i++)
            {
                data.Add(random.Next(1, 101)); // Random values between 1 and 100
            }
            SortingJobSystem job;
            if (jobName.Equals("Bubble Sort", StringComparison.OrdinalIgnoreCase))
            {
                job = new BubbleSortJob(data, LogMessage);
            }
            else if (jobName.Equals("Quick Sort", StringComparison.OrdinalIgnoreCase))
            {
                job = new QuickSortJob(data, LogMessage);
            }
            else
            {
                MessageBox.Show("Unsupported job type. Please enter 'Bubble Sort' or 'Quick Sort'.");
                return;
            }

            scheduler.AddJob(job, priority);
            AddJobToListView(job, priority, "Queued");
            UpdateJobCount();

            txtJobName.Clear();
        }

        private void BtnStartScheduler_Click(object sender, EventArgs e)
        {
            scheduler.Start();
            btnStartScheduler.Enabled = false;
            btnStopScheduler.Enabled = true;
        }

        private void BtnStopScheduler_Click(object sender, EventArgs e)
        {
            scheduler.Stop();
            btnStartScheduler.Enabled = true;
            btnStopScheduler.Enabled = false;
        }

        private void UpdateJobCount()
        {
            this.Invoke((MethodInvoker)delegate
            {
                lblJobCount.Text = $"Jobs in queue: {scheduler.JobCount}";
            });
        }

        private void AddJobToListView(SortingJobSystem job, JobPriority priority, string status)
        {
            ListViewItem item = new ListViewItem(new[] { job.Name, priority.ToString(), status, "-" });
            item.Tag = job;
            this.Invoke((MethodInvoker)delegate
            {
                lstJobs.Items.Add(item);
            });
        }

        private void UpdateJobInListView(SortingJobSystem job, string status, double executionTime = 0)
        {
            this.Invoke((MethodInvoker)delegate
            {
                foreach (ListViewItem item in lstJobs.Items)
                {
                    if ((SortingJobSystem)item.Tag == job)
                    {
                        item.SubItems[2].Text = status;
                        item.SubItems[3].Text = executionTime > 0 ? $"{executionTime:F2}s" : "-";
                        break;
                    }
                }
            });
        }

        private void Scheduler_JobStarted(object sender, JobEventArgs e)
        {
            UpdateJobInListView(e.Job, "Running");
        }

        private void Scheduler_JobCompleted(object sender, JobEventArgs e)
        {
            UpdateJobInListView(e.Job, "Completed", e.Job.LastExecutionTime);
            UpdateJobCount();
            UpdateCompletedJobCount();
        }

        private void UpdateCompletedJobCount()
        {
            this.Invoke((MethodInvoker)delegate
            {
                int completedCount = 0;
                foreach (ListViewItem item in lstJobs.Items)
                {
                    if (item.SubItems[2].Text == "Completed")
                        completedCount++;
                }
                lblCompletedJobs.Text = $"Completed jobs: {completedCount}";
            });
        }

        private void CancelJob_Click(object sender, EventArgs e)
        {
            if (lstJobs.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = lstJobs.SelectedItems[0];
                SortingJobSystem job = (SortingJobSystem)selectedItem.Tag;
                if (scheduler.CancelJob(job))
                {
                    UpdateJobInListView(job, "Cancelled");
                    UpdateJobCount();
                }
                else
                {
                    MessageBox.Show("Unable to cancel the job. It may have already started or completed.");
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            scheduler.Stop();
            base.OnFormClosing(e);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }

    // Updates to JobScheduler and related classes:

    public class JobEventArgs : EventArgs
    {
        public SortingJobSystem Job { get; set; }
    }

       

}
