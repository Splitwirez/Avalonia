﻿#nullable enable

using System;
using System.Runtime.InteropServices;
using System.Threading;
using Avalonia.Platform;

namespace Avalonia.LinuxFramebuffer.Output
{
    internal unsafe class FbDevBackBuffer : IDisposable
    {
        private readonly int _fb;
        private readonly fb_fix_screeninfo _fixedInfo;
        private readonly fb_var_screeninfo _varInfo;
        private readonly IntPtr _targetAddress;
        private readonly object _lock = new object();

        public FbDevBackBuffer(int fb, fb_fix_screeninfo fixedInfo, fb_var_screeninfo varInfo, IntPtr targetAddress)
        {
            _fb = fb;
            _fixedInfo = fixedInfo;
            _varInfo = varInfo;
            _targetAddress = targetAddress;
            Address = Marshal.AllocHGlobal(RowBytes * Size.Height);
        }
        

        public void Dispose()
        {
            if (Address != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(Address);
                Address = IntPtr.Zero;
            }
        }

        public static LockedFramebuffer LockFb(IntPtr address, fb_var_screeninfo varInfo,
            fb_fix_screeninfo fixedInfo, Vector dpi, Action? dispose)
        {
            return new LockedFramebuffer(address,
                new PixelSize((int)varInfo.xres, (int)varInfo.yres),
                (int)fixedInfo.line_length, dpi,
                varInfo.bits_per_pixel == 16 ? PixelFormat.Rgb565
                : varInfo.blue.offset == 16 ? PixelFormat.Rgba8888
                : PixelFormat.Bgra8888, dispose);
        }

        public ILockedFramebuffer Lock(Vector dpi)
        {
            Monitor.Enter(_lock);
            try
            {
                return LockFb(Address, _varInfo, _fixedInfo, dpi,
                    () =>
                    {
                        try
                        {
                            NativeUnsafeMethods.ioctl(_fb, FbIoCtl.FBIO_WAITFORVSYNC, null);
                            NativeUnsafeMethods.memcpy(_targetAddress, Address, new IntPtr(RowBytes * Size.Height));
                        }
                        finally
                        {
                            Monitor.Exit(_lock);
                        }
                    });
            }
            catch
            {
                Monitor.Exit(_lock);
                throw;
            }
        }

        public IntPtr Address { get; private set; }
        public PixelSize Size => new PixelSize((int)_varInfo.xres, (int) _varInfo.yres);
        public int RowBytes => (int) _fixedInfo.line_length;
    }
}
