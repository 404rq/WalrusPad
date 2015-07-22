using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace WalrusPad
{
	public class SecureFTP
	{
		private string remoteHost, remotePath, remoteUser, remotePass, mes;
		private int remotePort, bytes;
		private Socket clientSocket;
		private int retValue;
		private Boolean debug;
		private Boolean logined;
		private string reply;
		private bool useStream;
		private bool isUpload;
		private Stream stream = null;
		private Stream stream2 = null;

		private static int BLOCK_SIZE = 1024;

		Byte[] buffer = new Byte[BLOCK_SIZE];
		Encoding UTF8 = Encoding.UTF8;

		public SecureFTP() {

			remoteHost = string.Empty;
			remotePath = ".";
			remoteUser = string.Empty;
			remotePass = string.Empty;
			remotePort = 21;
			debug = false;
			logined = false;

		}

		///
		/// Set the name of the FTP server to connect to.
		///
		/// Server name
		public void setRemoteHost(string remoteHost) {
			this.remoteHost = remoteHost;
		}

		public void setUseStream(bool value) {
			this.useStream = value;
		}

		///
		/// Return the name of the current FTP server.
		///
		/// Server name
		public string getRemoteHost() {
			return remoteHost;
		}

		///
		/// Set the port number to use for FTP.
		///
		/// Port number
		public void setRemotePort(int remotePort) {
			this.remotePort = remotePort;
		}

		///
		/// Return the current port number.
		///
		/// Current port number
		public int getRemotePort() {
			return remotePort;
		}

		///
		/// Set the remote directory path.
		///
		/// The remote directory path
		public void setRemotePath(string remotePath) {
			this.remotePath = remotePath;
		}

		///
		/// Return the current remote directory path.
		///
		/// The current remote directory path.
		public string getRemotePath() {
			return remotePath;
		}

		///
		/// Set the user name to use for logging into the remote server.
		///
		/// Username
		public void setRemoteUser(string remoteUser) {
			this.remoteUser = remoteUser;
		}

		///
		/// Set the password to user for logging into the remote server.
		///
		/// Password
		public void setRemotePass(string remotePass) {
			this.remotePass = remotePass;
		}

		///
		/// Return a string array containing the remote directory's file list.
		///
		///
		///
		public string[] getFileList(string mask) {

			if (!logined) {
				login();
			}

			Socket cSocket = createDataSocket();

			sendCommand("NLST " + mask);

			if (!(retValue == 150 || retValue == 125)) {
				throw new IOException(reply.Substring(4));
			}

			mes = "";

			while (true) {

				int bytes = cSocket.Receive(buffer, buffer.Length, 0);
				mes += UTF8.GetString(buffer, 0, bytes);

				if (bytes < buffer.Length) {
					break;
				}
			}

			string[] seperator = { "\r\n" };
			string[] mess = mes.Split(seperator, StringSplitOptions.RemoveEmptyEntries);

			cSocket.Close();

			readReply();

			if (retValue != 226) {
				throw new IOException(reply.Substring(4));
			}
			return mess;

		}

		///
		/// Return the size of a file.
		///
		///
		///
		public long getFileSize(string fileName) {

			if (!logined) {
				login();
			}

			sendCommand("SIZE " + fileName);
			long size = 0;

			if (retValue == 213) {
				size = Int64.Parse(reply.Substring(4));
			}
			else {
				throw new IOException(reply.Substring(4));
			}

			return size;

		}


		public void login() {
			//SslStream sslStream = new SslStream(client.GetStream(), false);

			try {
				if (clientSocket == null || clientSocket.Connected == false)
					this.loginWithoutUser();
			}
			catch (Exception) {
				throw new IOException("Couldn't connect to remote server");
			}


			if (debug)
				Console.WriteLine("USER " + remoteUser);

			sendCommand("USER " + remoteUser);

			if (!(retValue == 331 || retValue == 230)) {
				cleanup();
				throw new IOException(reply.Substring(4));
			}

			if (retValue != 230) {
				if (debug)
					Console.WriteLine("PASS xxx");

				sendCommand("PASS " + remotePass);
				if (!(retValue == 230 || retValue == 202)) {
					cleanup();
					throw new IOException(reply.Substring(4));
				}
			}

			logined = true;
			Console.WriteLine("Connected to " + remoteHost);

			chdir(remotePath);

		}
		///
		/// Login to the remote server.
		///
		public void loginWithoutUser() {
			//SslStream sslStream = new SslStream(client.GetStream(), false);
			clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			IPEndPoint ep = new IPEndPoint(Dns.GetHostEntry(remoteHost).AddressList[0], remotePort);

			try {
				clientSocket.Connect(ep);
			}
			catch (Exception) {
				throw new IOException("Couldn't connect to remote server");
			}

			readReply();
			if (retValue != 220) {
				close();
				throw new IOException(reply.Substring(4));
			}


		}

		private static bool OnCertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) {

			Console.WriteLine("Server Certificate Issued To: {0}", certificate.Subject);

			Console.WriteLine("Server Certificate Issued By: {0}", certificate.Issuer);


			// Return true if there are no policy errors

			// The certificate can also be manually verified to 

			//make sure it meets your specific // policies by 

			//   interrogating the x509Certificate object.

			if (errors != SslPolicyErrors.None) {

				Console.WriteLine("Server Certificate Validation Error");

				Console.WriteLine(errors.ToString());

				return false;

			}

			else {

				Console.WriteLine("No Certificate Validation Errors");

				return true;

			}

		}

		private void showCertificateInfo(X509Certificate remoteCertificate, bool verbose) {
			Console.WriteLine("Certficate Information for:\n{0}\n", remoteCertificate.Subject);
			Console.WriteLine("Valid From: \n{0}", remoteCertificate.GetEffectiveDateString());
			Console.WriteLine("Valid To: \n{0}", remoteCertificate.GetExpirationDateString());
			Console.WriteLine("Certificate Format: \n{0}\n", remoteCertificate.GetFormat());

			Console.WriteLine("Issuer Name: \n{0}", remoteCertificate.Issuer);

			if (verbose) {
				Console.WriteLine("Serial Number: \n{0}", remoteCertificate.GetSerialNumberString());
				Console.WriteLine("Hash: \n{0}", remoteCertificate.GetCertHashString());
				Console.WriteLine("Key Algorithm: \n{0}", remoteCertificate.GetKeyAlgorithm());
				Console.WriteLine("Key Algorithm Parameters: \n{0}", remoteCertificate.GetKeyAlgorithmParametersString());
				Console.WriteLine("Public Key: \n{0}", remoteCertificate.GetPublicKeyString());
			}
		}
		private void showSslInfo(string serverName, SslStream sslStream, bool verbose) {
			showCertificateInfo(sslStream.RemoteCertificate, verbose);

			Console.WriteLine("\n\nSSL Connect Report for : {0}\n", serverName);
			Console.WriteLine("Is Authenticated: {0}", sslStream.IsAuthenticated);
			Console.WriteLine("Is Encrypted: {0}", sslStream.IsEncrypted);
			Console.WriteLine("Is Signed: {0}", sslStream.IsSigned);
			Console.WriteLine("Is Mutually Authenticated: {0}\n", sslStream.IsMutuallyAuthenticated);

			Console.WriteLine("Hash Algorithm: {0}", sslStream.HashAlgorithm);
			Console.WriteLine("Hash Strength: {0}", sslStream.HashStrength);
			Console.WriteLine("Cipher Algorithm: {0}", sslStream.CipherAlgorithm);
			Console.WriteLine("Cipher Strength: {0}\n", sslStream.CipherStrength);

			Console.WriteLine("Key Exchange Algorithm: {0}", sslStream.KeyExchangeAlgorithm);
			Console.WriteLine("Key Exchange Strength: {0}\n", sslStream.KeyExchangeStrength);
			Console.WriteLine("SSL Protocol: {0}", sslStream.SslProtocol);
		}

		public void getSslStream() {
			this.getSslStream(clientSocket);
		}
		public void getSslStream(Socket Csocket) {
			//SslStream sslStream = new SslStream(client.GetStream(), false);

			RemoteCertificateValidationCallback callback = new RemoteCertificateValidationCallback(OnCertificateValidation);
			SslStream _sslStream = new SslStream(new NetworkStream(Csocket));//,new RemoteCertificateValidationCallback(ValidateServerCertificate), null);

			try {


				_sslStream.AuthenticateAsClient(
				  remoteHost,
				  null,
				  System.Security.Authentication.SslProtocols.Ssl3 | System.Security.Authentication.SslProtocols.Tls,
				  true);

				if (_sslStream.IsAuthenticated)
					if (isUpload)
						stream2 = _sslStream;
					else
						stream = _sslStream;

			}
			catch (Exception ex) {
				throw new IOException(ex.Message);
			}

			showSslInfo(remoteHost, _sslStream, true);

			// readReply();

		}


		///
		/// If the value of mode is true, set binary mode for downloads.
		/// Else, set Ascii mode.
		///
		///
		public void setBinaryMode(Boolean mode) {

			if (mode) {
				sendCommand("TYPE I");
			}
			else {
				sendCommand("TYPE A");
			}
			if (retValue != 200) {
				throw new IOException(reply.Substring(4));
			}
		}

		///
		/// Download a file to the Assembly's local directory,
		/// keeping the same file name.
		///
		///
		public void download(string remFileName) {
			download(remFileName, "", false);
		}

		///
		/// Download a remote file to the Assembly's local directory,
		/// keeping the same file name, and set the resume flag.
		///
		///
		///
		public void download(string remFileName, Boolean resume) {
			download(remFileName, "", resume);
		}

		///
		/// Download a remote file to a local file name which can include
		/// a path. The local file name will be created or overwritten,
		/// but the path must exist.
		///
		///
		///
		public void download(string remFileName, string locFileName) {
			download(remFileName, locFileName, false);
		}

		///
		/// Download a remote file to a local file name which can include
		/// a path, and set the resume flag. The local file name will be
		/// created or overwritten, but the path must exist.
		///
		///
		///
		///
		public void download(string remFileName, string locFileName, Boolean resume) {
			if (!logined) {
				login();
			}

			setBinaryMode(true);

			Console.WriteLine("Downloading file " + remFileName + " from " + remoteHost + "/" + remotePath);

			if (locFileName.Equals("")) {
				locFileName = remFileName;
			}

			if (!File.Exists(locFileName)) {
				Stream st = File.Create(locFileName);
				st.Close();
			}

			FileStream output = new FileStream(locFileName, FileMode.Open);

			Socket cSocket = createDataSocket();

			long offset = 0;

			if (resume) {

				offset = output.Length;

				if (offset > 0) {
					sendCommand("REST " + offset);
					if (retValue != 350) {
						//throw new IOException(reply.Substring(4));
						//Some servers may not support resuming.
						offset = 0;
					}
				}

				if (offset > 0) {
					if (debug) {
						Console.WriteLine("seeking to " + offset);
					}
					long npos = output.Seek(offset, SeekOrigin.Begin);
					Console.WriteLine("new pos=" + npos);
				}
			}

			sendCommand("RETR " + remFileName);

			if (!(retValue == 150 || retValue == 125)) {
				throw new IOException(reply.Substring(4));
			}

			while (true) {

				bytes = cSocket.Receive(buffer, buffer.Length, 0);
				output.Write(buffer, 0, bytes);

				if (bytes <= 0) {
					break;
				}
			}

			output.Close();
			if (cSocket.Connected) {
				cSocket.Close();
			}

			Console.WriteLine("");

			readReply();

			if (!(retValue == 226 || retValue == 250)) {
				throw new IOException(reply.Substring(4));
			}

		}

		///
		/// Upload a file.
		///
		///

		///
		/// Secure Upload a file and set the resume flag.
		///
		///
		///
		public void uploadSecure(string fileName, Boolean resume) {

			sendCommand("PASV");

			if (retValue != 227) {
				throw new IOException(reply.Substring(4));
			}

			if (!logined) {
				login();
			}

			Socket cSocket = createDataSocket();
			isUpload = true;

			this.getSslStream(cSocket);

			long offset = 0;

			if (resume) {

				try {

					setBinaryMode(true);
					offset = getFileSize(fileName);

				}
				catch (Exception) {
					offset = 0;
				}
			}

			if (offset > 0) {
				sendCommand("REST " + offset);
				if (retValue != 350) {
					//throw new IOException(reply.Substring(4));
					//Remote server may not support resuming.
					offset = 0;
				}
			}

			sendCommand("STOR " + Path.GetFileName(fileName));

			if (!(retValue == 125 || retValue == 150)) {
				throw new IOException(reply.Substring(4));
			}


			FileStream input = File.OpenRead(fileName);
			byte[] bufferFile = new byte[input.Length];

			input.Read(bufferFile, 0, bufferFile.Length);
			input.Close();

			if (offset != 0) {

				if (debug) {
					Console.WriteLine("seeking to " + offset);
				}
				input.Seek(offset, SeekOrigin.Begin);
			}

			Console.WriteLine("Uploading file " + fileName + " to " + remotePath);

			if (cSocket.Connected) {

				this.stream2.Write(bufferFile, 0, bufferFile.Length);
				Console.WriteLine("File Upload");

			}


			this.stream2.Close();
			if (cSocket.Connected) {
				cSocket.Close();
			}

			readReply();
			if (!(retValue == 226 || retValue == 250)) {
				throw new IOException(reply.Substring(4));
			}


		}
		///
		/// Upload a file and set the resume flag.
		///
		///
		///
		public void upload(string fileName, Boolean resume) {

			if (!logined) {
				login();
			}


			Socket cSocket = createDataSocket();
			long offset = 0;
			isUpload = true;
			if (resume) {

				try {

					setBinaryMode(true);
					offset = getFileSize(fileName);

				}
				catch (Exception) {
					offset = 0;
				}
			}

			if (offset > 0) {
				sendCommand("REST " + offset);
				if (retValue != 350) {
					//throw new IOException(reply.Substring(4));
					//Remote server may not support resuming.
					offset = 0;
				}
			}

			sendCommand("STOR " + Path.GetFileName(fileName));

			if (!(retValue == 125 || retValue == 150)) {
				throw new IOException(reply.Substring(4));
			}

			// open input stream to read source file
			FileStream input = new FileStream(fileName, FileMode.Open);

			if (offset != 0) {

				if (debug) {
					Console.WriteLine("seeking to " + offset);
				}
				input.Seek(offset, SeekOrigin.Begin);
			}

			Console.WriteLine("Uploading file " + fileName + " to " + remotePath);

			while ((bytes = input.Read(buffer, 0, buffer.Length)) > 0) {

				cSocket.Send(buffer, bytes, 0);

			}
			input.Close();

			Console.WriteLine("");

			if (cSocket.Connected) {
				cSocket.Close();
			}

			readReply();
			if (!(retValue == 226 || retValue == 250)) {
				throw new IOException(reply.Substring(4));
			}
		}



		///
		/// Delete a file from the remote FTP server.
		///
		///
		public void deleteRemoteFile(string fileName) {

			if (!logined) {
				login();
			}

			sendCommand("DELE " + fileName);

			if (retValue != 250) {
				throw new IOException(reply.Substring(4));
			}

		}

		///
		/// Rename a file on the remote FTP server.
		///
		///
		///
		public void renameRemoteFile(string oldFileName, string newFileName) {

			if (!logined) {
				login();
			}

			sendCommand("RNFR " + oldFileName);

			if (retValue != 350) {
				throw new IOException(reply.Substring(4));
			}

			// known problem
			// rnto will not take care of existing file.
			// i.e. It will overwrite if newFileName exist
			sendCommand("RNTO " + newFileName);
			if (retValue != 250) {
				throw new IOException(reply.Substring(4));
			}

		}

		///
		/// Create a directory on the remote FTP server.
		///
		///
		public void mkdir(string dirName) {

			if (!logined) {
				login();
			}

			sendCommand("MKD " + dirName);

			if (retValue != 250) {
				throw new IOException(reply.Substring(4));
			}

		}

		///
		/// Delete a directory on the remote FTP server.
		///
		///
		public void rmdir(string dirName) {

			if (!logined) {
				login();
			}

			sendCommand("RMD " + dirName);

			if (retValue != 250) {
				throw new IOException(reply.Substring(4));
			}

		}

		///
		/// Change the current working directory on the remote FTP server.
		///
		///
		public void chdir(string dirName) {

			if (dirName.Equals(".")) {
				return;
			}

			if (!logined) {
				login();
			}

			sendCommand("CWD " + dirName);

			if (retValue != 250) {
				throw new IOException(reply.Substring(4));
			}

			this.remotePath = dirName;

			Console.WriteLine("Current directory is " + remotePath);

		}

		///
		/// Close the FTP connection.
		///
		public void close() {

			if (clientSocket != null) {
				sendCommand("QUIT");
			}

			cleanup();
			Console.WriteLine("Closing...");
		}

		///
		/// Set debug mode.
		///
		///
		public void setDebug(Boolean debug) {
			this.debug = debug;
		}

		private void readReply() {
			if (useStream)
				reply = ResponseMsg();
			else {

				mes = "";
				reply = readLine();
				retValue = Int32.Parse(reply.Substring(0, 3));
			}
		}

		private void cleanup() {
			if (clientSocket != null) {
				clientSocket.Close();
				clientSocket = null;
			}
			logined = false;
		}

		private string readLine() {

			while (true) {
				if (useStream)
					bytes = stream.Read(buffer, buffer.Length, 0);
				else
					bytes = clientSocket.Receive(buffer, buffer.Length, 0);


				mes += UTF8.GetString(buffer, 0, bytes);
				if (bytes < buffer.Length) {
					break;
				}
			}

			char[] seperator = { '\n' };
			string[] mess = mes.Split(seperator);

			if (mes.Length > 2) {
				mes = mess[mess.Length - 2];
			}
			else {
				mes = mess[0];
			}

			if (!mes.Substring(3, 1).Equals(" ")) {
				return readLine();
			}

			if (debug) {
				for (int k = 0; k < mess.Length - 1; k++) {
					Console.WriteLine(mess[k]);
				}
			}
			return mes;
		}

		private void WriteMsg(string message) {
			System.Text.UTF8Encoding en = new System.Text.UTF8Encoding();

			byte[] WriteBuffer = new byte[1024];
			WriteBuffer = en.GetBytes(message);

			stream.Write(WriteBuffer, 0, WriteBuffer.Length);

			Console.WriteLine(" WRITE:" + message);
		}

		private string ResponseMsg() {
			System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();
			byte[] serverbuff = new Byte[1024];
			int count = 0;
			while (true) {
				byte[] buff = new Byte[2];
				int bytes = stream.Read(buff, 0, 1);
				if (bytes == 1) {
					serverbuff[count] = buff[0];
					count++;

					if (buff[0] == '\n') {
						break;
					}
				}
				else {
					break;
				};
			};
			string retval = enc.GetString(serverbuff, 0, count);
			Console.WriteLine(" READ:" + retval);
			retValue = Int32.Parse(retval.Substring(0, 3));
			return retval;
		}
		public void sendCommand(String command) {

			Byte[] cmdBytes = Encoding.UTF8.GetBytes((command + "\r\n").ToCharArray());

			if (useStream) {
				WriteMsg(command + "\r\n");
			}
			else
				clientSocket.Send(cmdBytes, cmdBytes.Length, 0);


			readReply();
		}



		public Socket createDataSocket() {

			sendCommand("PASV");

			if (retValue != 227) {
				throw new IOException(reply.Substring(4));
			}


			int index1 = reply.IndexOf('(');
			int index2 = reply.IndexOf(')');
			string ipData = reply.Substring(index1 + 1, index2 - index1 - 1);
			int[] parts = new int[6];

			int len = ipData.Length;
			int partCount = 0;
			string buf = "";

			for (int i = 0; i < len && partCount <= 6; i++) {

				char ch = Char.Parse(ipData.Substring(i, 1));
				if (Char.IsDigit(ch))
					buf += ch;
				else if (ch != ',') {
					throw new IOException("Malformed PASV reply: " + reply);
				}

				if (ch == ',' || i + 1 == len) {

					try {
						parts[partCount++] = Int32.Parse(buf);
						buf = "";
					}
					catch (Exception) {
						throw new IOException("Malformed PASV reply: " + reply);
					}
				}
			}

			string ipAddress = parts[0] + "." + parts[1] + "." +
			 parts[2] + "." + parts[3];

			int port = (parts[4] << 8) + parts[5];

			Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			IPEndPoint ep = new IPEndPoint(Dns.GetHostEntry(ipAddress).AddressList[0], port);

			try {
				s.Connect(ep);
			}
			catch (Exception) {
				throw new IOException("Can't connect to remoteserver");
			}

			return s;
		}
	}
}
