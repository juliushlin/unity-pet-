// Deterministic state transitions: only opening, idle and timeout sleep are automatic.
public sealed class PetMotion
{
    public enum Mode { Opening, Idle, Reaction, Sleeping }
    public Mode State { get; private set; }
    public int Pose { get; private set; }
    public double ChangedAt { get; private set; }
    public const double SleepAfter = 180;
    public void Open(double now){Change(Mode.Opening,1,now);}
    public void React(int pose,double now){Change(pose==4?Mode.Sleeping:Mode.Reaction,pose,now);}
    public void Idle(double now){Change(Mode.Idle,0,now);}
    public void Tick(double now,double inactiveSeconds)
    {
        if(State==Mode.Opening && now-ChangedAt>=1.8)Idle(now);
        if(State==Mode.Reaction && now-ChangedAt>=2.5)Idle(now);
        if(State==Mode.Idle && inactiveSeconds>=SleepAfter)Change(Mode.Sleeping,4,now);
        else if(State==Mode.Sleeping && inactiveSeconds<.1)Idle(now);
    }
    void Change(Mode mode,int pose,double now){State=mode;Pose=pose;ChangedAt=now;}
}
