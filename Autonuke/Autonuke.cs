using Exiled.API.Features;
using Exiled.API.Interfaces;
using MEC;
using System.ComponentModel;
using Exiled.Events.EventArgs;
using System.Collections.Generic;

namespace Autonuke
{
    public class Autonuke : Plugin<Config>
    {
		private EventHandlers ev;
		internal static Autonuke instance;

		internal CoroutineHandle coroutine;

		private bool state = false;

		public override void OnEnabled()
		{
			if (state) return;
			ev = new EventHandlers();

			instance = this;

			Exiled.Events.Handlers.Server.RoundStarted += ev.OnRoundStarted;
			Exiled.Events.Handlers.Server.RestartingRound += ev.OnRoundRestart;
			Exiled.Events.Handlers.Warhead.Stopping += ev.OnWarheadStop;

			state = true;
			base.OnEnabled();
		}

		public override void OnDisabled()
		{
			if (!state) return;

			Exiled.Events.Handlers.Server.RoundStarted -= ev.OnRoundStarted;
			Exiled.Events.Handlers.Server.RestartingRound -= ev.OnRoundRestart;
			Exiled.Events.Handlers.Warhead.Stopping -= ev.OnWarheadStop;

			Timing.KillCoroutines(coroutine);

			ev = null;
			state = false;
			base.OnDisabled();
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
		private bool isAutoNukeGoingOff = false;

		internal void OnRoundStarted()
		{
			if (Autonuke.instance.coroutine.IsRunning) Timing.KillCoroutines(Autonuke.instance.coroutine);
			Autonuke.instance.coroutine = Timing.RunCoroutine(Timer());
		}

		internal IEnumerator<float> Timer ()
        {
			for (int i = 0; i<Autonuke.instance.Config.TimeUntilStart; i++) yield return Timing.WaitForSeconds(1f);
			if (!Warhead.IsInProgress) Warhead.Start();
			isAutoNukeGoingOff = true;
		}

		internal void OnRoundRestart()
		{
			Timing.KillCoroutines(Autonuke.instance.coroutine);
			isAutoNukeGoingOff = false;
		}

		internal void OnWarheadStop(StoppingEventArgs ev) => ev.IsAllowed = !(!Autonuke.instance.Config.CanStopDetonation && isAutoNukeGoingOff);
	}
}
