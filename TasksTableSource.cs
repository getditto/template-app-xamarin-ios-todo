using System;
using System.Collections.Generic;
using Foundation;
using UIKit;
using DittoSDK;
using System.Collections;

namespace Tasks
{
    public class TasksTableSource : UITableViewSource
    {
        DittoTask[] tasks;
        NSString cellIdentifier = new NSString("taskCell");

        private Ditto ditto
        {
            get
            {
                var appDelegate = (AppDelegate)UIApplication.SharedApplication.Delegate;
                return appDelegate.ditto;
            }
        }

        public TasksTableSource(DittoTask[] taskList)
        {
            this.tasks = taskList;
        }

        public TasksTableSource()
        {
        }

        public void updateTasks(List<DittoTask> tasks)
        {
            this.tasks = tasks.ToArray();
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            UITableViewCell cell = tableView.DequeueReusableCell(cellIdentifier);
            if (cell == null)
            {
                cell = new UITableViewCell(UITableViewCellStyle.Default, cellIdentifier);
            }

            var task = tasks[indexPath.Row];

            cell.TextLabel.Text = task.Body;
            var taskComplete = task.IsCompleted;
            if (taskComplete)
            {
                cell.Accessory = UITableViewCellAccessory.Checkmark;
            }
            else
            {
                cell.Accessory = UITableViewCellAccessory.None;

            }

            var tapGesture = new UITapGestureRecognizer();
            tapGesture.AddTarget(() =>
            {
                var updateQuery = $"UPDATE {DittoTask.CollectionName} " +
                    $"SET isCompleted = {!task.IsCompleted} " +
                    $"WHERE _id = '{task.Id}' AND isCompleted != {!task.IsCompleted}";
                ditto.Store.ExecuteAsync(updateQuery);
            });
            cell.AddGestureRecognizer(tapGesture);

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            if (this.tasks == null)
            {
                return 0;
            }
            return tasks.Length;
        }

        public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
        {
            return true;
        }

        public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, Foundation.NSIndexPath indexPath)
        {
            switch (editingStyle)
            {
                case UITableViewCellEditingStyle.Delete:

                    var task = tasks[indexPath.Row];
                    var updateQuery = $"UPDATE {DittoTask.CollectionName} " +
                        "SET isDeleted = true " +
                        $"WHERE _id = '{task.Id}'";
                    ditto.Store.ExecuteAsync(updateQuery);
                    break;
                case UITableViewCellEditingStyle.None:
                    Console.WriteLine("CommitEditingStyle:None called");
                    break;
            }
        }

    }
}

