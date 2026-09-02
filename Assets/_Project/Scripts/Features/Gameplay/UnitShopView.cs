using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    public sealed class UnitShopCardData
    {
        public UnitShopCardData(string id, string name, string description, string stats, int price, bool owned, bool canAfford)
        {
            Id = id;
            Name = name;
            Description = description;
            Stats = stats;
            Price = price;
            Owned = owned;
            CanAfford = canAfford;
        }

        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public string Stats { get; }
        public int Price { get; }
        public bool Owned { get; }
        public bool CanAfford { get; }
    }

    [DisallowMultipleComponent]
    public sealed class UnitShopView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _coinsText;
        [SerializeField] private TMP_Text _gemsText;
        [SerializeField] private TMP_Text _messageText;
        [SerializeField] private UnitShopCardView[] _cards;

        public void Refresh(int coins, int gems, IReadOnlyList<UnitShopCardData> models)
        {
            SetText(_coinsText, $"COINS  {coins:N0}");
            SetText(_gemsText, $"GEMS  {gems:N0}");

            if (_cards == null)
            {
                return;
            }

            for (var i = 0; i < _cards.Length; i++)
            {
                _cards[i]?.Bind(models != null && i < models.Count ? models[i] : null);
            }
        }

        public void SetMessage(string message)
        {
            SetText(_messageText, message ?? string.Empty);
        }

        private static void SetText(TMP_Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }
    }

    [DisallowMultipleComponent]
    public sealed class UnitShopCardView : MonoBehaviour
    {
        [SerializeField] private string _unitId;
        [SerializeField] private Sprite _portraitSprite;
        [SerializeField] private Color _accentColor = Color.white;
        [SerializeField] private Image _portrait;
        [SerializeField] private Image _accent;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private TMP_Text _statsText;
        [SerializeField] private TMP_Text _priceText;
        [SerializeField] private TMP_Text _buttonLabel;
        [SerializeField] private Button _purchaseButton;

        public void Bind(UnitShopCardData data)
        {
            var hasData = data != null && string.Equals(data.Id, _unitId, System.StringComparison.Ordinal);
            gameObject.SetActive(hasData);
            if (!hasData)
            {
                return;
            }

            if (_portrait != null)
            {
                _portrait.sprite = _portraitSprite;
                _portrait.enabled = _portraitSprite != null;
            }

            if (_accent != null)
            {
                _accent.color = _accentColor;
            }

            SetText(_nameText, data.Name.ToUpperInvariant());
            SetText(_descriptionText, data.Description);
            SetText(_statsText, data.Stats);
            SetText(_priceText, data.Owned ? "STARTER UNIT" : $"COINS  {data.Price:N0}");
            SetText(_buttonLabel, data.Owned ? "OWNED" : data.CanAfford ? "BUY" : "NOT ENOUGH");
            if (_purchaseButton != null)
            {
                _purchaseButton.interactable = !data.Owned && data.CanAfford;
            }
        }

        private static void SetText(TMP_Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }
    }
}
