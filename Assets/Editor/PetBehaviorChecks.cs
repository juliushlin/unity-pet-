using System;
using UnityEngine;
public static class PetBehaviorChecks
{
    public static string Run()
    {
        int checks=0;
        void Expect(bool ok,string label){if(!ok)throw new Exception("Pet behavior check failed: "+label);checks++;}
        var pet=new PetMotion();pet.Open(0);Expect(pet.State==PetMotion.Mode.Opening,"startup opens");
        pet.Tick(1.79,1.79);Expect(pet.State==PetMotion.Mode.Opening,"opening completes before idle");
        pet.Tick(1.8,1.8);Expect(pet.State==PetMotion.Mode.Idle&&pet.Pose==0,"one idle pose after opening");
        for(int i=2;i<180;i++){pet.Tick(i,i);Expect(pet.State==PetMotion.Mode.Idle&&pet.Pose==0,"no random reactions before sleep at "+i);}
        pet.Tick(179.999,179.999);Expect(pet.State==PetMotion.Mode.Idle,"does not sleep early");
        pet.Tick(180,180);Expect(pet.State==PetMotion.Mode.Sleeping&&pet.Pose==4,"sleeps at three minutes");
        pet.Tick(240,240);Expect(pet.State==PetMotion.Mode.Sleeping,"sleep persists");
        pet.Tick(241,0);Expect(pet.State==PetMotion.Mode.Idle,"input wakes to idle");
        pet.React(2,250);Expect(pet.State==PetMotion.Mode.Reaction&&pet.Pose==2,"explicit mouse reaction");
        pet.Tick(252.49,2.49);Expect(pet.Pose==2,"reaction duration");
        pet.Tick(252.5,2.5);Expect(pet.State==PetMotion.Mode.Idle&&pet.Pose==0,"reaction returns to idle");
        pet.Idle(260);pet.Tick(261,1);Expect(pet.Pose==0,"drag release stays idle");
        string result=checks+" checks passed: opening, single idle, no random actions, exact 180-second sleep, wake and click reactions.";
        Debug.Log(result);return result;
    }
}
