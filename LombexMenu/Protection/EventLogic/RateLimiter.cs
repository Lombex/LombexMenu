using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Protection.EventLogic
{
    internal class RateLimiter
    {
        private readonly IDictionary<int, IDictionary<string, int>> SendsPerSecond;
        private readonly IDictionary<string, int> AllowedSendsPerSecond;
        private readonly HashSet<int> BlacklistedUsers;
        private DateTime CurrentTime = DateTime.Now;
        public void CleanDeparture()
        {
            lock (BlacklistedUsers) BlacklistedUsers.Clear();
            SendsPerSecond.Clear();
        }
        public void OnlyAllowedPerSecond(string EventName, int Amount)
        {
            if (AllowedSendsPerSecond == null) return;
            if (AllowedSendsPerSecond.ContainsKey(EventName)) return;
            AllowedSendsPerSecond[EventName] = Amount;
        }
        public void UnlistUser(int ActorID)
        {
            lock (BlacklistedUsers) BlacklistedUsers.Remove(ActorID);
        }
        public bool IsRateLimited(int SenderID)
        {
            return BlacklistedUsers.Contains(SenderID);
        }
        public void BlacklistUser(int SenderID)
        {
            if (!IsRateLimited(SenderID))
            {
                lock (BlacklistedUsers) BlacklistedUsers.Add(SenderID);
                new Thread(() => 
                {
                    Thread.Sleep(30000);
                    UnlistUser(SenderID);
                });
            }
        }
        public bool IsSafeToRun(string EventName, int SenderID)
        {
            if (SendsPerSecond == null || AllowedSendsPerSecond == null) return true;
            if (BlacklistedUsers.Contains(SenderID)) return false;
            if (DateTime.Now.Subtract(CurrentTime).TotalSeconds > 1)
            {
                CurrentTime = DateTime.Now;
                foreach (var Sends in SendsPerSecond)
                {
                    if (Sends.Value == null) continue;
                    Sends.Value.Clear();
                }
            } else
            {
                if (!SendsPerSecond.ContainsKey(SenderID)) SendsPerSecond.Add(SenderID, new Dictionary<string, int>());
                if (!SendsPerSecond[SenderID].ContainsKey(EventName)) SendsPerSecond[SenderID][EventName] = 1;
                else SendsPerSecond[SenderID][EventName]++;

                if (!AllowedSendsPerSecond.ContainsKey(EventName)) return true;

                if (SendsPerSecond[SenderID][EventName] > AllowedSendsPerSecond[EventName])
                {
                    lock (BlacklistedUsers) BlacklistedUsers.Add(SenderID);
                    new Thread(() => 
                    {
                        Thread.Sleep(30000);
                        UnlistUser(SenderID);
                    });
                    return false;
                }
            }
            return true;
        }
        public RateLimiter()
        {
            SendsPerSecond = new Dictionary<int, IDictionary<string, int>>();
            AllowedSendsPerSecond = new Dictionary<string, int>();
            BlacklistedUsers = new HashSet<int>();
            CurrentTime = DateTime.Now;
            SceneManager.add_sceneUnloaded(new Action<Scene>(s => { CleanDeparture(); }));
        }
    }
}
