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
                case 2: return new Color32(230, 240, 250, 255);   // 매우 연한 하늘색
                case 4: return new Color32(210, 230, 245, 255);
                case 8: return new Color32(180, 215, 240, 255);
                case 16: return new Color32(150, 200, 235, 255);
                case 32: return new Color32(120, 180, 230, 255);
                case 64: return new Color32(90, 160, 220, 255);
                case 128: return new Color32(70, 140, 210, 255);
                case 256: return new Color32(50, 120, 200, 255);
                case 512: return new Color32(40, 100, 185, 255);
                case 1024: return new Color32(30, 80, 170, 255);
                case 2048: return new Color32(20, 60, 150, 255);  // 가장 진한 하늘색
                default: return new Color32(50, 60, 70, 255);     // 어두운 블루그레이
            }
        }
    }
}
