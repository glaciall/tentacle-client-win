using cn.org.hentai.tentacle.graphic;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace MirrSharp.Driver
{
    public class DesktopMirror : IDisposable
	{
		#region External Constants

		private const int Map = 1030;
		private const int UnMap = 1031;
		private const int TestMapped = 1051;

		private const int IGNORE = 0;
		private const int BLIT = 12;
		private const int TEXTOUT = 18;
		private const int MOUSEPTR = 48;

		private const int CDS_UPDATEREGISTRY = 0x00000001;
		private const int CDS_TEST = 0x00000002;
		private const int CDS_FULLSCREEN = 0x00000004;
		private const int CDS_GLOBAL = 0x00000008;
		private const int CDS_SET_PRIMARY = 0x00000010;
		private const int CDS_RESET = 0x40000000;
		private const int CDS_SETRECT = 0x20000000;
		private const int CDS_NORESET = 0x10000000;
		private const int MAXIMUM_ALLOWED = 0x02000000;
		private const int DM_BITSPERPEL = 0x40000;
		private const int DM_PELSWIDTH = 0x80000;
		private const int DM_PELSHEIGHT = 0x100000;
		private const int DM_POSITION = 0x00000020;
		#endregion

		#region External Methods

		[DllImport("user32.dll")]
		private static extern int ChangeDisplaySettingsEx(string lpszDeviceName, ref DeviceMode mode, IntPtr hwnd, uint dwflags, IntPtr lParam);

		[DllImport("gdi32.dll", SetLastError = true)]
		private static extern IntPtr CreateDC(string lpszDriver, string lpszDevice, string lpszOutput, IntPtr lpInitData);

		[DllImport("gdi32.dll")]
		private static extern bool DeleteDC(IntPtr pointer);

		[DllImport("user32.dll")]
		private static extern bool EnumDisplayDevices(string lpDevice, uint ideviceIndex, ref DisplayDevice lpdevice, uint dwFlags);

		[DllImport("gdi32.dll", SetLastError = true)]
		private static extern int ExtEscape(IntPtr hdc, int nEscape, int cbInput, IntPtr lpszInData, int cbOutput, IntPtr lpszOutData);

		[DllImport("user32.dll", EntryPoint = "GetDC")]
		private static extern IntPtr GetDC(IntPtr ptr);

		[DllImport("user32.dll", EntryPoint = "ReleaseDC")]
		private static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDc);

		#endregion

		public event EventHandler<DesktopChangeEventArgs> DesktopChange;
		public class DesktopChangeEventArgs : EventArgs
		{
			public int x1;
			public int y1;
			public int x2;
			public int y2;
			public OperationType type;

			public DesktopChangeEventArgs(int x1, int y1, int x2, int y2, OperationType type)
			{
				this.x1 = x1;
				this.y1 = y1;
				this.x2 = x2;
				this.y2 = y2;
				this.type = type;
			}
		}

		private string driverInstanceName = "";
		private IntPtr _getChangesBuffer = IntPtr.Zero;

		private static void SafeChangeDisplaySettingsEx(string lpszDeviceName, ref DeviceMode mode, IntPtr hwnd, uint dwflags, IntPtr lParam)
		{
			int result = ChangeDisplaySettingsEx(lpszDeviceName, ref mode, hwnd, dwflags, lParam);
			switch (result)
			{
				case 0: return; //DISP_CHANGE_SUCCESSFUL
				case 1: throw new Exception("The computer must be restarted for the graphics mode to work."); //DISP_CHANGE_RESTART
				case -1: throw new Exception("The display driver failed the specified graphics mode."); // DISP_CHANGE_FAILED
				case -2: throw new Exception("The graphics mode is not supported."); // DISP_CHANGE_BADMODE
				case -3: throw new Exception("Unable to write settings to the registry."); // DISP_CHANGE_NOTUPDATED
				case -4: throw new Exception("An invalid set of flags was passed in."); // DISP_CHANGE_BADFLAGS
				case -5: throw new Exception("An invalid parameter was passed in. This can include an invalid flag or combination of flags."); // DISP_CHANGE_BADPARAM
				case -6: throw new Exception("The settings change was unsuccessful because the system is DualView capable."); // DISP_CHANGE_BADDUALVIEW
			}
		}

		public enum MirrorState
		{
			Idle,
			Loaded,
			Connected,
			Running
		}

		public MirrorState State { get; private set; }

		private const string driverDeviceNumber = "DEVICE0";
		private const string driverMiniportName = "dfmirage";
		private const string driverName = "Mirage Driver";
		private const string driverRegistryPath = "SOFTWARE\\CurrentControlSet\\Hardware Profiles\\Current\\System\\CurrentControlSet\\Services";
        
        // 屏幕截屏图像，以RGB为次序排列，多申请一些空间，省得来回调整
        private int[] screenBitmap = new int[4096 * 2160];

        private bool Load()
		{
			var device = new DisplayDevice();
			var deviceMode = new DeviceMode { dmDriverExtra = 0 };

			device.CallBack = Marshal.SizeOf(device);
			deviceMode.dmSize = (short)Marshal.SizeOf(deviceMode);
			deviceMode.dmBitsPerPel = Screen.PrimaryScreen.BitsPerPixel;

			if (deviceMode.dmBitsPerPel == 24)
				deviceMode.dmBitsPerPel = 32;

            if (deviceMode.dmBitsPerPel != 32) throw new Exception("不支持的颜色模式：" + deviceMode.dmBitsPerPel);

			deviceMode.dmDeviceName = string.Empty;
			deviceMode.dmFields = (DM_BITSPERPEL | DM_PELSWIDTH | DM_PELSHEIGHT | DM_POSITION);

			bool deviceFound;
			uint deviceIndex = 0;

			while (deviceFound = EnumDisplayDevices(null, deviceIndex, ref device, 0))
			{
				if (device.DeviceString == driverName)
					break;
				deviceIndex++;
			}

            if (!deviceFound) throw new Exception("无法加载驱动程序，请确保己正确安装dfmirage-setup-xxx.exe");

			driverInstanceName = device.DeviceName;

			#region This was CommitDisplayChanges

			SafeChangeDisplaySettingsEx(device.DeviceName, ref deviceMode, IntPtr.Zero, CDS_UPDATEREGISTRY, IntPtr.Zero);
			SafeChangeDisplaySettingsEx(device.DeviceName, ref deviceMode, IntPtr.Zero, 0, IntPtr.Zero);

			#endregion

			return true;
		}

		public void Init()
		{
            this.Load();

            bool result = mapSharedBuffers();
            if (!result) throw new Exception("初始化失败");

            State = MirrorState.Running;
		}

		public void Release()
		{
            unmapSharedBuffers();

            var deviceMode = new DeviceMode();
			deviceMode.dmSize = (short)Marshal.SizeOf(typeof(DeviceMode));
			deviceMode.dmDriverExtra = 0;
			deviceMode.dmFields = (DM_BITSPERPEL | DM_PELSWIDTH | DM_PELSHEIGHT | DM_POSITION);

			var device = new DisplayDevice();
			device.CallBack = Marshal.SizeOf(device);
			deviceMode.dmDeviceName = string.Empty;
			uint deviceIndex = 0;
			while (EnumDisplayDevices(null, deviceIndex, ref device, 0))
			{
				if (device.DeviceString.Equals(driverName))
					break;

				deviceIndex++;
			}

			deviceMode.dmDeviceName = driverMiniportName;

			if (deviceMode.dmBitsPerPel == 24) deviceMode.dmBitsPerPel = 32;

            State = MirrorState.Idle;

            #region This was CommitDisplayChanges

            SafeChangeDisplaySettingsEx(device.DeviceName, ref deviceMode, IntPtr.Zero, CDS_UPDATEREGISTRY, IntPtr.Zero);
			SafeChangeDisplaySettingsEx(device.DeviceName, ref deviceMode, IntPtr.Zero, 0, IntPtr.Zero);

			#endregion
		}

		private IntPtr _globalDC;

		private bool mapSharedBuffers()
		{
			_globalDC = CreateDC(driverInstanceName, null, null, IntPtr.Zero);
			if (_globalDC == IntPtr.Zero)
			{
				throw new Win32Exception("Code: " + Marshal.GetLastWin32Error() + ", Drvier: " + driverInstanceName);
			}

			if (_getChangesBuffer != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(_getChangesBuffer);
			}

			_getChangesBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof (GetChangesBuffer)));

			int res = ExtEscape(_globalDC, Map, 0, IntPtr.Zero, Marshal.SizeOf(typeof(GetChangesBuffer)), _getChangesBuffer);
			if (res > 0)
				return true;

			return false;
		}

		private void unmapSharedBuffers()
		{
			int res = ExtEscape(_globalDC, UnMap, Marshal.SizeOf(typeof(GetChangesBuffer)), _getChangesBuffer, 0, IntPtr.Zero);
			if (res < 0)
				throw new Win32Exception(Marshal.GetLastWin32Error());

			Marshal.FreeHGlobal(_getChangesBuffer);
			_getChangesBuffer = IntPtr.Zero;

			ReleaseDC(IntPtr.Zero, _globalDC);
		}

        public unsafe Screenshot captureScreen()
        {
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;

            var getChangesBuffer = (GetChangesBuffer)Marshal.PtrToStructure(_getChangesBuffer, typeof(GetChangesBuffer));
            IntPtr ptr = getChangesBuffer.UserBuffer;
            Trace.WriteLine("ColorDepth: " + Screen.PrimaryScreen.BitsPerPixel);
            GCHandle gc = GCHandle.Alloc(ptr);
            byte* pointer = (byte*)ptr.ToPointer();
            int byteCount = screenWidth * screenHeight;
            for (int i = 0; i < byteCount; i++)
            {
                // BGRA
                byte b = *(pointer++);
                byte g = *(pointer++);
                byte r = *(pointer++);
                byte a = *(pointer++);

                screenBitmap[i] = (a << 24) | (r << 16) | (g << 8) | b;
            }
            gc.Free();
            return new Screenshot(screenBitmap, screenWidth, screenHeight);
        }

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		public void Dispose()
		{
			if (State == MirrorState.Running)
				Release();
		}
	}
}
