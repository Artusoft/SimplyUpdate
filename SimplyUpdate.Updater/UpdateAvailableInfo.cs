using System;
using System.Collections.Generic;
using System.Text;

namespace SimplyUpdate.Updater
{
	public class UpdateAvailableInfo
	{
		internal UpdateAvailableInfo(Int32 localVersion, Int32 remoteVersion)
		{
			LocalVersion = localVersion;
			RemoteVersion = remoteVersion;
		}

		public Int32 LocalVersion { get; }
		public Int32 RemoteVersion { get; }

		public Boolean UpdateAvailable { get => RemoteVersion > LocalVersion; }
	}
}
