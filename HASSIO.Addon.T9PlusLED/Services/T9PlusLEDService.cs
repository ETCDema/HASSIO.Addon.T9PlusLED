using System;
using System.IO.Ports;
using System.Threading;

using Dm.Data.UM;

using HASSIO.Addon.T9PlusLED.Models;
using HASSIO.Supervisor.API.Services;

using Microsoft.Extensions.Logging;

namespace HASSIO.Addon.T9PlusLED.Services
{
	internal class T9PlusLEDService(ILogger<T9PlusLEDService> log, string portName, string modeEntityID, string brightnessEntityID, string speedEntityID): IDeviceService
	{
		private T9PlusLEDState? _state;

		public void Apply(T9PlusLEDState state)
		{
			if (state==null || _state!=null && _state.Equals(state)) return;

			try
			{
				var cmd			= state.ToCommand();

				using var port  = new SerialPort(portName, 10000);
				port.Open();
				var buf         = new byte[1];
				foreach (var c in cmd)
				{
					buf[0]      = c;
					port.Write(buf, 0, buf.Length);
					Thread.Sleep(5);
				}
				port.Close();

				log.LogInformation("New T9 Plus LED state: {state}", state);
			} catch (Exception ex)
			{
				log.LogError(ex, "Apply state {state} FAIL", state);
			}
			Interlocked.Exchange(ref _state, state);
		}

		public string[] GetListeningEntities()
		{
			return [ modeEntityID, brightnessEntityID, speedEntityID ];
		}

		public IDeviceStateUpdater NewUpdater()
		{
			return new _updater(this, modeEntityID, brightnessEntityID, speedEntityID);
		}

		private class _updater(T9PlusLEDService owner, string modeEntityID, string brightnessEntityID, string speedEntityID) : IDeviceStateUpdater
		{
			private T9PlusLEDState? _state;

			public bool TryMap(string entityId, IMapSource src)
			{
				var changed		= false;
				while (src.GetNextProp())
				{
					if (src.NodeName=="state")
						changed	= _trySet(entityId, src);
					else
						src.Skip();
				}

				return changed;
			}

			public void Update()
			{
				if (_state!=null) owner.Apply(_state);
			}

			private bool _trySet(string entityId, IMapSource src)
			{
				if (entityId==modeEntityID)
				{
					var prev    = _getPrev();
					_state      = new T9PlusLEDState((T9PlusLEDState.Modes)Enum.Parse(typeof(T9PlusLEDState.Modes), src.GetData<string>()!, true), prev.Brightness, prev.Speed);
					return true;
				}
				if (entityId==brightnessEntityID)
				{
					var prev    = _getPrev();
					_state      = new T9PlusLEDState(prev.Mode, src.GetData<byte>(), prev.Speed);
					return true;
				}
				if (entityId==speedEntityID)
				{
					var prev    = _getPrev();
					_state      = new T9PlusLEDState(prev.Mode, prev.Brightness, src.GetData<byte>());
					return true;
				}

				return false;
			}

			private T9PlusLEDState _getPrev()
			{
				return _state ?? owner._state ?? new T9PlusLEDState();
			}
		}
	}
}
