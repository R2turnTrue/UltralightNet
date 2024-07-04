using System.Runtime.CompilerServices;

namespace UltralightNet;

/// <summary>Rendering details for a View, to be used with your own GPUDriver</summary>
public struct RenderTarget : IEquatable<RenderTarget>
{
	private byte _IsEmpty;
	/// <summary>Whether this target is empty (null texture)</summary>
	public bool IsEmpty { readonly get => Methods.BitCast<byte, bool>(_IsEmpty); set => _IsEmpty = Methods.BitCast<bool, byte>(value); }

	/// <summary>The viewport width (in device coordinates).</summary>
	public uint Width;

	/// <summary>The viewport height (in device coordinates).</summary>
	public uint Height;

	/// <summary><see cref="ULGPUDriver" />'s texture id</summary>
	public uint TextureId;

	/// <summary>The texture width (in pixels). This may be padded.</summary>
	public uint TextureWidth;

	/// <summary>The texture height (in pixels). This may be padded.</summary>
	public uint TextureHeight;

	private byte _TextureFormat;
	/// <summary>The pixel format of the texture.</summary>
	public ULBitmapFormat TextureFormat { readonly get => Methods.BitCast<byte, ULBitmapFormat>(_TextureFormat); set => _TextureFormat = Methods.BitCast<ULBitmapFormat, byte>(value); }

	/// <summary>UV coordinates of the texture (this is needed because the texture may be padded).</summary>
	public ULRect UV;

	/// <summary><see cref="ULGPUDriver" />'s render buffer id</summary>
	public uint RenderBufferId;

	public readonly bool Equals(RenderTarget rt) =>
		IsEmpty == rt.IsEmpty &&
		Width == rt.Width &&
		Height == rt.Height &&
		TextureId == rt.TextureId &&
		TextureWidth == rt.TextureWidth &&
		TextureHeight == rt.TextureHeight &&
		TextureFormat == rt.TextureFormat &&
		UV == rt.UV &&
		RenderBufferId == rt.RenderBufferId;
	public override readonly bool Equals(object? other) => other is RenderTarget rt && Equals(rt);
#if NETSTANDARD2_1 || NETCOREAPP2_1_OR_GREATER
	public override readonly int GetHashCode() => HashCode.Combine(HashCode.Combine(IsEmpty, Width, Height, TextureId), HashCode.Combine(TextureWidth, TextureHeight, TextureFormat, UV), RenderBufferId);
#endif
}
