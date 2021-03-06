﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OhScrap
{
    class SRBFailureModule : BaseFailureModule
    {
        ModuleEngines engine;
        bool message;

        //SRBs can fail straight away, and will override the "launched" bool because we need them to fail before the player launches.
        //They will however suppress the messages until the player tries to launch.
        protected override void Overrides()
        {
            launched = true;
            Fields["displayChance"].guiName = "Chance of SRB Failure";
            Fields["safetyRating"].guiName = "SRB Safety Rating";
            failureType = "ignition failure";
            engine = part.FindModuleImplementing<ModuleEngines>();
            suppressFailure = true;
        }

        protected override bool FailureAllowed()
        {
            if (KRASHWrapper.simulationActive()) return false;
            if (engine.currentThrottle > 0) return false;
            return HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().SRBFailureModuleAllowed;
        }
        //Part will just shutdown and not be restartable.
        protected override void FailPart()
        {
            if (engine.currentThrottle == 0) return;
            engine.allowShutdown = true;
            engine.allowRestart = false;
            engine.Shutdown();
            suppressFailure = false;
            if (!message)
            {
                if(vessel.vesselType != VesselType.Debris) ScreenMessages.PostScreenMessage(part.partInfo.title + " has failed to ignite");
                Debug.Log("[OhScrap]: " + SYP.ID + " has failed to ignite");
                postMessage = true;
                message = true;
            }
        }
        //SRBs cant be reoaired.
        public override void RepairPart()
        {
            ScreenMessages.PostScreenMessage("Igniting an SRB manually doesn't seem like a good idea");
            ModuleUPFMEvents UPFM = part.FindModuleImplementing<ModuleUPFMEvents>();
            UPFM.customFailureEvent = true;
            return;
        }
    }
}
