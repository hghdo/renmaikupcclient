using System;
using System.Runtime.InteropServices;

namespace Snarl
{
	[StructLayout( LayoutKind.Sequential, Pack = 4 )]
	struct SNARLSTRUCTEX
	{
		public Int16 Cmd;
		public Int32 Id;
		public Int32 Timeout;
		public Int32 LngData2;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = SnarlConnector.SNARL_STRING_LENGTH)]
		public byte[] Title;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = SnarlConnector.SNARL_STRING_LENGTH)]
		public byte[] Text;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = SnarlConnector.SNARL_STRING_LENGTH)]
		public byte[] Icon;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = SnarlConnector.SNARL_STRING_LENGTH)]
		public byte[] Class;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = SnarlConnector.SNARL_STRING_LENGTH)]
		public byte[] Extra;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = SnarlConnector.SNARL_STRING_LENGTH)]
		public byte[] Extra2;

		public Int32 Reserved1;
		public Int32 Reserved2;
	}
}
