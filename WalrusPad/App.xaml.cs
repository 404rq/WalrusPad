using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Microsoft.Shell;

namespace WalrusPad
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
	public partial class App : Application, ISingleInstanceApp
    {
		private const string Unique = "406ea660-64cf-4c82-b6f0-42d48172a799";
		String appStartPath = System.IO.Path.GetDirectoryName(
			System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
		String configPath = Environment.GetFolderPath(
			Environment.SpecialFolder.MyDocuments) + "\\WalrusCorp\\walruspad\\~app-config";

		[STAThread]
		public static void Main() {
			if (SingleInstance<App>.InitializeAsFirstInstance(Unique)) {
				var application = new App();

				application.InitializeComponent();
				application.Run();

				// Allow single instance code to perform cleanup operations
				SingleInstance<App>.Cleanup();
			}
		}

		public bool SignalExternalCommandLineArgs(IList<string> args) {
			if (System.IO.File.Exists(appStartPath + "\\settings\\args.txt")) {
				System.IO.StreamReader r = new System.IO.StreamReader(configPath + "\\settings\\args.txt");
				string sLine = r.ReadLine();
				while (sLine != null) {
					args.Add(sLine);
					sLine = r.ReadLine();
				}
				r.Close();
			}
			System.IO.StreamWriter w = new System.IO.StreamWriter(configPath + "\\settings\\args.txt");
			for (int i = 0; i < args.Count; i++)
				w.WriteLine(args[i]);
			w.Close();
			return true;
		}
    }
}
