namespace HASSIO.Addon.T9PlusLED.Models
{
	internal class T9PlusLEDState(T9PlusLEDState.Modes mode = T9PlusLEDState.Modes.Auto, byte brightness = 5, byte speed = 3)
	{
		private static readonly byte _CMD_PREFIX		= 0xFA;
		private static readonly byte _MIN_VALUE			= 1;
		private static readonly byte _MAX_VALUE			= 5;
		private static readonly byte _INVERT_VALUE		= 6;

		public enum Modes: byte
		{
			Rainbow				= 0x01,
			Breathing			= 0x02,
			ColorCycle			= 0x03,
			Off					= 0x04,
			Auto				= 0x05,
		}

		public Modes Mode		{ get; } = mode;

		public byte Brightness	{ get; } = brightness<_MIN_VALUE ? _MIN_VALUE : _MAX_VALUE<brightness ? _MAX_VALUE : brightness;

		public byte Speed		{ get; } = speed<_MIN_VALUE      ? _MIN_VALUE : _MAX_VALUE<speed      ? _MAX_VALUE : speed;

		public byte[] ToCommand()
		{
			var mode			= (byte)Mode;
			var brightness      = (byte)(_INVERT_VALUE-Brightness);	// Значение нужно инвертировать
			var speed			= (byte)(_INVERT_VALUE-Speed);		// Значение нужно инвертировать
			var checksum        = (byte)((_CMD_PREFIX + mode + brightness + speed) & 0xFF);
			return [ _CMD_PREFIX, mode, brightness, speed, checksum ];
		}

		public override string ToString()
		{
			return $"{Mode} 🔅{Brightness} 🕑{Speed}";
		}

		public override bool Equals(object? obj)
		{
			return obj is T9PlusLEDState v
				&& v.Mode==Mode && v.Brightness==brightness && v.Speed==Speed;
		}

		public override int GetHashCode()
		{
			return new { Mode, Brightness, Speed }.GetHashCode();
		}
	}
}
