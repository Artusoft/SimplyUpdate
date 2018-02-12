using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimplyUpdate.WinAppTest
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			
			

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			using (var frm = new FormMain())
			{
				var upt = new SimplyUpdate.Updater.UpdaterClient();
				var cts = new System.Threading.CancellationTokenSource();
				frm.button1.Click += (_, __)=> cts.Cancel();
				frm.Load += async (_, __) => await upt.Configure("https://commessestorage.blob.core.windows.net/public/v1.0/")
					.WhenUpdateAvailable(() => MessageBox.Show("Aggiornamento disponibile. Aggiornare ora?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
					.WhenUpdateCompleted(() => Application.Restart())
					.WithUpdateProgress(new Progress<Updater.ProgressValue>(v => frm.progressBar1.Value = v.Progress))
					.WithCancellationToken(cts.Token)
					.RunAsync();

				Application.Run(frm);
			}
				
		}
	}
}
