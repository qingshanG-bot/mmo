using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventController : MonoBehaviour
{
    public EntityEffectManager EffectMgr;

    private void PlayEffect(string name)
    {
        Debug.LogFormat("AnimationEventController:PlayEffect :{0} :{1}", this.name, name);
        EffectMgr.PlayEffect(name);
    }

    private void PlaySound()
    {
        Debug.LogFormat("AnimationEventController : PlaySound:{0} :{1}", this.name, name);
    }
}
