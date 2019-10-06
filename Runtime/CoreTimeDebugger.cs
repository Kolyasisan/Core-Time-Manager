using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreTimeDebugger : CoreMonoBeh
{
    public float optimizedUnscaledDeltaTime;
    public float trueDeltaTime;
    public float previousDeltaTime;
    public int stuttersCatched;
    public CoreTimeSlowdownTypeEnum slowdownType;

    private int framesToClearStutters = 300;
    private int curFrames;

    public List<Vector2> stutteredTimings = new List<Vector2>(32);

    public override void CoreInitSetup()
    {
        UM_SETTINGS_UPDATE = new LoopUpdateSettings(33);
    }

    public override void CoreUpdate()
    {
        optimizedUnscaledDeltaTime = CoreTime.optimalUnscaledDeltaTime;
        trueDeltaTime = CoreTime.trueUnscaledDeltaTime;
        previousDeltaTime = CoreTime.previousDeltaTime;
        slowdownType = CoreTime.slowdownType;

        if (CoreTime.containsHeartbeatStutter)
        {
            stuttersCatched++;
            curFrames = 0;

            stutteredTimings.Add(new Vector2(CoreTime.previousDeltaTime, trueDeltaTime));
        }

        curFrames++;
        if (curFrames >= framesToClearStutters)
        {
            stuttersCatched = 0;
            stutteredTimings.Clear();
        }
    }
}
