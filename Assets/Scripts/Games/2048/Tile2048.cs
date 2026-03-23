using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace Games._2048
{
    public class Tile2048 : MonoBehaviour
    {
        public int Value { get; private set; }
        public Cell2048 cell;
        public bool mergedThisTurn;

        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI valueText;

        public void Initialize(int value)
        {
            Value = value;
            UpdateVisuals();
        }

        public void SetValue(int newValue)
        {
            Value = newValue;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            valueText.text = Value.ToString();
            backgroundImage.color = GetColor(Value);
            
            // Adjust text color based on value (dark for low values, light for high)
            valueText.color = Value <= 4 ? new Color32(119, 110, 101, 255) : Color.white;
            
            // Adjust font size for larger numbers
            if (Value >= 1000) valueText.fontSize = 35;
            else if (Value >= 100) valueText.fontSize = 45;
            else valueText.fontSize = 55;
        }

        public void MoveTo(Cell2048 targetCell)
        {
            if (cell != null) cell.ClearTile();
            targetCell.SetTile(this);
            
            transform.DOMove(targetCell.transform.position, 0.1f).SetEase(Ease.OutQuad);
        }

        public void MergeInto(Cell2048 targetCell)
        {
            if (cell != null) cell.ClearTile();
            
            Tile2048 targetTile = targetCell.currentTile;
            targetTile.mergedThisTurn = true;
            
            transform.DOMove(targetCell.transform.position, 0.1f).SetEase(Ease.OutQuad).OnComplete(() => {
                targetTile.SetValue(targetTile.Value * 2);
                targetTile.PlayMergeAnimation();
                Destroy(gameObject);
            });
        }

        public void PlaySpawnAnimation()
        {
            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
        }

        public void PlayMergeAnimation()
        {
            transform.DOPunchScale(Vector3.one * 0.2f, 0.15f, 10, 1);
        }

        private Color GetColor(int value)
        {
            switch (value)
            {
                case 2: return new Color32(238, 228, 218, 255);
                case 4: return new Color32(237, 224, 200, 255);
                case 8: return new Color32(242, 177, 121, 255);
                case 16: return new Color32(245, 149, 99, 255);
                case 32: return new Color32(246, 124, 95, 255);
                case 64: return new Color32(246, 94, 59, 255);
                case 128: return new Color32(237, 207, 114, 255);
                case 256: return new Color32(237, 204, 97, 255);
                case 512: return new Color32(237, 200, 80, 255);
                case 1024: return new Color32(237, 197, 63, 255);
                case 2048: return new Color32(237, 194, 46, 255);
                default: return new Color32(60, 58, 50, 255);
            }
        }
    }
}
