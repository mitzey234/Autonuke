using Exiled.API.Features;
using Exiled.API.Interfaces;
using MEC;
using System.ComponentModel;
using Exiled.Events.EventArgs;

namespace Autonuke
{
    public class Autonuke : Plugin<Config>
    {
		private EventHandlers ev;
		internal static Autonuke instance;

		public override void OnEnabled()
		{
			base.OnEnabled();

			ev = new EventHandlers();

			instance = this;

			Exiled.Events.Handlers.Server.RoundStarted += ev.OnRoundStarted;
			Exiled.Events.Handlers.Server.RestartingRound += ev.OnRoundRestart;
			Exiled.Events.Handlers.Warhead.Stopping += ev.OnWarheadStop;
		}

		public override void OnDisabled()
		{
			base.OnDisabled();

			Exiled.Events.Handlers.Server.RoundStarted -= ev.OnRoundStarted;
			Exiled.Events.Handlers.Server.RestartingRound -= ev.OnRoundRestart;
			Exiled.Events.Handlers.Warhead.Stopping -= ev.OnWarheadStop;

			ev = null;
		}

		public override string Name => "Autonuke";
		public override string Author => "Cyanox";
	}

	public class Config : IConfig
	{
		public bool IsEnabled { get; set; } = true;

		[Description("Determines if the nuke can be stopped after automatically starting.")]
		public bool CanStopDetonation { get; set; } = false;

		[Description("The amount of time in seconds before the nuke starts.")]
		public float TimeUntilStart { get; set; } = 900f;
	}

	public class EventHandlers
	{
		private CoroutineHandle coroutine;

		internal void OnRoundStarted() => coroutine = Timing.CallDelayed(Autonuke.instance.Config.TimeUntilStart, () => Warhead.Start());

		internal void OnRoundRestart() => Timing.KillCoroutines(coroutine);

		internal void OnWarheadStop(StoppingEventArgs ev) => ev.IsAllowed = Autonuke.instance.Config.CanStopDetonation;
	}
}
