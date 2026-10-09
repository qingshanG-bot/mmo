using Common;
using Managers;
using Network;
using SkillBridge.Message;
using System;

public class BagService : Singleton<BagService>, IDisposable
{
    public void Init()
    {
    }

    public void Dispose()
    {
    }

    public void SendBagUnlock(int unlockTo)
    {
        NetMessage message = new NetMessage();
        message.Request = new NetMessageRequest();
        message.Request.bagUnlock = new BagUnlockRequest()
        {
            unlockTo = unlockTo
        };

        NetClient.Instance.SendMessage(message);
    }

    // 收：解锁结果
   
}
