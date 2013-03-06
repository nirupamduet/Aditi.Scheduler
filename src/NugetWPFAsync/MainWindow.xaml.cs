﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Aditi.Scheduler;
using Aditi.Scheduler.Models;

namespace NugetWPFAsync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ScheduledTasks scheduledTask;

        public MainWindow()
        {
            InitializeComponent();
            scheduledTask = new ScheduledTasks("43336f8e-fa22-4251-bd49-447eeb10f5ab",
                                                          "/HUccm4Bxnsy9UkYIVxijVJ8qjnk1vQeZ0FFT+/uKyE=");
        }

        private async void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            List<Guid> opIds;
            List<OperationStatus> opStatus = null;

            var tasks = new TaskModel[5];
            tasks = GetTasks();
            //Synchronous creation of tasks

            txtStatus.Text += "Starting synchronous task creation.";
            //start creating tasks and get the list of operation Ids
            opIds =  new List<Guid>();

            foreach (TaskModel t in tasks)
            {
                try
                {
                    opIds.Add(scheduledTask.CreateTask(t));
                }
                catch (SchedulerModelValidationException me)
                {
                    txtStatus.Text += "\n" + me.Message;
                    //TODO: Log exception

                }
                catch (SchedulerException schedExp)
                {
                    //TODO: Log exception
                }
            }
            txtStatus.Text += "\nGot operationIds, now checking status";
            //get operation status for each operationId
            if (opIds != null && opIds.Count > 0)
            {
                try
                {
                    opStatus = opIds.Select(t => scheduledTask.GetOperationStatus(t, true)).ToList();
                }
                catch (SchedulerException shedExp)
                {
                    //TODO: Log exception
                }

                if (opStatus != null && opStatus.Count > 0)
                    DisplayOperationStatus(opStatus);
            }
            txtStatus.Text += "\nEnding synchronous task creation.";

            opIds.Clear();
            
            //Asynsynchronous creation of tasks
            txtStatus.Text += "\nStarting asynsynchronous task creation.";
            //opIds = await CreatTasksAsync();
            foreach (TaskModel t in tasks)
            {
                try
                {
                    var currentOperationId = await scheduledTask.CreateTaskAsync(t);
                    opIds.Add(currentOperationId);
                }
                catch (SchedulerModelValidationException me)
                {
                    //write to log
                    txtStatus.Text += "\n" + me.Message;
                }
                catch (SchedulerException exp)
                {
                    //write to log
                }
            }
            txtStatus.Text += "\nGot operationIds, now checking status";
            if (opIds.Count > 0)
            {
                //opStatus = await GetOperationStatusAsync(opIds);
                foreach (Guid t in opIds)
                {
                    try
                    {
                        var currentStatus = await scheduledTask.GetOperationStatusAsync(t, true);
                        opStatus.Add(currentStatus);
                    }
                    catch (SchedulerException exp)
                    {
                        //write log
                    }
                }
                if (opStatus.Count > 0)
                    DisplayOperationStatus(opStatus);
            }
            txtStatus.Text += "\nEnding asyncsynchronous task creation.";
        }

        private void DisplayOperationStatus(List<OperationStatus> opStatusCol)
        {
            foreach (var operationStatus in opStatusCol)
            {
                txtStatus.Text += string.Format("\n{0}: {1}", operationStatus.Id, operationStatus.Status);
            }
        }

        private List<OperationStatus> GetOperationStatus(List<Guid> opIds)
        {
            List<OperationStatus> opStatus = null;
            try
            {
                 opStatus =  opIds.Select(t => scheduledTask.GetOperationStatus(t,true)).ToList();
            }
            catch (SchedulerException shedExp)
            {
              //TODO: Log exception
            }
            return opStatus;
        }

        private List<Guid> CreateTasks()
        {
            List<Guid> operationIds = null;
            Guid currentOperationId;
            var tasks = new TaskModel[5];
            
            tasks = GetTasks();
            operationIds = new List<Guid>();

            foreach (TaskModel t in tasks)
            {
                try
                {
                    operationIds.Add(scheduledTask.CreateTask(t));
                }
                catch (SchedulerModelValidationException me)
                {
                    txtStatus.Text += "\n" + me.Message;
                    //TODO: Log exception

                }
                catch (SchedulerException schedExp)
                {  
                    //TODO: Log exception
                }
            }
            return operationIds;
        }

        private async Task<List<Guid>> CreatTasksAsync()
        {
            List<Guid> operationIds;
            Guid currentOperationId;

            var tasks = new TaskModel[5];
            tasks = GetTasks();

            operationIds = new List<Guid>();
            foreach (TaskModel t in tasks)
            {
                try
                {
                    currentOperationId = await scheduledTask.CreateTaskAsync(t);
                    operationIds.Add(currentOperationId);

                }
                catch (SchedulerModelValidationException me)
                {
                    //write to log
                    txtStatus.Text += "\n" + me.Message;
                }
                catch (SchedulerException exp)
                {
                    //write to log
                }
            }
            
            return operationIds;
        }

        private async Task<List<OperationStatus>> GetOperationStatusAsync(List<Guid> opIds)
        {
            OperationStatus currentStatus;
            var opStatus = new List<OperationStatus>();

            foreach (Guid t in opIds)
            {
                try
                {
                    currentStatus = await scheduledTask.GetOperationStatusAsync(t, true);
                    opStatus.Add(currentStatus);
                }
                catch (SchedulerException exp)
                {
                    //write log
                }
            }
          
            return opStatus;
        }
        
        private static TaskModel[] GetTasks()
        {
            var tasks = new TaskModel[5];

            //valid
            tasks[0] = new TaskModel
            {
                Name = "ZZ first Nuget Task",
                JobType = JobType.Webhook,
                CronExpression = "0 0 12 1/1 * ? *", // run every 5 minutes; http://cronmaker.com/
                Params = new Dictionary<string, object>
                {
                    {"url", "http:/local/"}
                }
            };

            //valid
            tasks[1] = new TaskModel
            {
                Name = "ZZ second Nuget Task",
                JobType = JobType.Webhook,
                CronExpression = "0 0 12 1/1 * ? *", // run every 5 minutes; http://cronmaker.com/
                Params = new Dictionary<string, object>
                {
                    {"url", "http:/local/"}
                }
            };

            //invalid
            tasks[2] = new TaskModel
            {
                Name = "ZZ third Nuget Task",
                JobType = JobType.Webhook,
                CronExpression = "ABCD", // run every 5 minutes; http://cronmaker.com/
                Params = new Dictionary<string, object>
                {
                    {"url", "http:/local/"}
                },
                Start = DateTime.Now,
                End = DateTime.Now.Subtract(new TimeSpan(3, 0, 0))

            };

            //invalid
            tasks[3] = new TaskModel
            {
                Name = "",
                JobType = JobType.Webhook,
                CronExpression = "ABCD", // run every 5 minutes; http://cronmaker.com/
                Params = new Dictionary<string, object>
                {
                    {"url", "http:/local/"}
                }
            };

            //valid
            tasks[4] = new TaskModel
            {
                Name = "ZZ fifth Nuget Task",
                JobType = JobType.Webhook,
                CronExpression = "0 0 12 1/1 * ? *", // run every 5 minutes; http://cronmaker.com/
                Params = new Dictionary<string, object>
                {
                    {"url", "http:/local/"}
                }
            };

            return tasks;
        }
    }
}