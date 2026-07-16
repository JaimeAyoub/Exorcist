using UnityEngine;
using UnityEngine.Playables;
using UnityUtils;

public class TimelinesManager : Singleton<TimelinesManager>
{
    public PlayableDirector StartCombatTimeline;

    public PlayableDirector EndCombatTimeline;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void PauseTimeLine(PlayableDirector timeline)
    {
        if (timeline)
        {
            timeline.Pause();
        }
    }


    public void ResumeTimeLine(PlayableDirector timeline)
    {
        if (timeline)
        {
            timeline.Resume();
        }
        else
        {
            Debug.LogWarning("No hay timeline");
        }
    }

    public void PlayTimeLine(PlayableDirector timeline)
    {
        if (timeline)
        {
            timeline.Play();
        }
    }
    
    
}