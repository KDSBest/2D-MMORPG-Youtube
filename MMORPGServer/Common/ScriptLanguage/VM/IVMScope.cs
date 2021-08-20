using System;
using System.Collections.Generic;
using System.Text;

namespace Common.ScriptLanguage.VM
{
	public interface IVMScope
	{
		string Scope { get; }

		VMVar Execute(string functionName, List<VMVar> parameters);
	}
}
