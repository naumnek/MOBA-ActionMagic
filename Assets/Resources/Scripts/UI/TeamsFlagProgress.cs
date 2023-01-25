using System;
using UnityEngine;
using UnityEngine.UI;
using Platinum.Info;
using TMPro;
using Platinum.Level;

namespace Platinum.UI
{
    [Serializable]
    public struct TeamImage
    {
        public Image teamFillImages;
        public TMP_Text amountText;
    }

    public class TeamsFlagProgress : MonoBehaviour
    {
        [Header("General")] 
        public TeamImage[] teamUI;
        public Color enemyColor;
        public Color playerColor;
        
        private CenterMapCircle centerMapCircle;
        private bool activation = true;
        private int[] teamsScore;

        private void Awake()
        {
            centerMapCircle = FindObjectOfType<CenterMapCircle>();
            foreach (var team in teamUI)
            {
                team.amountText.text = "0%";
                team.teamFillImages.fillAmount = 0;
            }
            centerMapCircle.onUpdateTeamScore += OnUpdateTeamScore;
        }

        private void OnUpdateTeamScore(int team)
        {
            if (activation) SetFillImages();
            teamsScore[team] += 1;
            teamUI[team].teamFillImages.fillAmount = teamsScore[team] / 100f;
            teamUI[team].amountText.text = teamsScore[team] + "%";
        }

        private void SetFillImages()
        {
            activation = false;
            teamsScore = new int[centerMapCircle.actorsManager.AffiliationsInfo.Count];
            for (int i = 0; i < teamUI.Length; i++)
            {
                teamUI[i].teamFillImages.color = i == centerMapCircle.actorsManager.PlayerAffiliation ? playerColor : enemyColor;
            }
        }
    }
}