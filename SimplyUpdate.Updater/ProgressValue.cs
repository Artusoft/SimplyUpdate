using System;
using System.Collections.Generic;
using System.Text;

namespace SimplyUpdate.Updater
{
	public enum UpdateStepEnum
	{
		Download,
		Unzip
	}

	public class ProgressValue
	{
		internal ProgressValue(UpdateStepEnum step,Int32 progress)
		{
			this.Progress = progress;
			this.Step = step;
		}

		public Int32 Progress { get; }
		public UpdateStepEnum Step { get; }
	}
}
