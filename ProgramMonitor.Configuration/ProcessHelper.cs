using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;

namespace ProgramMonitor.Configuration
{
	public static class ProcessHelper
	{
		public const int TOKEN_QUERY = 0X00000008;

		const int ERROR_NO_MORE_ITEMS = 259;

		enum TOKEN_INFORMATION_CLASS
		{
			TokenUser = 1,
			TokenGroups,
			TokenPrivileges,
			TokenOwner,
			TokenPrimaryGroup,
			TokenDefaultDacl,
			TokenSource,
			TokenType,
			TokenImpersonationLevel,
			TokenStatistics,
			TokenRestrictedSids,
			TokenSessionId
		}

		enum SID_NAME_USE
		{
			SidTypeUser = 1,
			SidTypeGroup,
			SidTypeDomain,
			SidTypeAlias,
			SidTypeWellKnownGroup,
			SidTypeDeletedAccount,
			SidTypeInvalid,
			SidTypeUnknown,
			SidTypeComputer
		}

		public struct TOKEN_USER
		{
			public SID_AND_ATTRIBUTES User;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SID_AND_ATTRIBUTES
		{

			public IntPtr Sid;
			public int Attributes;
		}

		[DllImport("advapi32")]
		static extern bool OpenProcessToken(
			IntPtr ProcessHandle, // handle to process
			int DesiredAccess, // desired access to process
			ref IntPtr TokenHandle // handle to open access token
		);

		[DllImport("kernel32")]
		static extern bool CloseHandle(IntPtr handle);

		[DllImport("advapi32", CharSet = CharSet.Auto)]
		static extern bool GetTokenInformation(
			IntPtr hToken,
			TOKEN_INFORMATION_CLASS tokenInfoClass,
			IntPtr TokenInformation,
			int tokeInfoLength,
			ref int reqLength
		);

		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		static extern bool LookupAccountSid(
			string lpSystemName,
			IntPtr Sid,
			//[MarshalAs(UnmanagedType.LPArray)] byte[] Sid,
			System.Text.StringBuilder lpName,
			ref uint cchName,
			System.Text.StringBuilder ReferencedDomainName,
			ref uint cchReferencedDomainName,
			out SID_NAME_USE peUse);        

		public static Process[] GetProcessesForUser(string userName)
		{
			List<Process> result = new List<Process>(30);
			List<Process> allProcesses = new List<Process>(Process.GetProcesses());
			foreach(Process process in allProcesses)
			{
				try
				{
					if (string.Compare(userName, GetProcessOwner(process.Handle), StringComparison.InvariantCultureIgnoreCase) == 0)
					{
						result.Add(process);
					}
				}
				catch (Win32Exception ex)
				{
					if (ex.NativeErrorCode != 5)
						throw;
				}
				catch(InvalidOperationException ex)
				{
					// skip - probably for process exited
				}
			}
			return result.ToArray();
		}

		public static string[] GetUserAccounts()
		{
			List<string> result = new List<string>(5);
			SelectQuery sQuery = new SelectQuery("Win32_UserAccount", "LocalAccount=true");

			try
			{
				ManagementObjectSearcher mSearcher = new ManagementObjectSearcher(sQuery);
				ManagementObjectCollection userSet = mSearcher.Get();

				foreach (ManagementObject mObject in userSet)
				{
					result.Add((string)mObject["name"]);
				}
			}
			catch (Exception ex)
			{
				Logger.LogException(ex);
			}

			return result.ToArray();
		}

		private static string GetProcessOwner(IntPtr hProcess)
		{
			string result = "(unknown)";
			try
			{
				IntPtr hProcessToken = IntPtr.Zero;
				if (OpenProcessToken(hProcess, TOKEN_QUERY, ref hProcessToken))
				{
					TOKEN_USER tokUser;
					int bufLength = 256;
					IntPtr tu = Marshal.AllocHGlobal(bufLength);

					try
					{
						GetTokenInformation(hProcessToken, TOKEN_INFORMATION_CLASS.TokenUser, tu, bufLength, ref bufLength);
						tokUser = (TOKEN_USER)Marshal.PtrToStructure(tu, typeof(TOKEN_USER));

						StringBuilder name = new StringBuilder();
						uint cchName = (uint)name.Capacity;
						StringBuilder referencedDomainName = new StringBuilder();
						uint cchReferencedDomainName = (uint)referencedDomainName.Capacity;
						SID_NAME_USE sidUse;

						if (LookupAccountSid(null, tokUser.User.Sid, name, ref cchName, referencedDomainName, ref cchReferencedDomainName, out sidUse))
						{
							result = name.ToString();
						}
					}
					finally
					{
						Marshal.FreeHGlobal(tu);
						CloseHandle(hProcessToken);
					}
				}
			}
			catch (Exception ex)
			{
				Logger.LogException(ex);
			}
			return result;
		}
	}
}
