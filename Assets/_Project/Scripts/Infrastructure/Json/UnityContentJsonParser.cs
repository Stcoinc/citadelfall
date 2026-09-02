using ClubGamerZone.TowerDefense.Application.Configuration.Dtos;
using UnityEngine;

namespace ClubGamerZone.TowerDefense.Infrastructure.Json
{
    public sealed class UnityContentJsonParser : IContentJsonParser
    {
        public StarterContentDto Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonUtility.FromJson<StarterContentDto>(json);
        }
    }
}
