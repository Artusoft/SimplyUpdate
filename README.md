<h1>Getting start</h1>

await upt.Configure("https://storage.blob.core.windows.net/public/v1.0/")
			.WhenUpdateAvailable(() => MessageBox.Show("Update available.\nUpdate now?", "SimplyUpdate test", MessageBoxButtons.YesNo) == DialogResult.Yes)
			.WhenUpdateCompleted(() => Application.Restart())
			.RunAsync();