using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SimplyUpdate.Updater
{
	public class UpdaterClient
	{
		private String RemoteRepository = String.Empty;
		private String LocalPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
		private Func<Boolean> WhenUpdateAvailableAction = () => true;
		private IProgress<ProgressValue> UpdateProgress = null;
		private Action WhenUpdateCompletedAction = null;
		private Action<Exception> WhenErrorAction = null;
		private CancellationToken cancellationToken = CancellationToken.None;

		public UpdaterClient Configure(String remoteRepository)
		{
			RemoteRepository = remoteRepository;
			return this;
		}

		public UpdaterClient WhenUpdateAvailable(Func<Boolean> action)
		{
			WhenUpdateAvailableAction = action;
			return this;
		}

		public UpdaterClient WithUpdateProgress(IProgress<ProgressValue> progress)
		{
			UpdateProgress = progress;
			return this;
		}

		public UpdaterClient WithCancellationToken(CancellationToken ct)
		{
			cancellationToken = ct;
			return this;
		}

		public UpdaterClient WhenUpdateCompleted(Action action)
		{
			WhenUpdateCompletedAction = action;
			return this;
		}
		public UpdaterClient WhenError(Action<Exception> action)
		{
			WhenErrorAction = action;
			return this;
		}


		public void Run() => RunAsync().Wait();

		public async Task RunAsync()
		{
			PurgeOldFile();

			if (await CheckUpdate() && WhenUpdateAvailableAction())
			{
				try
				{
					String zipFile = Path.GetTempFileName();
					var uriZip = new Uri(RemoteRepository + "software.zip");

					if (await DownloadFile(uriZip, zipFile))
					{
						UnzipFile(zipFile);
						var uriXml = new Uri(RemoteRepository + "software.xml");
						using (WebClient clt = new WebClient())
							clt.DownloadFile(uriXml, Path.Combine(LocalPath, "software.xml"));

						File.Delete(zipFile);
						WhenUpdateCompletedAction?.Invoke();
					}
				}
				catch(Exception ex)
				{
					WhenErrorAction?.Invoke(ex);
					// TODO:Send to logger 
				}
			}
		}

		private void PurgeOldFile()
		{
			foreach (var f in Directory.GetFiles(LocalPath, "*.SimplyUpdateOldFile", SearchOption.AllDirectories))
				try
				{
					File.Delete(f);
				}
				catch
				{
					// TODO:Send to logger 
				}
		}

		private Task<Boolean> CheckUpdate()
		=> Task.Factory.StartNew(() =>
			{
				Int32 localVersion = 0;
				Int32 remoteVersion = 0;

				try
				{
					Console.WriteLine("Controllo disponibilità aggiornamento ...");
					String uriXml = RemoteRepository + "software.xml";
					XDocument doc = XDocument.Load(uriXml);
					remoteVersion = Convert.ToInt32((from n in doc.Descendants("Version")
																					 select n.Value).FirstOrDefault());
				}
				catch { }

				String localXml = System.IO.Path.Combine(LocalPath, "software.xml");
				try
				{

					if (System.IO.File.Exists(localXml))
					{
						XDocument doc = XDocument.Load(localXml);
						localVersion = Convert.ToInt32((from n in doc.Descendants("Version")
																						select n.Value).FirstOrDefault());
					}
				}
				catch { }

				return remoteVersion > localVersion;
			});

		private void UnzipFile(string zipFile)
		{
			using (ZipArchive archive = ZipFile.OpenRead(zipFile))
				foreach (var entry in archive.Entries)
				{
					var localFile = Path.Combine(LocalPath, entry.FullName);
					if (File.Exists(localFile))
						RenameToOld(localFile);

					Directory.GetParent(localFile).Create();
					entry.ExtractToFile(localFile);
				}
		}

		private void RenameToOld(String sourceFileName)
		{
			var newFile = sourceFileName + ".SimplyUpdateOldFile";
			var i = 0;
			while (File.Exists(newFile))
				newFile = sourceFileName + $".{i}.SimplyUpdateOldFile";

			File.Move(sourceFileName, newFile);
		}

		private async Task<Boolean> DownloadFile(Uri address, String localPath)
		{
			try
			{
				using (WebClient clt = new WebClient())
				{
					clt.DownloadProgressChanged += (sender, e) =>
					{
						UpdateProgress?.Report(new ProgressValue( UpdateStepEnum.Download, e.ProgressPercentage));
						if (cancellationToken.IsCancellationRequested)
							clt.CancelAsync();
					};

					await clt.DownloadFileTaskAsync(address, localPath);

					return true;
				}
			}
			catch(Exception ex)
			{
				// TODO:Send to logger 
				WhenErrorAction?.Invoke(ex);
				return false;
			}
		}
	}
}
