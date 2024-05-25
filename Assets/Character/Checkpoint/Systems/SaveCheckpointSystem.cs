using System;
using Soil;

namespace Discone {

/// a character's ability to save new checkpoints
/// not => smelling => (grab) planting => (plant) done
[Serializable]
sealed class SaveCheckpointSystem: SimpleSystem<CheckpointContainer > {
    // -- ThirdPerson.System --
    protected override Phase<CheckpointContainer> InitInitialPhase() {
        return NotSaving;
    }

    // -- NotSaving --
    static readonly Phase<CheckpointContainer> NotSaving = new("NotSaving",
        enter: (_, c) => {
            c.State.IsSaving = false;
        },
        update: (_, s, c) => {
            if (CanSave(c)) {
                s.ChangeTo(Delaying);
            }
        },
        exit: (_, c) => {
            c.State.Save_PendingCheckpoint = Checkpoint.FromState(c.Character.State.Next);
        }
    );

    // -- Delaying --
    static readonly Phase<CheckpointContainer> Delaying = new("Delaying",
        update: (_, s, c) => {
            // continue delaying
            if (!CanSave(c)) {
                s.ChangeTo(NotSaving);
                return;
            }

            // start smelling once delay elapses
            if (s.PhaseElapsed > c.Tuning.Save_Delay) {
                s.ChangeTo(Smelling);
            }
        }
    );

    // -- Smelling --
    static readonly Phase<CheckpointContainer> Smelling = new("Smelling",
        enter: (s, c) => {
            c.State.IsSaving = true;
        },
        update: (delta, s, c) => {
            // continue smelling
            if (!CanSave(c)) {
                s.ChangeTo(NotSaving);
                return;
            }

            // start planting once you finish smelling around for a flower
            if (s.PhaseElapsed > c.Tuning.Save_SmellDuration) {
                s.ChangeTo(Planting);
            }
        }
    );

    // -- Planting --
    static readonly Phase<CheckpointContainer> Planting = new("Planting",
        enter: (s, c) => {
            c.GrabCheckpoint();
        },
        update: (delta, s, c) => {
            if (!CanSave(c)) {
                s.ChangeTo(NotSaving);
                return;
            }

            // switch to simply existing after planting
            if (s.PhaseElapsed > c.Tuning.Save_PlantDuration) {
                s.ChangeTo(Being);
            }
        }
    );

    // -- Being --
    static readonly Phase<CheckpointContainer> Being = new("Being",
        enter: (s, c) => {
            c.CreateCheckpoint(c.State.Save_PendingCheckpoint);
        },
        update: (delta, s, c) => {
            if (!CanSave(c)) {
                s.ChangeTo(NotSaving);
                return;
            }
        }
    );

    // -- queries --
    /// if the character can currently save
    static bool CanSave(CheckpointContainer c) {
        return c.Character.State.Curr.IsCrouching && c.Character.State.Curr.IsIdle;
    }
}

}