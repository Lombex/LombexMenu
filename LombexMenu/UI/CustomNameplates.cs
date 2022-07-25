using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRC;
using UnityEngine;
using Utils;
using TMPro;
using MelonLoader;

namespace LombexMenu.UI
{
    public static class CustomNameplates
    {
        public enum Rank
        {
            User,
            VIP,
            Moderator,
            Host,
            Admin
        }
        private static IEnumerator SetRankNameplate(Rank rank, Player player)
        {
            while (ReferenceEquals(PlayerUtils.GetVRCPlayer(), null)) yield return new WaitForEndOfFrame();
            while (ReferenceEquals(player, null)) yield return new WaitForEndOfFrame();
            Transform QuickStats = player?.GetVRCPlayer().transform.Find("Player Nameplate/Canvas/Nameplate/Contents/Quick Stats");
            GameObject GetNameplate = GameObject.Instantiate(QuickStats.gameObject, QuickStats.parent.Find("Status Line"));
            GetNameplate.gameObject.name = "LombexMenu_Rank";
            GetNameplate.transform.localPosition = new Vector3(-90, -58, 0);
            for (int i = 0; i < GetNameplate.transform.childCount; i++)
            {
                Transform Child = GetNameplate.transform.GetChild(i);
                if (Child.gameObject.name != "Performance Text") GameObject.Destroy(Child.gameObject);
            }
            TextMeshProUGUI GetText = GetNameplate.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name.Equals("Performance Text", StringComparison.Ordinal)).GetComponent<TextMeshProUGUI>();
            GetText.m_text = rank.ToString();
            switch (rank)
            {
                case Rank.User:
                    GetText.m_fontColor = new Color(0.6f, 0.85f, 0.35f);
                    break;
                case Rank.VIP:
                    GetText.m_fontColor = new Color(0.5f, 0.6f, 1f);
                    break;
                case Rank.Moderator:
                    GetText.m_fontColor = new Color(0.8f, 0.7f, 1f);
                    break;
                case Rank.Host:
                    GetText.m_fontColor = new Color(1f, 0.8f, 0.35f);
                    break;
                case Rank.Admin:
                    GetText.m_fontColor = new Color(1f, 0.25f, 0.25f);
                    break;
            }
            yield break;
        }
        public static void CreateRankNameplate(Rank rank, Player player) => MelonCoroutines.Start(SetRankNameplate(rank, player));
    }
}
