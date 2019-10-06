//* ---------------------------------------------------------------
//* "THE BEERWARE LICENSE" (Revision 42):
//* Nikolai "Kolyasisan" Ponomarev @ PCHK Studios wrote this code.
//* As long as you retain this notice, you can do whatever you
//* want with this stuff. If we meet someday, and you think this
//* stuff is worth it, you can buy me a beer in return.
//* ---------------------------------------------------------------

//Uncomment this in order to initialize the debugger, which will be added as a script alongside this one in the DontDestroyOnLoadScene.
//#define INITIALIZEDEBUGGER

using UnityEngine;

/// <summary>
/// All you need for any time related stuff.
/// </summary>
public class CoreTime : CoreMonoBeh
{
    #region constantvariables

    /// <summary>
    /// Used to reset the time values.
    /// </summary>
    const float TIME_RESET_THRESHOLD = 64000f;

    /// <summary>
    /// If deltaTime > FixedDeltaTimeRange * this - we may not be stable
    /// </summary>
    const float TIME_MULTIPLIER_FOR_STABILIZE_UPPER = 1.15f;

    /// <summary>
    /// If deltaTime > FixedDeltaTimeRange * this - we may not be stable
    /// </summary>
    const float TIME_MULTIPLIER_FOR_STABILIZE_LOWER = 0.85f;

    /// <summary>
    /// A delta time fluctuation that needs to be met when comparing to the previous frame in order to detect that the system has suffered a Heartbeat Stutter.
    /// </summary>
    const float TIME_MULTIPLIER_HEARTBEATSTUTTER_DETECT_UPPER = 1.25f;

    /// <summary>
    /// A delta time fluctuation that needs to be met when comparing to the previous frame in order to detect that the system has suffered a Heartbeat Stutter.
    /// </summary>
    const float TIME_MULTIPLIER_HEARTBEATSTUTTER_DETECT_LOWER = 0.75f;

    #endregion

    #region variables

    private static float internal_previousDeltaTime;

    /// <summary>
    /// Previous frame's Delta Time value.
    /// </summary>
    public static float previousDeltaTime { get { return m_previousDeltaTime; } }
    private static float m_previousDeltaTime;

    /// <summary>
    /// An optimized Delta Time variable that minimizes fluctuations of Hearbeat Stutter.
    /// </summary>
    public static float optimalDeltaTime { get { return m_optimalDeltaTime; } }
    private static float m_optimalDeltaTime;

    /// <summary>
    /// An optimized Delta Time variable that minimizes fluctuations of Hearbeat Stutter. Not scaled by the time scale.
    /// </summary>
    public static float optimalUnscaledDeltaTime { get { return m_optimalUnscaledDeltaTime; } }
    private static float m_optimalUnscaledDeltaTime;

    /// <summary>
    /// An unoptimzied Delta Time variable with scaling. Not recommended, use OptimalDeltaTime instead.
    /// </summary>
    public static float trueDeltaTime { get { return m_trueDeltaTime; } }
    private static float m_trueDeltaTime;

    /// <summary>
    /// An unoptimzied Delta Time variable without scaling. Not scaled by the time scale. Not recommended, use OptimalUnscaledDeltaTime instead.
    /// </summary>
    public static float trueUnscaledDeltaTime { get { return m_trueUnscaledDeltaTime; } }
    private static float m_trueUnscaledDeltaTime;

    /// <summary>
    /// Duh. Unscaled variants of times are not affected by this.
    /// </summary>
    public static float timeScale { get { return m_timeScale; } set { m_timeScale = value; } }
    private static float m_timeScale = 1f;

    /// <summary>
    /// An imaginary perfect Delta Time for the current monitor if we consider that the framerate never drops. Used to eliminate Heartbeat Stutter.
    /// </summary>
    public static float perfectDeltaTime { get { return m_perfectDeltaTime; } }
    private static float m_perfectDeltaTime = 0f;

    /// <summary>
    /// Time in seconds since the initialization.
    /// </summary>
    public static float time { get { return m_time; } }
    private static float m_time;

    /// <summary>
    /// Game Time in seconds since the initialization.
    /// </summary>
    public static float gameTime { get { return m_gameTime; } }
    private static float m_gameTime;

    /// <summary>
    /// Time since the last sequence break (e.g., a loading screen). You need to reset it yourself.
    /// </summary>
    public static float timeSinceLastBreak { get { return m_timeSinceLastBreak; } }
    private static float m_timeSinceLastBreak;

    /// <summary>
    /// Game time scine the last sequence break (e.g., a loading screen). Counts only when in gameplay. You need to reset it yourself.
    /// </summary>
    public static float gameTimeSinceLastBreak { get { return m_gameTimeSinceLastBreak; } }
    private static float m_gameTimeSinceLastBreak;

