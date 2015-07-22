using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit;
using AvalonDock.Controls;
using AvalonDock.Layout;
using System.Xml;
using System.IO;
using System.ComponentModel;
using System.Net;
using acorp;

namespace WalrusPad
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
	public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        //Globals
        AbstractFoldingStrategy foldingStrategy;
		System.Windows.Threading.DispatcherTimer dt;
		String appStartPath = System.IO.Path.GetDirectoryName(
			System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
		String webFilesPath = Environment.GetFolderPath(
			Environment.SpecialFolder.MyDocuments) + "\\WalrusCorp\\walruspad\\~web-files\\";
		String configPath = Environment.GetFolderPath(
			Environment.SpecialFolder.MyDocuments) + "\\WalrusCorp\\walruspad\\~app-config";
		List<string> recentFiles = new List<string>();
		String replaceCache = "";
		String encryptionKey = "";
		Int32 searchIndex;
		Boolean isConnectedToFTP = false;
		Boolean isSecureConnection = false;
		String dlgFileName = "";
		Int32 dlgTmpCounter = 0;

        //All commands in the menu
        #region Menu
		//Tools - Connect
		private void MenuItem_Click_23(object sender, RoutedEventArgs e) {
			if (!isConnectedToFTP) {
				ftp_browser_expander.IsExpanded = true;
				ftpBox.Focus();
			}
			else {
				DisconnectFTP();
			}
		}
        //View - File system browser
        private void MenuItem_Click_24(object sender, RoutedEventArgs e) {
			System.Diagnostics.Process.Start(appStartPath);
        }
		private void MenuItem_Click_25(object sender, RoutedEventArgs e) {
			System.Diagnostics.Process.Start(configPath + "\\connections\\");
		}
		private void MenuItem_Click_26(object sender, RoutedEventArgs e) {
			System.Diagnostics.Process.Start(appStartPath + "\\hlight\\");
		}
		private void MenuItem_Click_27(object sender, RoutedEventArgs e) {
			System.Diagnostics.Process.Start(webFilesPath);
		}
        //Edit - Cut, copy, paste, find, replace
		private void MenuItem_Click_5(object sender, RoutedEventArgs e) {
			try { activeDocument().Cut(); }
			catch { }
		}
		private void MenuItem_Click_6(object sender, RoutedEventArgs e) {
			try { activeDocument().Copy(); }
			catch { }
		}
		private void MenuItem_Click_7(object sender, RoutedEventArgs e) {
			try { activeDocument().Paste(); }
			catch { }
		}
		private void MenuItem_Click_8(object sender, RoutedEventArgs e) {
			if (grid_find.Visibility == System.Windows.Visibility.Visible)
				HideFindReplace();
			else
				ShowFind();
		}
		private void MenuItem_Click_9(object sender, RoutedEventArgs e) {
			if (grid_replace.Visibility == System.Windows.Visibility.Visible)
				HideFindReplace();
			else
				ShowReplace();
		}
		private void MenuItem_Click_12(object sender, RoutedEventArgs e) {
			try { activeDocument().Undo(); }
			catch { }
		}
		private void MenuItem_Click_13(object sender, RoutedEventArgs e) {
			try { activeDocument().Redo(); }
			catch { }
		}
		private void MenuItem_Click_14(object sender, RoutedEventArgs e) {
			IndentRight();
		}
		private void MenuItem_Click_15(object sender, RoutedEventArgs e) {
			IndentLeft();
		}
		private void MenuItem_Click_16(object sender, RoutedEventArgs e) {
			AddComment();
		}
		private void MenuItem_Click_17(object sender, RoutedEventArgs e) {
			RemoveComment();
		}
		private void MenuItem_Click_28(object sender, RoutedEventArgs e) {
			activeDocument().Text = activeDocument().Text.Replace("\n", "\r\n");
		}
		private void MenuItem_Click_29(object sender, RoutedEventArgs e) {
			activeDocument().Text = activeDocument().Text.Replace("\r\n", "\n");
		}
		private void MenuItem_Click_30(object sender, RoutedEventArgs e) {
			activeDocument().Text = activeDocument().Text.Replace(" \n", " ");
		}
        //File - Save and Load
        private void MenuItem_Click_10(object sender, RoutedEventArgs e)
        {
            Save();
        }
		private void MenuItem_Click(object sender, RoutedEventArgs e) {
			SaveAll();
		}
        private void MenuItem_Click_11(object sender, RoutedEventArgs e)
        {
            Load();
        }
        #endregion

        #region Toolbar
        private void btn_new_Click(object sender, RoutedEventArgs e)
        {
            AddDocument("");
        }
        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }
		private void btn_save_all_Click(object sender, RoutedEventArgs e) {
			SaveAll();
		}
        private void btn_load_Click(object sender, RoutedEventArgs e)
        {
            Load();
        }
		private void btn_publish_Click(object sender, RoutedEventArgs e) {
			Publish();
		}
		private void btn_publish_all_Click(object sender, RoutedEventArgs e) {
			PublishAll();
		}
        private void btn_cut_Click(object sender, RoutedEventArgs e)
        {
            try
            { activeDocument().Cut(); }
            catch 
            { }
        }
        private void btn_copy_Click(object sender, RoutedEventArgs e)
        {
            try
            { activeDocument().Copy(); }
            catch
            { }
        }
        private void btn_paste_Click(object sender, RoutedEventArgs e)
        {
            try
            { activeDocument().Paste(); }
            catch
            { }
        }
        private void btn_undo_Click(object sender, RoutedEventArgs e)
        {
            try
            { activeDocument().Undo(); }
            catch 
            { }
        }
        private void btn_redo_Click(object sender, RoutedEventArgs e)
        {
            try
            { activeDocument().Redo(); }
            catch
            { }
        }
		private void btn_zoom_in_Click(object sender, RoutedEventArgs e) {
			Zoom(activeDocument(), 1);
		}
		private void btn_zoom_out_Click(object sender, RoutedEventArgs e) {
			Zoom(activeDocument(), -1);
		}
		private void btn_find_Click(object sender, RoutedEventArgs e) {
			if (grid_find.Visibility == System.Windows.Visibility.Visible)
				HideFindReplace();
			else
				ShowFind();
		}
		private void btn_replace_Click(object sender, RoutedEventArgs e) {
			if (grid_replace.Visibility == System.Windows.Visibility.Visible)
				HideFindReplace();
			else
				ShowReplace();
		}
		private void btn_comment_Click(object sender, RoutedEventArgs e) {
			AddComment();
		}
		private void btn_uncomment_Click(object sender, RoutedEventArgs e) {
			RemoveComment();
		}
		private void btn_folding_open_Click(object sender, RoutedEventArgs e) {
			OpenFolding();
		}
		private void btn_folding_close_Click(object sender, RoutedEventArgs e) {
			CloseFolding();
		}
		private void btn_custom_comment_Click(object sender, RoutedEventArgs e) {
			//Allow the user to choose type of comment by himself
			(sender as Button).ContextMenu.IsEnabled = true;
			(sender as Button).ContextMenu.PlacementTarget = (sender as Button);
			(sender as Button).ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
			(sender as Button).ContextMenu.IsOpen = true;
		}
		private void MenuItem_Click_18(object sender, RoutedEventArgs e) {
			AddComment("/*", "*/");
		}
		private void MenuItem_Click_19(object sender, RoutedEventArgs e) {
			AddComment("<!--", "-->");
		}
		private void MenuItem_Click_20(object sender, RoutedEventArgs e) {
			AddComment("--[[", "]]--");
		}
		private void MenuItem_Click_21(object sender, RoutedEventArgs e) {
			AddComment("//", "");
		}
		private void MenuItem_Click_22(object sender, RoutedEventArgs e) {
			AddComment("--", "");
		}
		private void btn_indentL_Click(object sender, RoutedEventArgs e) {
			IndentLeft();
		}
		private void btn_indentR_Click(object sender, RoutedEventArgs e) {
			IndentRight();
		}
        #endregion

		//Load and close
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
			//Set up non existing folders
			if (!Directory.Exists(configPath))
				Directory.CreateDirectory(configPath);
			if (!Directory.Exists(webFilesPath))
				Directory.CreateDirectory(webFilesPath);
			if (!Directory.Exists(configPath + "\\connections"))
				Directory.CreateDirectory(configPath + "\\connections");
			if (!Directory.Exists(configPath + "\\settings"))
				Directory.CreateDirectory(configPath + "\\settings");

            //Set default theme
            acorp.WindowSSL ssl = new acorp.WindowSSL();
			ssl.LoadWindowData(this, configPath + "\\settings\\main-win.txt");
			dockingManager.Theme = new AvalonDock.Themes.MetroTheme();

			//Load menu
			try {
				string[] xshdFiles = Directory.GetFiles(appStartPath + "\\hlight\\");
				for (int i = 0; i < xshdFiles.Length - 1; i++) {
					MenuItem item = new MenuItem();
					string fileType = xshdFiles[i].Split('\\')[(xshdFiles[i].Split(
						'\\').Length - 1)].Remove(xshdFiles[i].Split(
						'\\')[(xshdFiles[i].Split('\\').Length - 1)].Length - 5, 5);
					item.Header = fileType;
					item.Click += new RoutedEventHandler(
					   delegate(object s, RoutedEventArgs earg) {
						   AddDocument(fileType);
					   });
					newItem.Items.Add(item);
				}

				//Create directory if it doesn't exist
				if (!Directory.Exists(configPath + "\\connections"))
					Directory.CreateDirectory(configPath + "\\connections");

				//Create directory if it doesn't exist
				if (!Directory.Exists(webFilesPath))
					Directory.CreateDirectory(webFilesPath);

				//Load the encryption key from local file
				if (File.Exists(configPath + "\\key.n2c")) {
					StreamReader encR = new StreamReader(configPath + "\\key.n2c");
					encryptionKey = encR.ReadLine();
					encR.Close();
				}
				else {
					StreamWriter encW = new StreamWriter(configPath + "\\key.n2c");
					Random rand = new Random();
					char[] randomLetters = new char[rand.Next(16,32)];
					
					for (int i = 0; i < randomLetters.Length; i++)
						randomLetters[i] = Convert.ToChar(rand.Next(1, 255));
					string tmpKey = new string(randomLetters);
					encW.WriteLine(tmpKey);
					encryptionKey = tmpKey;
					encW.Close();
				}
				
				//Get connection files
				string[] connections = Directory.GetFiles(configPath + "\\connections\\");
				for (int i = 0; i < connections.Length; i++) {
					if (connections[i].EndsWith(".n2c")) {
						string fPath = connections[i];
						MenuItem item = new MenuItem();
						item.Header = System.IO.Path.GetFileName(connections[i]).Replace(".n2c", "");
						item.Click += new RoutedEventHandler(
						   delegate(object s, RoutedEventArgs earg) {
							   cbx_conn.Text = System.IO.Path.GetFileName(fPath).Replace(".n2c", "");
							   loadN2CFile();
						   });
						menuFile.Items.Add(item);
					}
				}
			}
			catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); }

			//Check and load arguments from command line
			string[] args = Environment.GetCommandLineArgs();
			if (args.Length > 0) {
				Load(args.ToArray());
			}
			/*if (args.Length < 2) {
				//Adds an empty document
				AddDocument("txt");
			}*/
        }
		private void dockingManager_Loaded(object sender, RoutedEventArgs e) 
		{
			//Initialize a timer to check for aruments
			dt = new System.Windows.Threading.DispatcherTimer();
			dt.Tick +=dt_Tick;
			dt.Interval = new TimeSpan(0,0,0,0,100);
			dt.Start();
		}
		void dt_Tick(object sender, EventArgs e) 
		{
			// Check if the argument file exist
			if (File.Exists(configPath + "\\settings\\args.txt")) {
				List<string> args = new List<string>();
				StreamReader r = new StreamReader(configPath + "\\settings\\args.txt");
				string sLine = r.ReadLine();
				while (sLine != null) {
					args.Add(sLine);
					sLine = r.ReadLine();
				}
				r.Close();
				try {
					//Load arguments
					if (args.Count > 0) {
						Load(args.ToArray());
					}

					//Removes the argument file
					File.Delete(configPath + "\\settings\\args.txt");
				}
				catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
			}
		}
        private void Window_Closed(object sender, EventArgs e)
        {
			//Clear the cache
			ClearCache(webFilesPath);

			// Save data
            acorp.WindowSSL ssl = new acorp.WindowSSL();
			ssl.SaveWindowData(this, configPath + "\\settings\\main-win.txt");
        }
		private void Window_Closing(object sender, CancelEventArgs e) {
			//Check for unsaved files
			foreach (LayoutDocument dc in documentPanel.Children) {
				if (dc.Title.EndsWith("*")) {
					MessageBoxResult r = MessageBox.Show("There are unsaved files opened, do you want to save them before exit?",
						"Unsaved files found", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
					if (r == MessageBoxResult.Cancel) { e.Cancel = true; }
					else if (r == MessageBoxResult.Yes) { SaveAll(); }
				}
			}
		}

        //Window key down event
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) > 0) {
                //Default key combinations
                if ((Keyboard.GetKeyStates(Key.N) & KeyStates.Down) > 0)
                    AddDocument("");
                if ((Keyboard.GetKeyStates(Key.O) & KeyStates.Down) > 0)
                    Load();
                if ((Keyboard.GetKeyStates(Key.S) & KeyStates.Down) > 0)
                    Save();
                if ((Keyboard.GetKeyStates(Key.LeftShift) & KeyStates.Down) > 0) {
                    if ((Keyboard.GetKeyStates(Key.S) & KeyStates.Down) > 0) {
						SaveAll();
                    }
                }
				if ((Keyboard.GetKeyStates(Key.U) & KeyStates.Down) > 0)
					Publish();
				if ((Keyboard.GetKeyStates(Key.LeftShift) & KeyStates.Down) > 0) {
					if ((Keyboard.GetKeyStates(Key.U) & KeyStates.Down) > 0) {
						PublishAll();
					}
				}
				if ((Keyboard.GetKeyStates(Key.F) & KeyStates.Down) > 0) {
					if (grid_find.Visibility == System.Windows.Visibility.Visible)
						HideFindReplace();
					else
						ShowFind();
				}
				if ((Keyboard.GetKeyStates(Key.R) & KeyStates.Down) > 0) {
					if (grid_replace.Visibility == System.Windows.Visibility.Visible)
						HideFindReplace();
					else
						ShowReplace();
				}
            }
			if ((Keyboard.GetKeyStates(Key.Enter) & KeyStates.Down) > 0) {
				if (grid_find.Visibility == System.Windows.Visibility.Visible)
					FindInDocument();
				else if (passwordBox.IsKeyboardFocused || userNameBox.IsKeyboardFocused || ftpBox.IsFocused) {
					//Collapse and connect to FTP server
					ftp_browser_expander.IsExpanded = false;
					SetupFTP();
				}
			}
			if ((Keyboard.GetKeyStates(Key.Delete) & KeyStates.Down) > 0) {
				if (fileList.SelectedIndex > -1) {
					MessageBoxResult r = MessageBox.Show("Are you sure you wish to delete these files?", 
						"Confirm delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
					//Delete selected files
					if (r == MessageBoxResult.Yes) {
						FTPLibrary lib = new FTPLibrary(userNameBox.Text, passwordBox.Password, isSecureConnection);
						string res = lib.DeleteFTPDirectory(ftpBox.Text + "/" + fileList.SelectedItem.ToString());
						MessageBox.Show(res);
					}
				}
			}
        }

        //All functions
        #region Functions
        //File system
        /// <summary>
        /// Save a file on the local computer or on the web
        /// </summary>
        private string Save(bool isPublish = false)
        {    
            try
            {
                //Save the file
				string[] fileNames = activeFileName().Text.Split('\t');
				for (int i = 0; i < fileNames.Length; i++) {
					if (fileNames[i] != "") {
						string dir = System.IO.Path.GetDirectoryName(fileNames[i]);
						if (!Directory.Exists(dir)) { Directory.CreateDirectory(dir); }

						//Status update
						ShowStatus("Saving file: (" + System.IO.Path.GetFileName(
							fileNames[i]) + ")");

						//Mark as saved
						if (activeLayout().Title.EndsWith(" *"))
							activeLayout().Title = activeLayout().Title.Replace(" *", "");

						//Prepare and start process
						String[] tmp = new String[4];
						tmp[0] = fileNames[i];
						tmp[1] = activeDocument().Text;
						if (isPublish && fileNames[i].StartsWith(webFilesPath)) {
							//Get FTP path
							tmp[2] = "true";
							tmp[3] = fileNames[i].Substring(webFilesPath.Length);
							tmp[3] = tmp[3].Replace('\\', '/');
						}
						else {
							tmp[2] = "false";
							tmp[3] = "";
						}

						//Set name of file
						if (activeLayout().Title == "New page" || activeFileName().Text == "") {
							activeLayout().Title = activeLayout(
								).Title = System.IO.Path.GetFileName(fileNames[i]);
							activeFileName().Text = fileNames[i];
						}
						
						//Define background worker events
						BackgroundWorker worker = new BackgroundWorker();
						worker.DoWork += worker_DoWork;
						worker.RunWorkerCompleted += worker_RunWorkerCompleted;
						worker.WorkerReportsProgress = true;
						worker.RunWorkerAsync(tmp);
					}
					else {
						int tmpLen = activeDocument().Text.Length;
						if (dlgFileName == "") {
							Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
							dlg.DefaultExt = ".txt";
							dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
							dlg.AddExtension = true;
							dlg.OverwritePrompt = true;
							dlg.Filter = "Text File (*.txt)|*.txt|C# File (*.cs)|*.cs|C++ File (*.cpp)|*.cpp" +
								"|C File (*.c)|*.c|PHP File (*.php)|*.php|JavaScript File (*.js)|*.js|HTML File " +
								"(*.html, *.htm)|*.html;*.htm|CSS File (*.css, *.less)|*.css;*.less|All Files (*.*)|*.*";
							dlg.Title = "Save document";
							dlg.ShowDialog();
							fileNames[i] = dlg.FileName;
							dlgFileName = dlg.FileName;
						}
						else {
							dlgTmpCounter++;
						}

						//Increase file index
						if (dlgTmpCounter > 0) {
							fileNames[i] = fileNames[i] + "(" + dlgTmpCounter + ")";
						}

						string dir = System.IO.Path.GetDirectoryName(fileNames[i]);
						if (!Directory.Exists(dir)) { Directory.CreateDirectory(dir); }

						//Status update
						ShowStatus("Saving file: (" + System.IO.Path.GetFileName(
							fileNames[i]) + ")");

						//Prepare and start process
						String[] tmp = new String[4];
						tmp[0] = fileNames[i];
						tmp[1] = activeDocument().Text;
						if (isPublish && fileNames[i].StartsWith(webFilesPath)) {
							//Get FTP path
							tmp[2] = "true";
							tmp[3] = fileNames[i].Substring(webFilesPath.Length);
							tmp[3] = tmp[3].Replace('\\', '/');
						}
						else {
							tmp[2] = "false";
							tmp[3] = "";
						}

						//Correct details
						if (tmpLen != activeDocument().Text.Length)
							activeDocument().Text.Remove((tmpLen-1),1);

						//Set name of file
						activeLayout().Title = activeLayout(
							).Title = System.IO.Path.GetFileName(fileNames[i]);
						activeFileName().Text = fileNames[i];

						//Define background worker events
						BackgroundWorker worker = new BackgroundWorker();
						worker.DoWork += worker_DoWork;
						worker.RunWorkerCompleted += worker_RunWorkerCompleted;
						worker.WorkerReportsProgress = true;
						worker.RunWorkerAsync(tmp);

						//Mark as saved
						if (activeLayout().Title.EndsWith(" *"))
							activeLayout().Title = activeLayout().Title.Replace(" *", "");
					}
				}
				return activeFileName().Text;
            }
			catch 
			{ 
				ShowStatus("Invalid file name, please try again");
				return "";
			}
        }
		private string[] SaveAll(bool isPublish = false) {
			List<string> filesToSave = new List<string>();
			int tmp = documentPanel.SelectedContentIndex;
			foreach (LayoutDocument dc in documentPanel.Children) {
				dc.IsActive = true;
				if (isPublish)
					filesToSave.Add(Save(true));
				else
					filesToSave.Add(Save(false));
			}
			documentPanel.SelectedContentIndex = tmp;

			//Status update
			ShowStatus("Document saved: (All documents)");
			dlgFileName = "";
			dlgTmpCounter = 0;

			//Return list
			return filesToSave.ToArray();
		}
		//Save files in the background
		private void worker_DoWork(object sender, DoWorkEventArgs e) {
			//Get arguments
			string[] tmp = (e.Argument as string[]);

			//Attempt to save file
			try {
				//Save the file
				StreamWriter w = new StreamWriter(tmp[0]);
				w.Write(tmp[1]);
				w.Close();
			}
			catch { //Set result status code on failure
				e.Result = "Error! Unable to save file: " + System.IO.Path.GetFileName(tmp[0]);
			}
			try {
				//Save the file
				if (tmp[2] == "true") {
					//Attempts to upload file
					string check = "";
					this.Dispatcher.Invoke((Action)(() =>
					{
						FTPLibrary ftpLib = new FTPLibrary(userNameBox.Text, passwordBox.Password, isSecureConnection);
						check = ftpLib.Upload("ftp://" + tmp[3], tmp[0]);
					}));

					//Set result status code
					if (!check.StartsWith("Error"))
						e.Result = "Document published: (" + System.IO.Path.GetFileName(tmp[0]) + ")";
					else {
						e.Result = "Failed to publish document! Reason: (" + check.Substring(7) + ")";
					}
				}
				else {
					//Set result status code
					e.Result = "Document saved: (" + System.IO.Path.GetFileName(tmp[0]) + ")";
				}
			}
			catch (Exception ex){ //Set result status code on failure
				e.Result = "Error! Unable to publish file: " + System.IO.Path.GetFileName(tmp[0]) + ", Reason: (" + ex.Message + ")";
			}
		}
		//Report status
		private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
			//Status update
			ShowStatus(e.Result.ToString());
		}
        /// <summary>
        /// Load a file from local computer or web
        /// </summary>
        private void Load(string[] fileNames = null)
        {
			try {
				//Open a dialog and let the user select a file to load
				if (fileNames == null) {
					Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
					dlg.DefaultExt = ".txt";
					dlg.Multiselect = true;
					dlg.Filter = "Text File (*.txt)|*.txt|C# File (*.cs)|*.cs|C++ File (*.cpp)|*.cpp" +
						"|C File (*.c)|*.c|PHP File (*.php)|*.php|JavaScript File (*.js)|*.js|HTML File " +
						"(*.html, *.htm)|*.html;*.htm|CSS File (*.css, *.less)|*.css;*.less|All Files (*.*)|*.*";
					dlg.Title = "Open document(s)";
					dlg.ShowDialog();
					fileNames = dlg.FileNames;
				}

				//Load the file
				if (fileNames.Length > 0) {
					string status = "Document(s) loaded: ( ";
					for (int i = 0; i < fileNames.Length; i++) {
						if (!fileNames[i].Contains(".exe")) {
							//Check if file is opened and select it if so
							bool isOpen = false;
							foreach (LayoutDocument dc in documentPanel.Children) {
								string[] tmpFileList = ((dc.Content as Grid).Children[1] as TextBox).Text.Split('\t');
								if (fileNames[i] == tmpFileList[0]) {
									dc.IsActive = true;
									isOpen = true;
									break;
								}
							}
							//Only load the file if it's not already loaded
							if (!isOpen) {
								//Add new document
								string hLightExt = System.IO.Path.GetExtension(fileNames[i]).Remove(0, 1);
								AddDocument(hLightExt, System.IO.Path.GetFileName(fileNames[i]));
								activeFileName().Text = fileNames[i];

								//Prepare for backup
								string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
									"\\WalrusCorp\\walruspad\\";
								if (!Directory.Exists(docs)) { Directory.CreateDirectory(docs); }

								//Save a backup of the file is loaded from a network path
								if (fileNames[i].StartsWith("\\\\"))
									activeFileName().Text += "\t" + docs + fileNames[i].Remove(0, 2);

								//Load the file
								StreamReader r = new StreamReader(fileNames[i]);
								activeDocument().Text = r.ReadToEnd();
								r.Close();

								//Deal with conflicts between local files and global files
								if (File.Exists(fileNames[i] + "~")) {
									//Load the backup
									StreamReader rb = new StreamReader(fileNames[i] + "~");
									string tmp = rb.ReadToEnd();
									rb.Close();

									// Add document if different
									if (tmp != activeDocument().Text) {
										TextEditor rtb = AddDocument(hLightExt, System.IO.Path.GetFileName(fileNames[i] + 
											" (Local)"), true, activeDocument());
										rtb.Text = tmp;
									}
								}
								status += System.IO.Path.GetFileName(fileNames[i]) + " ";

								//if (!FileAssociation.IsAssociated(System.IO.Path.GetExtension(
									//	fileNames[i]))) {
									if (File.Exists(appStartPath + "\\icons\\" + System.IO.Path.GetExtension(
										fileNames[i]).Remove(0, 1) + ".ico")) {
										FileAssociation.Associate(System.IO.Path.GetExtension(fileNames[i]),
											"acorp.Notepad_II", "WalrusPad document", appStartPath + "\\icons\\" +
											System.IO.Path.GetExtension(fileNames[i]).Remove(0, 1) + ".ico", "WalrusPad.exe");
									}
								//}

								//Mark as saved
								if (activeLayout().Title.EndsWith(" *"))
									activeLayout().Title = activeLayout().Title.Replace(" *", "");
							}
						}
					}

					//Activate window
					this.Activate();

					//Update status
					status += ")";
					ShowStatus(status);
				}
			}
			catch { }
        }
        //Add document
        private TextEditor AddDocument(string hLight, string title = null, bool showInRight = false, TextEditor syncBox = null)
        {
            //Define objects
			FoldingManager foldingManager;
            LayoutDocument dc = new LayoutDocument();
			Grid gd = new Grid();
            TextEditor rtb = new TextEditor();
			TextBox tb = new TextBox();

			//Installs the folding manager
			foldingManager = FoldingManager.Install(rtb.TextArea);

            //Get title
            if (title == null)
                title = "New page";

            //Check for multiple titles
            int counter = 0;
            foreach (LayoutDocument ld in documentPanel.Children) {
                if (ld.Title.Contains(title))
                    counter++;
            }

            //Object properties
            if (counter > 0)
                dc.Title = title + " (" + counter + ")";
            else
                dc.Title = title;
			tb.Margin = new Thickness(0,0,0,0);
			tb.VerticalAlignment = System.Windows.VerticalAlignment.Top;
			tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
			tb.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
			tb.BorderBrush = new SolidColorBrush(Color.FromRgb(226,226,226));
			tb.Foreground = new SolidColorBrush(Color.FromRgb(91, 91, 91));
			tb.SelectionBrush = new SolidColorBrush(Color.FromArgb(90, 0, 0, 0));
			tb.Height = 23;
			
			//Text editor area
            rtb.AllowDrop = true;
            rtb.Tag = foldingManager;
            rtb.Margin = new Thickness(0, 23, 0, 0);
            rtb.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            rtb.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            rtb.FontFamily = new System.Windows.Media.FontFamily("Courier New");
            rtb.FontSize = Convert.ToDouble(12);
            rtb.Padding = new Thickness(11);
            rtb.ShowLineNumbers = true;
			rtb.LineNumbersForeground = new SolidColorBrush(Color.FromRgb(140, 140, 140));
            rtb.TextArea.Options.EnableEmailHyperlinks = true;
            rtb.TextArea.Options.EnableHyperlinks = true;
            rtb.TextArea.Options.EnableTextDragDrop = true;
            rtb.TextArea.SelectionCornerRadius = 0;
            rtb.TextArea.SelectionBrush = new SolidColorBrush(Color.FromArgb(190, 0, 0, 0));
            rtb.TextArea.SelectionBorder = new Pen(new SolidColorBrush(Color.FromArgb(30, 0, 0, 0)), 1);
			//rtb.TextArea.TextView.LineTransformers.Add(new ColorizeAvalonEdit());
			rtb.Options.EnableRectangularSelection = true;
			rtb.Options.AllowScrollBelowDocument = true;

            //Rich text change
            rtb.TextChanged += new EventHandler(
				delegate(object sender, EventArgs e)
				{
					//Update folding layout
					try
					{
						//Set status to not saved
						if (!activeLayout().Title.EndsWith(" *"))
							activeLayout().Title += " *";

						//Update stats
						documentStats();
						foldingStrategy.UpdateFoldings((activeDocument().Tag as FoldingManager), activeDocument().Document);
					}
					catch { }
				});
			rtb.TextArea.Caret.PositionChanged += new EventHandler(
				delegate(object sender, EventArgs e) {
					try { 
						documentStats();
					}
					catch { }
				});
			if (showInRight && syncBox != null) {
				//rtb.TextArea.TextView.ScrollOffsetChanged += new EventHandler(
				//	delegate(object sender, EventArgs e) {
				//		if (IsMouseOver)
				//			syncBox.ScrollToVerticalOffset(rtb.VerticalOffset);
						
				//});
				syncBox.TextArea.Caret.PositionChanged += new EventHandler(
					delegate(object sender, EventArgs e) {
						rtb.ScrollToVerticalOffset(syncBox.VerticalOffset);
				});
				syncBox.TextArea.TextView.ScrollOffsetChanged += new EventHandler(
					delegate(object sender, EventArgs e) {
						if (IsMouseOver)
							rtb.ScrollToVerticalOffset(syncBox.VerticalOffset);
				});
			}
            dc.IsSelectedChanged += new EventHandler(
                delegate(object sender, EventArgs e)
                {
                    //Update folding layout
                    try
                    {
						documentStats();
                        ApplyFoldings(activeDocument());
						//foldingStrategy.UpdateFoldings((activeDocument().Tag as FoldingManager), activeDocument().Document);
                    }
                    catch { }
                });

            //Add document
            dc.Content = gd;
			if (showInRight) {
				documentPanelRight.Children.Add(dc);
				rtb.IsReadOnly = true;
				ApplyFoldings(rtb);
			}
			else
				documentPanel.Children.Add(dc);
			dc.IsSelected = true;
            dc.IsActive = true;

			//Add children structure
			gd.Children.Add(rtb);
			gd.Children.Add(tb);

            //Apply default highlight
			ApplyHighlight(rtb, hLight);

			//Return the box
			return rtb;
        }
        //Apply the highlight features
        private bool ApplyHighlight(TextEditor editor, string ext = "txt")
        {
            //Adds .xshd as in highlight files
            ext += ".xshd";
            ext = appStartPath + "\\hlight\\" + ext;
            if (File.Exists(ext))
            {
                StreamReader s = new StreamReader(ext);
                using (XmlTextReader reader = new XmlTextReader(s))
                {
					editor.SyntaxHighlighting =
                        ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(
                        reader, HighlightingManager.Instance);
                }
            }
			ApplyFoldings(editor);
            return true;
        }
        private void ApplyFoldings(TextEditor editor)
        {
			try {
				if (editor.SyntaxHighlighting == null)
					foldingStrategy = null;
				else {
					switch (editor.SyntaxHighlighting.Name) {
						case "XML":
							foldingStrategy = new XmlFoldingStrategy();
							editor.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.DefaultIndentationStrategy();
							break;
						case "C#":
						case "C++":
						case "PHP":
						case "Java":
							foldingStrategy = new acorp.BraceFoldingStrategy();
							editor.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.CSharp.CSharpIndentationStrategy(editor.Options);
							break;
						default:
							foldingStrategy = new acorp.BraceFoldingStrategy();
							editor.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.DefaultIndentationStrategy();
							break;
					}
				}
				if (foldingStrategy != null) {
					foldingStrategy.UpdateFoldings((editor.Tag as FoldingManager), editor.Document);
				}
			}
			catch { }
        }
        //Get active texteditor
        TextEditor activeDocument()
        {
            foreach (LayoutDocument ld in documentPanel.Children)
            {
                if (ld.IsSelected)
                    return ((ld.Content as Grid).Children[0] as TextEditor);
            }
            return null;
        }
		//Get active fileName
		TextBox activeFileName() {
			foreach (LayoutDocument ld in documentPanel.Children) {
				if (ld.IsSelected)
					return ((ld.Content as Grid).Children[1] as TextBox);
			}
			return null;
		}
        //Get active texteditor
        LayoutDocument activeLayout()
        {
            foreach (LayoutDocument ld in documentPanel.Children)
            {
                if (ld.IsSelected)
                    return ld;
            }
            return null;
        }
		//Zoom function
		private void Zoom(TextEditor te, double scale, bool setDefault = false) 
		{
			try {
				if (setDefault)
					te.TextArea.FontSize = 12;
				else if (te.TextArea.FontSize > 1)
					te.TextArea.FontSize = activeDocument().TextArea.FontSize + scale;
				else
					te.TextArea.FontSize = 1;
				ShowStatus("Current zoom level: " + te.TextArea.FontSize.ToString());
			}
			catch {  }
		}
		//Add comment
		private void AddComment(string start = null, string end = null) {
			try {
				string commentStart = "/*";
				string commentEnd = "*/";
				if (start == null && end == null) {
					string ext = System.IO.Path.GetExtension(activeFileName().Text.Split('\t')[0]);
					if (ext.Equals(".lua") || ext.Equals(".slua") || ext.Equals(".luac")) {
						commentStart = "--[[";
						commentEnd = "]]--";
					}
					else if (ext.Equals(".xml") || ext.Equals(".xaml") || ext.Equals(".html") ||
						ext.Equals(".htm") || ext.Equals(".conf") || ext.Equals(".xhtml")) {
						commentStart = "<!--";
						commentEnd = "-->";
					}
				}
				//Take from arguments
				else {
					commentStart = start;
					commentEnd = end;
				}
				if (!activeDocument().SelectedText.StartsWith(commentStart))
					activeDocument().SelectedText = commentStart + activeDocument().SelectedText + commentEnd;
			}
			catch { }
		}
		//Remove comment
		private void RemoveComment() {
			try {
				string ext = System.IO.Path.GetExtension(activeFileName().Text.Split('\t')[0]);
				string commentStart = "/*";
				string commentEnd = "*/";
				if (ext.Equals(".lua") || ext.Equals(".slua") || ext.Equals(".luac")) {
					commentStart = "--[[";
					commentEnd = "]]--";
				}
				else if (ext.Equals(".xml") || ext.Equals(".xaml") || ext.Equals(".html") ||
					ext.Equals(".htm") || ext.Equals(".conf") || ext.Equals(".xhtml")) {
					commentStart = "<!--";
					commentEnd = "-->";
				}
				int start = activeDocument().Text.IndexOf(commentStart,activeDocument().SelectionStart,5);
				if (start.Equals(-1))
					start = activeDocument().Text.IndexOf(commentStart,activeDocument().TextArea.Caret.Offset,5);
				int end = activeDocument().Text.IndexOf(commentEnd,start) + commentEnd.Length;
				if (start >= 0 && end > 0) {
					activeDocument().Select(start, (end - start));
					activeDocument().SelectedText = activeDocument().SelectedText.Replace(
						activeDocument().SelectedText, 
						activeDocument().SelectedText.Substring(
						commentStart.Length, activeDocument().SelectedText.Length - (commentStart.Length + commentEnd.Length)));
				}
			}
			catch { }
		}
		//Open all foldings in current document
		private void OpenFolding() {
			foreach (FoldingSection fm in (activeDocument().Tag as FoldingManager).AllFoldings) {
				fm.IsFolded = false;
			}
		}
		//Close all foldings in current document
		private void CloseFolding() {
			foreach (FoldingSection fm in (activeDocument().Tag as FoldingManager).AllFoldings) {
				fm.IsFolded = true;
			}
		}
		//Clear cache
		private void ClearCache(string path) {
			string[] files = Directory.GetFiles(path);
			for (int i = 0; i < files.Length; i++) {
				if (files[i].EndsWith("~"))
					File.Delete(files[i]);
			}
			string[] dirs = Directory.GetDirectories(path);
			for (int i = 0; i < dirs.Length; i++)
				ClearCache(dirs[i]);
		}
		//Highlight symbols start and end
		private void highlightStartEnd(TextEditor rtb) {
			int offset = rtb.CaretOffset;
			if (offset < rtb.Text.Length) {
				char[] chStart = { '(', '[', '{' };
				char[] chEnd = { ')', ']', '}' };
				for (int i = 0; i < chStart.Length; i++) {
					if (rtb.Text.Substring(offset, 1) == chStart[i].ToString()) {
						rtb.Select(offset, 1);
						
						offset = rtb.Text.IndexOf(chEnd[i], offset);
						rtb.Select(offset, 1);
						rtb.TextArea.SelectionBrush = new SolidColorBrush(Color.FromRgb(100, 0, 100));
						break;
					}
				}
			}
		}
        #endregion

		#region Misc
		private void ShowStatus(string statusMessage) 
		{
			//Update status
			lbl_info.Opacity = 0;
			lbl_info.Content = statusMessage;
			lbl_purple.Content = statusMessage;

			//Save to output
			output.AppendText("[" + DateTime.Now.ToString() + "] " + statusMessage + "\n");
			output.ScrollToEnd();
			
			//Animate
			System.Windows.Media.Animation.DoubleAnimation db;
			db = new System.Windows.Media.Animation.DoubleAnimation();
			db.From = 0;
			db.To = 1;
			db.AccelerationRatio = 0.9;
			db.Duration = new Duration(new TimeSpan(0, 0, 5));
			lbl_info.BeginAnimation(Label.OpacityProperty, db);
		}
		private void ShowReplace() 
		{
			grid_replace.Visibility = System.Windows.Visibility.Visible;
			grid_find.Visibility = System.Windows.Visibility.Collapsed;
			txt_find.Focus();
		}
		private void ShowFind() 
		{
			grid_replace.Visibility = System.Windows.Visibility.Collapsed;
			grid_find.Visibility = System.Windows.Visibility.Visible;
			txt_search.Focus();
		}
		private void HideFindReplace() 
		{
			grid_replace.Visibility = System.Windows.Visibility.Collapsed;
			grid_find.Visibility = System.Windows.Visibility.Collapsed;
		}
		private void DoReplace() 
		{
			//Add to cache to allow undo
			replaceCache = activeDocument().Text;

			//Replace content
			if (rab_all_documents.IsChecked.Equals(true)) {
				foreach (LayoutDocument ld in documentPanel.Children) {
					((ld.Content as Grid).Children[0] as TextEditor).Text = 
						((ld.Content as Grid).Children[0] as TextEditor
						).Text.Replace(txt_find.Text, txt_replace.Text);
				}
				btn_undo_replace.IsEnabled = true;
			}
			else if (rab_current_document.IsChecked.Equals(true)) {
				activeDocument().Text = activeDocument().Text.Replace(txt_find.Text, txt_replace.Text);
				btn_undo_replace.IsEnabled = true;
			}
			else if (rab_selection.IsChecked.Equals(true)) {
				activeDocument().SelectedText = activeDocument().SelectedText.Replace(txt_find.Text, txt_replace.Text);
				btn_undo_replace.IsEnabled = true;
			}
		}
		private void IndentRight() 
		{
			//The user have to select the line to make it work
			try { activeDocument().SelectedText = "\t" + activeDocument().SelectedText.Replace("\n", "\n\t"); }
			catch { }
		}
		private void IndentLeft() 
		{
			try {
				//The user have to select the line to make it work
				if (activeDocument().SelectedText.StartsWith("\t"))
					activeDocument().SelectedText = activeDocument().SelectedText.Remove(0, 1);

				//Remove indentation
				activeDocument().SelectedText = activeDocument().SelectedText.Replace("\n\t", "\n");
			}
			catch { }
		}
		private void documentStats() 
		{
			lbl_count.Content = "Line: " + activeDocument().TextArea.Caret.Line +
				" of " + activeDocument().LineCount + " | Column: " + activeDocument().TextArea.Caret.Column +
				" | Selected: " + activeDocument().SelectionLength + " of " + activeDocument().Text.Length + " chars";
		}
		#endregion

		#region Buttons
		private void btn_replace_document_Click(object sender, RoutedEventArgs e) {
			DoReplace();
		}
		private void btn_close_replace_Click(object sender, RoutedEventArgs e) {
			HideFindReplace();
		}
		private void btn_close_find_Click(object sender, RoutedEventArgs e) {
			HideFindReplace();
		}
		private void btn_find_document_Click(object sender, RoutedEventArgs e) {
			FindInDocument();
		}
		private void FindInDocument() {
			try {
				//Get index from search
				searchIndex = activeDocument().Text.ToLower().IndexOf(
					txt_search.Text.ToLower(), searchIndex + txt_search.Text.Length);

				//Rescan
				if (searchIndex < 0)
					searchIndex = activeDocument().Text.ToLower(
						).IndexOf(txt_search.Text.ToLower(), 0);

				//Find text
				activeDocument().Select(searchIndex, txt_search.Text.Length);
				activeDocument().ScrollTo(activeDocument().TextArea.Caret.Line,
					activeDocument().TextArea.Caret.Column);

				//Count
				int counter = System.Text.RegularExpressions.Regex.Matches(activeDocument().Text.ToLower(), txt_search.Text.ToLower()).Count;
				ShowStatus(counter.ToString() + " matches was found in this document");
			}
			catch { searchIndex = -1; }
		}
		#endregion

		#region FTP browser
		//Variables for storing server related information
		private string userName;
		private string passWord;
		public string startingPath;

		//Our FtpWebRequest object
		FtpWebRequest request;

		//
		//Various dictionaries for creating mappings between folders, treeitems, and dockpanels
		//
		private Dictionary<string, TreeViewItem> folderNameToTreeItem = new Dictionary<string, TreeViewItem>();
		private Dictionary<string, TreeViewItem> seekAheadList = new Dictionary<string, TreeViewItem>();
		private Dictionary<TreeViewItem, string> treeItemToFolderName = new Dictionary<TreeViewItem, string>();
		private Dictionary<string, string> namePathDictionary = new Dictionary<string, string>();
		private Dictionary<DockPanel, string> dockPanelContent = new Dictionary<DockPanel, string>();

		//
		// Returns a folder name and strips out any folders with an extension (aka files)
		//
		private string ParseLine(string line, string path) {
			string[] parsedLine = line.Split('/');
			string temp;
			if (parsedLine.Length > 1) {
				temp = path + "/" + parsedLine[1];
				//if (parsedLine[1].Contains(".")) {
				//	return "ERROR";
				//}
			}
			else {
				temp = path + "/" + parsedLine[0];
				//if (parsedLine[0].Contains(".")) {
				//	return "ERROR";
				//}
			}
			return temp;
		}

		//
		// Initializes the application
		//
		private void SetupFTP() {
			try {
				//Clear any previous connection
				treeFolderBrowser.Items.Clear();
				folderNameToTreeItem.Clear();
				seekAheadList.Clear();
				treeItemToFolderName.Clear();
				namePathDictionary.Clear();
				dockPanelContent.Clear();
				fileList.Items.Clear();

				startingPath = ftpBox.Text;
				userName = userNameBox.Text;
				passWord = passwordBox.Password;
				isSecureConnection = chbx_ssl.IsChecked.Value;

				//Disable elements
				ftpBox.IsEnabled = false;
				userNameBox.IsEnabled = false;
				passwordBox.IsEnabled = false;
				chbx_ssl.IsEnabled = false;

				//Enable upload
				btn_publish.IsEnabled = true;
				btn_publish_all.IsEnabled = true;

				//Change status and load filesystem
				this.Title = "WalrusPad - " + userNameBox.Text + "@" + ftpBox.Text.Replace("ftp://", "");
				isConnectedToFTP = true;
				ConnectButton.Content = "Disconnect";
				menuToolsConnect.Header = "Disconnect FTP";
				if (isSecureConnection) {
					SecureFTP sftp = new SecureFTP();
					sftp.setRemoteHost(ftpBox.Text);
					sftp.setRemoteUser(userNameBox.Text);
					sftp.setRemotePass(passwordBox.Password);
					sftp.setRemotePort(22);
					sftp.setRemotePath(ftpBox.Text + ":" + sftp.getRemotePort().ToString() + "/");
					sftp.login();
					string[] tmp = sftp.getFileList("...");
				}
				else
					request = (FtpWebRequest)WebRequest.Create(startingPath);
				SetupTree();

				//Save the connection
				if (!Directory.Exists(configPath + "\\connections"))
					Directory.CreateDirectory(configPath + "\\connections");
				if (!File.Exists(configPath + "\\connections\\" + userNameBox.Text + "@" + ftpBox.Text.Replace("ftp://", "") + ".n2c")) {
					StreamWriter w = new StreamWriter(configPath + "\\connections\\" + 
						userNameBox.Text + "@" + ftpBox.Text.Replace("ftp://", "") + ".n2c");
					w.WriteLine(userNameBox.Text + "@" + ftpBox.Text.Replace("ftp://", ""));
					w.WriteLine(acorp.Security.Encrypt(ftpBox.Text, encryptionKey));
					w.WriteLine(acorp.Security.Encrypt(userNameBox.Text, encryptionKey));
					w.WriteLine(acorp.Security.Encrypt(passwordBox.Password, encryptionKey));
					if (chbx_ssl.IsChecked.Equals(true))
						w.WriteLine(acorp.Security.Encrypt("true", encryptionKey));
					else
						w.WriteLine(acorp.Security.Encrypt("false", encryptionKey));
					w.Close();

					String fPath = configPath + "\\connections\\" +
						userNameBox.Text + "@" + ftpBox.Text.Replace("ftp://", "") + ".n2c";
					MenuItem item = new MenuItem();
					item.Header = System.IO.Path.GetFileName(fPath).Replace(".n2c", "");
					item.Click += new RoutedEventHandler(
					   delegate(object s, RoutedEventArgs earg) {
						   cbx_conn.Text = System.IO.Path.GetFileName(fPath).Replace(".n2c", "");
						   loadN2CFile();
					   });
					menuFile.Items.Add(item);
				}
			}
			catch (Exception ex){
				MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		//
		// Disconnect from FTP server and reset
		//
		private void DisconnectFTP() {
			//Enable elements
			ftpBox.IsEnabled = true;
			userNameBox.IsEnabled = true;
			passwordBox.IsEnabled = true;
			chbx_ssl.IsEnabled = true;

			//Disable upload
			btn_publish.IsEnabled = false;
			btn_publish_all.IsEnabled = false;

			//Reset data
			isConnectedToFTP = false;
			ConnectButton.Content = "Connect";
			menuToolsConnect.Header = "Connect to FTP server";
			this.Title = "WalrusPad";

			//Clear any previous connection
			treeFolderBrowser.Items.Clear();
			folderNameToTreeItem.Clear();
			seekAheadList.Clear();
			treeItemToFolderName.Clear();
			namePathDictionary.Clear();
			dockPanelContent.Clear();
			fileList.Items.Clear();
		}

		//
		// Returns the folder name without having any path information
		//
		private string SafeFolderName(string path) {
			string[] fileParts = path.Split('/');
			return fileParts[fileParts.Length - 1];
		}

		//
		// Responsible for initially drawing the tree
		//
		private void SetupTree() {
			treeFolderBrowser.Items.Clear();
			TreeViewItem tv = new TreeViewItem();

			DockPanel dock = new DockPanel();
			dock.MouseUp += new MouseButtonEventHandler(TreeViewMouseDown);

			TextBlock textContent = new TextBlock();
			textContent.Text = startingPath;

			dock.Children.Add(GetImage(true));
			dock.Children.Add(textContent);

			dockPanelContent.Add(dock, startingPath);

			tv.Header = dock;

			treeFolderBrowser.Items.Add(tv);

			folderNameToTreeItem.Add(startingPath, tv);
			treeItemToFolderName.Add(tv, startingPath);

			FTPFolder folder = new FTPFolder(startingPath, SafeFolderName(startingPath));
			DrawChildTreeItems(tv, folder, true);
			treeFolderBrowser.Focus();
		}

		//
		// Returns an image used by the TreeViewItem to designate a visited Node
		//
		private Image GetImage(bool expanded) {
			Image imageContent = new Image();
			string imagePath;

			if (expanded) {
				imagePath = "pack://application:,,,/img/folder-open-icon.png";
			}
			else {
				imagePath = "pack://application:,,,/img/folder-close-icon.png";
			}
			imageContent.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(imagePath));
			return imageContent;
		}

		//
		// Given a parent TreeViewItem, FTPFolder, and an expand flag, draws the children folders
		// in your directory structure
		//
		private void DrawChildTreeItems(TreeViewItem parentTree, FTPFolder folder, bool expand) {
			List<string> childPaths = GetDirectories(folder.GetPath());
			List<TreeViewItem> treeViewItemList = new List<TreeViewItem>();

			foreach (string t in childPaths) {
				if (folderNameToTreeItem.ContainsKey(t)) {
					continue;
				}
				else {
					TreeViewItem tv = new TreeViewItem();
					FTPFolder newFolder = new FTPFolder(t, SafeFolderName(t));

					DockPanel dock = new DockPanel();
					dock.MouseUp += new MouseButtonEventHandler(TreeViewMouseDown);

					TextBlock textContent = new TextBlock();
					textContent.Text = newFolder.GetName();

					dock.Children.Add(GetImage(false));
					dock.Children.Add(textContent);
					dockPanelContent.Add(dock, newFolder.GetPath());

					treeViewItemList.Add(tv);


					tv.Header = dock;
					parentTree.Items.Add(tv);

					folderNameToTreeItem.Add(t, tv);
					treeItemToFolderName.Add(tv, t);

					if (expand) {
						parentTree.IsExpanded = true;
					}
					else {
						parentTree.IsExpanded = false;
					}
				}
			}
		}

		//
		// Our event handler for reacting to mousedown events
		//
		private void TreeViewMouseDown(object sender, MouseButtonEventArgs e) {
			DockPanel clickedDock = (DockPanel)sender;
			clickedDock.Children.RemoveAt(0);
			clickedDock.Children.Insert(0, GetImage(true));
			string clickedText = dockPanelContent[clickedDock];

			FTPFolder clickedFolder = new FTPFolder(clickedText, SafeFolderName(clickedText));
			DrawChildTreeItems(folderNameToTreeItem[clickedText], clickedFolder, true);
			ftpBox.Text = clickedText;
			LoadFilesToList(clickedText);
		}

		private void LoadFilesToList(string path) {
			request = (FtpWebRequest)WebRequest.Create(path);
			request.Credentials = new NetworkCredential(userName, passWord);
			request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
			request.EnableSsl = isSecureConnection;
			fileList.Items.Clear();
			try {
				FtpWebResponse response = (FtpWebResponse)request.GetResponse();
				StreamReader reader = new StreamReader(response.GetResponseStream());

				string line = reader.ReadLine();
				string fName = line.Split(' ')[line.Split(' ').Length - 1];
				while (fName != null) {
					string newPath = ParseLine(fName, path);
					if (line.StartsWith("-") && fName != "." && fName != "..") {
						fileList.Items.Add(SafeFolderName(newPath));
					}

					//Get next item
					line = reader.ReadLine();
					if (line != null)
						fName = line.Split(' ')[line.Split(' ').Length - 1];
					else
						fName = null;
				}
			}
			catch (WebException e) {
				MessageBox.Show(e.Message);
			}
		}
		//
		// Connects to the server and returns a list of directories located at the
		// given path.
		//
		private List<string> GetDirectories(string path) {
			request = (FtpWebRequest)WebRequest.Create(path);
			request.Credentials = new NetworkCredential(userName, passWord);
			request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
			request.EnableSsl = isSecureConnection;

			List<string> dir = new List<string>();

			try {
				FtpWebResponse response = (FtpWebResponse)request.GetResponse();
				StreamReader reader = new StreamReader(response.GetResponseStream());

				string line = reader.ReadLine();
				string fName = line.Split(' ')[line.Split(' ').Length-1];
				while (fName != null) {
					string newPath = ParseLine(fName, path);
					if (line.StartsWith("d") && fName != "." && fName != "..") {
						dir.Add(newPath);
					}

					//Get next item
					line = reader.ReadLine();
					if (line != null)
						fName = line.Split(' ')[line.Split(' ').Length - 1];
					else
						fName = null;
				}
			}
			catch (WebException e) {
				MessageBox.Show(e.Message);
			}
			return dir;
		}

		private void ConnectClick(object sender, RoutedEventArgs e) {
			//Collapse and connect to FTP server
			if (!isConnectedToFTP) {
				ftp_browser_expander.IsExpanded = false;
				SetupFTP();
			}
			else {
				DisconnectFTP();
			}
		}

		//Download FTP files
		private void fileList_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			DownloadFTPFiles();
		}
		//Publish current FTP file
		private void Publish() {
			Save(true);
		}
		//Publish all FTP files
		private void PublishAll() {
			SaveAll(true);
		}
		//Hide expandable panel when loosing focus
		private void treeFolderBrowser_GotFocus(object sender, RoutedEventArgs e) {
			ftp_browser_expander.IsExpanded = false;
		}
		private string[] DownloadFTPFiles(bool loadToEditor = true) {
			try {
				//Check if items are selected
				if (fileList.SelectedItems.Count > 0) {
					//Create directory if doesn't exist
					string downloadDir = webFilesPath + ftpBox.Text.Replace("ftp://", "").Replace("/", "\\");
					if (!Directory.Exists(downloadDir)) {
						Directory.CreateDirectory(downloadDir);
					}

					//Download FTP files
					List<string> filesToLoad = new List<string>();
					FTPLibrary ftpLib = new FTPLibrary(userNameBox.Text, passwordBox.Password, isSecureConnection);
					for (int i = 0; i < fileList.SelectedItems.Count; i++) {
						// Make backup if already exist (conflict)
						if (File.Exists(downloadDir + "\\" + fileList.SelectedItems[i].ToString())) {
							if (File.Exists(downloadDir + "\\" + fileList.SelectedItems[i].ToString() + "~"))
								File.Delete(downloadDir + "\\" + fileList.SelectedItems[i].ToString() + "~");
							File.Move(downloadDir + "\\" + fileList.SelectedItems[i].ToString(),
								downloadDir + "\\" + fileList.SelectedItems[i].ToString() + "~");
						}

						//Download FTP file
						ftpLib.Download(ftpBox.Text + "\\" + fileList.SelectedItems[i].ToString(),
							downloadDir + "\\" + fileList.SelectedItems[i].ToString());
						filesToLoad.Add(downloadDir + "\\" + fileList.SelectedItems[i].ToString());
					}
					//Load file into editor
					if (loadToEditor)
						Load(filesToLoad.ToArray());

					//Return file list
					return filesToLoad.ToArray();
				}
				return null;
			}
			catch (Exception ex) 
			{ 
				ShowStatus(ex.Message);
				return null;
			}
		}
		#endregion

		#region FTP manager
		//Handle uploading of files using drag drop
		private void fileList_DragEnter(object sender, DragEventArgs e) {
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
				e.Effects = DragDropEffects.Copy;
		}
		private void fileList_Drop(object sender, DragEventArgs e) {
			try {
				FTPLibrary ftpLib = new FTPLibrary(userNameBox.Text, passwordBox.Password, isSecureConnection);
				int count = 0;
				string log = "";
				if (e.Data is System.Windows.DataObject &&
					((System.Windows.DataObject)e.Data).ContainsFileDropList()) {
					foreach (string filePath in ((System.Windows.DataObject)e.Data).GetFileDropList()) {
						double size = ftpLib.GetFTPSize(ftpBox.Text + "/" + System.IO.Path.GetFileName(filePath));
						if (size > 0) {
							MessageBoxResult r;
							r = MessageBox.Show("The file: '" + System.IO.Path.GetFileName(filePath) +
								"' already exist on this server, do you want to overwrite?", "File exist",
								MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
							if (r == MessageBoxResult.Yes) {
								log = ftpLib.Upload(ftpBox.Text + "/" + System.IO.Path.GetFileName(filePath), filePath);
								count++;
							}
						}
						else {
							log = ftpLib.Upload(ftpBox.Text + "/" + System.IO.Path.GetFileName(filePath), filePath);
							fileList.Items.Add(System.IO.Path.GetFileName(filePath));
							count++;
						}
					}
				}
				if (log.StartsWith("Error"))
					ShowStatus(log);
				else
					ShowStatus("Succesfully uploaded " + count + " files at '" + ftpBox.Text + "'");
			}
			catch (Exception ex){
				ShowStatus(ex.Message);
			}
		}
		//Handle download of FTP files using dragdrop
		private Point start; 
		private void fileList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			this.start = e.GetPosition(null); 
		}
		private void fileList_MouseMove(object sender, MouseEventArgs e) {
			try {
				Point mpos = e.GetPosition(null);
				Vector diff = this.start - mpos;

				if (e.LeftButton == MouseButtonState.Pressed &&
					Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance &&
					Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance) {
					if (this.fileList.SelectedItems.Count == 0) {
						return;
					}

					//Download files to local FTP server
					string[] files = DownloadFTPFiles(false);

					//Copy directory to drop path
					if (files != null) {
						string dataFormat = DataFormats.FileDrop;
						DataObject dataObject = new DataObject(dataFormat, files);
						DragDrop.DoDragDrop(this.fileList, dataObject, DragDropEffects.Copy);
					}
				}
			}
			catch (Exception ex) { ShowStatus(ex.Message); }
		}
		//Handle selection of saved connections
		private void cbx_conn_DropDownOpened(object sender, EventArgs e) {
			//Clear list of connections
			cbx_conn.Items.Clear();

			//Create directory if it doesn't exist
			if (!Directory.Exists(configPath + "\\connections"))
				Directory.CreateDirectory(configPath + "\\connections");

			//Get connection files
			string[] connections = Directory.GetFiles(configPath + "\\connections\\");
			for (int i = 0; i < connections.Length; i++) {
				if (connections[i].EndsWith(".n2c"))
					cbx_conn.Items.Add(System.IO.Path.GetFileName(connections[i]).Replace(".n2c", ""));
			}
		}
		private void cbx_conn_DropDownClosed(object sender, EventArgs e) {
			if (cbx_conn.Text != "")
				loadN2CFile();
		}
		private void loadN2CFile() {
			try {
				//Load connection details
				StreamReader r = new StreamReader(configPath + "\\connections\\" +
					cbx_conn.Text + ".n2c");
				string[] tmp = r.ReadToEnd().Split('\n');
				ftpBox.Text = (acorp.Security.Decrypt(tmp[1].Substring(0, tmp[1].Length - 1), encryptionKey));
				userNameBox.Text = (acorp.Security.Decrypt(tmp[2].Substring(0, tmp[2].Length - 1), encryptionKey));
				passwordBox.Password = (acorp.Security.Decrypt(tmp[3].Substring(0, tmp[3].Length - 1), encryptionKey));

				if (tmp.Length > 4 && tmp[4] != "")
					if ((acorp.Security.Decrypt(tmp[4].Substring(0, tmp[4].Length - 1), encryptionKey)).Equals("true"))
						chbx_ssl.IsChecked = true;
				r.Close();

				//Connect to server
				ftp_browser_expander.IsExpanded = false;
				SetupFTP();
			}
			catch (Exception ex) {
				ShowStatus("Error: " + ex.Message);
			}
		}
		//Select on focus
		private void txt_search_GotFocus(object sender, RoutedEventArgs e) {
			txt_search.SelectAll();
		}
		private void txt_replace_GotFocus(object sender, RoutedEventArgs e) {
			txt_replace.SelectAll();
		}
		private void txt_find_GotFocus(object sender, RoutedEventArgs e) {
			txt_find.SelectAll();
		}
		#endregion	

		//END
    }
}
