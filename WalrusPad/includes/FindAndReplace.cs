using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalrusPad.includes
{
	public class FindAndReplace
	{
		/// <summary>
		/// Perform a pregreplace
		/// </summary>
		/// <param name="te">The text editor control that will be replaced</param>
		/// <param name="find">What to search for</param>
		/// <param name="replace">What to replace</param>
		/// <returns>Number of replacements</returns>
		public int DoPregReplace(ICSharpCode.AvalonEdit.TextEditor te, string find, string replace) 
		{
			te.Text = te.Text.Replace(find, replace);
			return 0;
		}
	}
}
