using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Shared.GameTicking;
using System.Linq;
using Content.Shared.Random.Helpers;

namespace Content.Server.White.StationGoal
{
    /// <summary>
    ///     Station goal is set at round start.
    ///     Admin can change the station goal via <see cref="StationGoalCommand"></see>
    /// </summary>
    public sealed class StationGoalSystem : EntitySystem
    {
        [Dependency] private readonly StationGoalPaperSystem _stationGoalPaperSystem = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly IRobustRandom _random = default!;

        private static readonly String _staticGoalId = "StationGoalMain";

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<RoundStartedEvent>(OnRoundStarted);
        }

        private void OnRoundStarted(RoundStartedEvent ev)
        {
            // Goal will be static for now.
            // CreateRandomStationGoal();
            CreateStaticGoal(_staticGoalId);
        }

        public void CreateRandomStationGoal()
        {
            var availableGoals = _prototypeManager.EnumeratePrototypes<StationGoalPrototype>();

            var random = IoCManager.Resolve<IRobustRandom>();
            var goal = random.Pick(availableGoals.ToList());
            _stationGoalPaperSystem.SpawnStationGoalPaper(goal);
        }

        public bool CreateStaticGoal(string goalId)
        {
            if (!_prototypeManager.TryIndex<StationGoalPrototype>(goalId, out var goal))
            {
                Logger.Error($"Couldn't spawn station goal with ID {goalId}");
                return false;
            }

            _stationGoalPaperSystem.SpawnStationGoalPaper(goal);
            return true;
        }


    }
}
