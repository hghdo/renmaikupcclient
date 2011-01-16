using System;
using System.Runtime.InteropServices;

namespace Snarl
{
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	struct SNARLSTRUCT
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
	}
}
