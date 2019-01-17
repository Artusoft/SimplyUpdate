using System;
using System.Collections.Generic;
using System.Text;

namespace SimplyUpdate.Updater
{
	public class UpdateAvailableInfo
	{
		internal UpdateAvailableInfo(Int32 localVersion, Int32 remoteVersion, Version localFileVersion, Version remoteFileVersion)
		{
			LocalVersion = localVersion;
			RemoteVersion = remoteVersion;
			LocalFileVersion = localFileVersion;
			RemoteFileVersion = remoteFileVersion;
		}

		public Int32 LocalVersion { get; }
		public Int32 RemoteVersion { get; }
		public Version LocalFileVersion { get; }
		public Version RemoteFileVersion { get; }

		public Boolean UpdateAvailable
		{
			get => RemoteVersion > LocalVersion || RemoteFileVersion > LocalFileVersion;
		}
	}
}
