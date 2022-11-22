using UnityEngine;
using Verse;

namespace VREHussars
{


    public class Gene_Dutiful : Gene
    {
        public MentalBreakDef preventedMentalBreak;
        public MentalStateDef preventedMentalState;
        string reason = null; 
        bool causedByMood; 
        Pawn otherPawn; 
        bool transitionSilently; 
        bool causedByDamage;
        bool causedByPsycast;

        public void RegisterMentalBreak(MentalBreakDef mentalBreak, string reason, bool causedByMood)
        {
            this.reason = reason;
            this.preventedMentalBreak = mentalBreak;
            this.causedByMood = causedByMood;
            this.preventedMentalState = null;
            this.otherPawn = null;
            this.transitionSilently = default;
            this.causedByDamage = default;
            this.causedByPsycast = default;
        }

        public void RegisterMentalState(MentalStateDef stateDef, string reason = null, bool causedByMood = false, 
            Pawn otherPawn = null, bool transitionSilently = false, bool causedByDamage = false, bool causedByPsycast = false)
        {
            this.reason = reason;
            this.preventedMentalState = stateDef;
            this.preventedMentalBreak = null;
            this.causedByMood = causedByMood;
            this.otherPawn = otherPawn;
            this.transitionSilently = transitionSilently;
            this.causedByDamage = causedByDamage;
            this.causedByPsycast = causedByPsycast;
        }

        public void TryDoMentalBreak()
        {
            if (preventedMentalState != null)
            {
                pawn.mindState.mentalStateHandler.TryStartMentalState(preventedMentalState, reason, false, causedByMood, otherPawn, 
                    transitionSilently, causedByDamage, causedByPsycast);
                ResetData();

            }
            else if (preventedMentalBreak != null)
            {
                preventedMentalBreak.Worker.TryStart(pawn, reason, causedByMood); 
                ResetData();
            }
        }

        public void ResetData()
        {
            preventedMentalBreak = null;
            preventedMentalState = null;
            reason = null;
            causedByMood = default;
            otherPawn = null;
            transitionSilently = default;
            causedByDamage = default;
            causedByPsycast = default;
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref preventedMentalBreak, "preventedMentalBreak");
            Scribe_Defs.Look(ref preventedMentalState, "preventedMentalState");
            Scribe_Values.Look(ref reason, "reason");
            Scribe_Values.Look(ref causedByMood, "causedByMood");
            Scribe_References.Look(ref otherPawn, "otherPawn");
            Scribe_Values.Look(ref transitionSilently, "transitionSilently");
            Scribe_Values.Look(ref causedByDamage, "causedByDamage");
            Scribe_Values.Look(ref causedByPsycast, "causedByPsycast");
        }
    }
}
