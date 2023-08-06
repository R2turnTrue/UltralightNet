using System.Diagnostics.CodeAnalysis;

namespace UltralightNet.LowStuff;

public abstract unsafe class NativeContainer : IDisposable, IEquatable<NativeContainer>
{
	private void* handle;
	protected virtual void* Handle
	{
		get => !IsDisposed ? handle : throw new ObjectDisposedException(nameof(NativeContainer));
		init => handle = value;
	}

	public bool IsDisposed { get; private set; }

	private bool _Owns = true;
	protected bool Owns
	{
		get => _Owns;
		[SuppressMessage("Usage", "CA1816: Call GC.SupressFinalize correctly")]
		init
		{
			if (value is false) GC.SuppressFinalize(this);
			_Owns = value;
		}
	}

	public virtual void Dispose()
	{
		IsDisposed = true;
		handle = default;
		GC.SuppressFinalize(this);
	}
	~NativeContainer() => Dispose(); // it does work (tested on MODiX)

	public bool Equals(NativeContainer? other) => other is not null && handle == other.handle && IsDisposed == other.IsDisposed;

	public override bool Equals(object? other) => other is NativeContainer container && Equals(container);
#if !NETSTANDARD2_0
	public override int GetHashCode() => HashCode.Combine((nuint)handle, IsDisposed);
#endif
}
