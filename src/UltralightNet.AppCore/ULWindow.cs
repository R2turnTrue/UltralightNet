using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using UltralightNet.LowStuff;

namespace UltralightNet.AppCore;

public static unsafe partial class AppCoreMethods
{
	[LibraryImport(LibAppCore)]
	internal static unsafe partial void* ulCreateWindow(ULMonitor monitor, uint width, uint height, [MarshalAs(UnmanagedType.U1)] bool fullscreen, ULWindowFlags flags);

	[LibraryImport(LibAppCore)]
	internal static partial void ulDestroyWindow(ULWindow window);

	[LibraryImport(LibAppCore)]
	internal static partial void ulWindowSetCloseCallback(ULWindow window, delegate* unmanaged[Cdecl]<nuint, nuint, void> callback, nuint id);

	[LibraryImport(LibAppCore)]
	internal static partial void ulWindowSetResizeCallback(ULWindow window, delegate* unmanaged[Cdecl]<nuint, nuint, uint, uint, void> callback, nuint id);

	[LibraryImport(LibAppCore)]
	internal static partial uint ulWindowGetScreenWidth(ULWindow window);

	[LibraryImport(LibAppCore)]
	internal static partial uint ulWindowGetWidth(ULWindow window);

	[LibraryImport(LibAppCore)]
	internal static partial uint ulWindowGetScreenHeight(ULWindow window);

	[LibraryImport(LibAppCore)]
	internal static partial uint ulWindowGetHeight(ULWindow window);

	[LibraryImport(LibAppCore)]
	internal static partial void ulWindowMoveTo(ULWindow window, int x, int y);

	[LibraryImport(LibAppCore)]
	internal static partial void ulWindowMoveToCenter(ULWindow window);

	[LibraryImport(LibAppCore)]
	internal static partial int ulWindowGetPositionX(ULWindow window);

	[LibraryImport(LibAppCore)]
	internal static partial int ulWindowGetPositionY(ULWindow window);

	[LibraryImport(LibAppCore)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static partial bool ulWindowIsFullscreen(ULWindow window);

	[LibraryImport(LibAppCore)]
	internal static partial double ulWindowGetScale(ULWindow window);

	[LibraryImport(LibAppCore)]
	internal static partial void ulWindowSetTitle(ULWindow window, [MarshalUsing(typeof(Utf8StringMarshaller))] string title);

	[LibraryImport(LibAppCore)]
	internal static partial void ulWindowSetCursor(ULWindow window, ULCursor cursor);

	[LibraryImport(LibAppCore)]
	internal static partial void ulWindowShow(ULWindow window);

	[LibraryImport(LibAppCore)]
	internal static partial void ulWindowHide(ULWindow window);

	[LibraryImport(LibAppCore)]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static partial bool ulWindowIsVisible(ULWindow window);

	[LibraryImport(LibAppCore)]
	internal static partial void ulWindowClose(ULWindow window);

	[LibraryImport(LibAppCore)]
	internal static partial int ulWindowScreenToPixels(ULWindow window, int val);

	[LibraryImport(LibAppCore)]
	internal static partial int ulWindowPixelsToScreen(ULWindow window, int val);

	[LibraryImport(LibAppCore)]
	internal static unsafe partial void* ulWindowGetNativeHandle(ULWindow window);
}

[NativeMarshalling(typeof(Marshaller))]
public unsafe sealed class ULWindow : NativeContainer
{
	readonly ULApp app;

	private ULWindow(void* ptr, ULApp app)
	{
		Handle = ptr;
		this.app = app;
		app.WindowInstances[(nuint)ptr] = new(this);
		AppCoreMethods.ulWindowSetCloseCallback(this, &NativeOnClose, app.GetUserData());
		AppCoreMethods.ulWindowSetResizeCallback(this, &NativeOnResize, app.GetUserData());
	}

	public event Action? OnClose;
	public event Action<uint, uint>? OnResize;

	public uint ScreenWidth => AppCoreMethods.ulWindowGetScreenWidth(this);
	public uint ScreenHeight => AppCoreMethods.ulWindowGetScreenHeight(this);

	public uint Width => AppCoreMethods.ulWindowGetWidth(this);
	public uint Height => AppCoreMethods.ulWindowGetHeight(this);

	public void MoveToCenter() => AppCoreMethods.ulWindowMoveToCenter(this);

	public (int X, int Y) Position // INTEROPTODO: TEST
	{
		get => new(AppCoreMethods.ulWindowGetPositionX(this), AppCoreMethods.ulWindowGetPositionY(this));
		set => AppCoreMethods.ulWindowMoveTo(this, value.X, value.Y);
	}

	public bool IsFullscreen => AppCoreMethods.ulWindowIsFullscreen(this);

	public double Scale => AppCoreMethods.ulWindowGetScale(this);
	public string Title { set => AppCoreMethods.ulWindowSetTitle(this, value); }

	public ULCursor Cursor { set => AppCoreMethods.ulWindowSetCursor(this, value); }

	public void Show() => AppCoreMethods.ulWindowShow(this);
	public void Hide() => AppCoreMethods.ulWindowHide(this);

	public void Close() => AppCoreMethods.ulWindowClose(this);

	public int ScreenToPixels(int val) => AppCoreMethods.ulWindowScreenToPixels(this, val);
	public int PixelsToScreen(int val) => AppCoreMethods.ulWindowPixelsToScreen(this, val);

	/// <summary>
	/// HWND on windows <br/>
	/// NSWindow* on mac <br/>
	/// GLFWwindow* on linux <br/>
	/// </summary>
	public unsafe void* NativeWindowHandle => AppCoreMethods.ulWindowGetNativeHandle(this);

	public unsafe ULOverlay CreateOverlay(uint width, uint height, int x = 0, int y = 0) => ULOverlay.FromHandle(AppCoreMethods.ulCreateOverlay(this, width, height, x, y), app.Renderer, null);
	public unsafe ULOverlay CreateOverlay(View view, int x = 0, int y = 0) => ULOverlay.FromHandle(AppCoreMethods.ulCreateOverlayWithView(this, view, x, y), app.Renderer, view);

	[UnmanagedCallersOnly(CallConvs = new Type[] { typeof(CallConvCdecl) })]
	static void NativeOnClose(nuint app, nuint window) => GetWindow(app, window).OnClose?.Invoke();
	[UnmanagedCallersOnly(CallConvs = new Type[] { typeof(CallConvCdecl) })]
	static void NativeOnResize(nuint app, nuint window, uint width, uint height) => GetWindow(app, window).OnResize?.Invoke(width, height);

	public override void Dispose()
	{
		if (!IsDisposed && Owns) AppCoreMethods.ulDestroyWindow(this);
		GC.KeepAlive(app);
		base.Dispose();
	}

	public static unsafe ULWindow FromHandle(void* ptr, ULApp app) => new(ptr, app);

	[CustomMarshaller(typeof(ULWindow), MarshalMode.ManagedToUnmanagedIn, typeof(Marshaller))]
	internal ref struct Marshaller
	{
		private ULWindow window;

		public void FromManaged(ULWindow window) => this.window = window;
		public readonly unsafe void* ToUnmanaged() => window.Handle;
		public readonly void Free() => GC.KeepAlive(window);
	}

	static ULWindow GetWindow(nuint appId, nuint windowId)
	{
		if (ULApp.Instances[appId].TryGetTarget(out var app))
		{
			if (app.WindowInstances[windowId].TryGetTarget(out var window))
			{
				return window;
			}
			else throw new ObjectDisposedException(nameof(ULWindow));
		}
		else throw new ObjectDisposedException(nameof(ULApp));
	}
}