    /// <summary>
    /// Gets set to true if the system detected a potential Heartbeat Stutter that can majorly disrupt the pace.
    /// </summary>
    public static bool containsHeartbeatStutter { get { return m_containsHeartbeatStutter; } }
    private static bool m_containsHeartbeatStutter;

    public static CoreTimeSlowdownTypeEnum slowdownType { get { return m_slowdownType; } }
    private static CoreTimeSlowdownTypeEnum m_slowdownType;

    #endregion

    #region functions

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Init()
    {
        GameObject go = new GameObject("CoreTime");
        DontDestroyOnLoad(go);
        go.AddComponent<CoreTime>();

#if UNITY_EDITOR && INITIALIZEDEBUGGER
        go.AddComponent<CoreTimeDebugger>();
#endif
    }

    public override void CoreInitSetup()
    {
        UM_SETTINGS_UPDATE = new LoopUpdateSettings(32, false, true);
        UM_SETTINGS_GAMEPLAYUPDATE = new LoopUpdateSettings(32, false, true);
    }

    /// <summary>
    /// Should be called first.
    /// </summary>
    public override void CoreUpdate()
    {
        m_containsHeartbeatStutter = false;
        bool closeToTargetFramerate = false;
        bool isFramerateUncapped = false;

        m_perfectDeltaTime = (float)QualitySettings.vSyncCount / (float)Screen.currentResolution.refreshRate;
        if (QualitySettings.vSyncCount == 0)
            isFramerateUncapped = true;

        m_trueUnscaledDeltaTime = Time.unscaledDeltaTime;
        m_trueDeltaTime = m_trueUnscaledDeltaTime * timeScale;

        if (!isFramerateUncapped)
        {
            if (m_trueUnscaledDeltaTime > internal_previousDeltaTime * TIME_MULTIPLIER_HEARTBEATSTUTTER_DETECT_UPPER || m_trueUnscaledDeltaTime < internal_previousDeltaTime * TIME_MULTIPLIER_HEARTBEATSTUTTER_DETECT_LOWER)
                m_containsHeartbeatStutter = true;

            if (m_trueUnscaledDeltaTime > m_perfectDeltaTime * TIME_MULTIPLIER_FOR_STABILIZE_LOWER && m_trueUnscaledDeltaTime < m_perfectDeltaTime * TIME_MULTIPLIER_FOR_STABILIZE_UPPER)
                closeToTargetFramerate = true;

            //We have no stutter and no slowdown.
            if (!m_containsHeartbeatStutter && closeToTargetFramerate)
            {
                m_optimalDeltaTime = m_perfectDeltaTime * m_timeScale;
                m_optimalUnscaledDeltaTime = m_perfectDeltaTime;
                m_slowdownType = CoreTimeSlowdownTypeEnum.Perfect;
            }
            //We have slowdown
            else if (!m_containsHeartbeatStutter && !closeToTargetFramerate)
            {
                m_optimalDeltaTime = m_trueDeltaTime;
                m_optimalUnscaledDeltaTime = m_trueUnscaledDeltaTime;
                m_slowdownType = CoreTimeSlowdownTypeEnum.Slowdown;
            }
            //We have stutter
            else
            {
                m_optimalDeltaTime = m_perfectDeltaTime * m_timeScale;
                m_optimalUnscaledDeltaTime = m_perfectDeltaTime;
                m_slowdownType = CoreTimeSlowdownTypeEnum.Stutter;
            }
        }
        else
        {
            m_optimalDeltaTime = m_trueDeltaTime;
            m_optimalUnscaledDeltaTime = m_trueUnscaledDeltaTime;
            m_slowdownType = CoreTimeSlowdownTypeEnum.Uncapped;
        }

        if (m_timeSinceLastBreak > TIME_RESET_THRESHOLD)
            m_timeSinceLastBreak -= TIME_RESET_THRESHOLD;

        m_timeSinceLastBreak += m_optimalDeltaTime;
        m_time += m_optimalDeltaTime;

        m_previousDeltaTime = internal_previousDeltaTime;
        internal_previousDeltaTime = m_trueUnscaledDeltaTime;
    }

    /// <summary>
    /// Should be called second.
    /// </summary>
    public override void CoreGameplayUpdate()
    {
        if (m_gameTimeSinceLastBreak > TIME_RESET_THRESHOLD)
            m_gameTimeSinceLastBreak -= TIME_RESET_THRESHOLD;

        m_gameTimeSinceLastBreak += m_optimalDeltaTime;
        m_gameTime += m_optimalDeltaTime;
    }

    /// <summary>
    /// Should be called by you in order to reset timeSinceLastBreak values. Typically should be called on level load, transitions, etc.
    /// </summary>
    [ContextMenu("Try reset time")]
    public static void ResetTimeValues()
    {
        m_timeSinceLastBreak = 0f;
        m_gameTimeSinceLastBreak = 0f;
    }

    #endregion
}