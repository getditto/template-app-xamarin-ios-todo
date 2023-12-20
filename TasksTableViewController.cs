using System;
using Foundation;
using UIKit;
using DittoSDK;
using System.Collections.Generic;
using Newtonsoft.Json;
using Xamarin.Essentials;

namespace Tasks
{
	public partial class TasksTableViewController : UITableViewController
	{
		private TasksTableSource tasksTableSource = new TasksTableSource();

		private Ditto ditto
		{
			get
			{
				var appDelegate = (AppDelegate)UIApplication.SharedApplication.Delegate;
				return appDelegate.ditto;
			}
		}

		private DittoCollection collection
		{
			get
			{
				return ditto.Store.Collection(DittoTask.CollectionName);
			}
		}

		public TasksTableViewController(IntPtr handle) : base(handle)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			setupTaskList();
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);

			TableView.Source = tasksTableSource;
		}

		public void setupTaskList()
		{
			var query = $"SELECT * FROM {DittoTask.CollectionName} WHERE isDeleted = false";

			ditto.Sync.RegisterSubscription(query);
			ditto.Store.RegisterObserver(query, storeObservationHandler: async (queryResult) =>
			{
				var tasks = queryResult.Items.ConvertAll(d =>
				{
					return JsonConvert.DeserializeObject<DittoTask>(d.JsonString());
				});

				tasksTableSource.updateTasks(tasks);

				InvokeOnMainThread(() =>
				{
					TableView.ReloadData();
				});
			});
		}

		partial void didClickAddTask(UIBarButtonItem sender)
		{
			// Create an alert
			var alertControl = UIAlertController.Create(
				title: "Add New Task",
				message: null,
				preferredStyle: UIAlertControllerStyle.Alert);

			// Add a text field to the alert for the new task text
			alertControl.AddTextField(configurationHandler: (UITextField obj) => obj.Placeholder = "Enter Task");

			alertControl.AddAction(UIAlertAction.Create(title: "Cancel", style: UIAlertActionStyle.Cancel, handler: null));

			// Add a "OK" button to the alert.
			alertControl.AddAction(UIAlertAction.Create(title: "OK", style: UIAlertActionStyle.Default, alarm => addTask(alertControl.TextFields[0].Text)));

			// Present the alert to the user
			PresentViewController(alertControl, animated: true, null);
		}

		public void addTask(string text)
		{
			var dict = new Dictionary<string, object>
			{
				{"body", text},
				{"isCompleted", false},
				{ "isDeleted", false }
			};

            ditto.Store.ExecuteAsync($"INSERT INTO {DittoTask.CollectionName} DOCUMENTS (:doc1)", new Dictionary<string, object>()
            {
                { "doc1", dict }
            });
        }
	}
}
