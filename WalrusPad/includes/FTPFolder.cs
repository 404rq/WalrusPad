using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalrusPad
{
	//
	// The FTPFolder class allows you to store as well as quickly
	// access a folder's name and path.
	//
	public class FTPFolder
	{
		private string folderPath;
		private string folderName;

		public FTPFolder(string folderPath, string folderName) {
			this.folderPath = folderPath;
			this.folderName = folderName;
		}

		public string GetName() {
			return folderName;
		}

		public string GetPath() {
			return folderPath;
		}
	}
}
