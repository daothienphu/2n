using _Project._Scripts.Managers;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project._Scripts.Elements
{
    public class Block : MonoBehaviour {
        [FormerlySerializedAs("Value")] public long _value;
        [FormerlySerializedAs("Node")] public Node _node;
        [FormerlySerializedAs("MergingBlock")] public Block _mergingBlock;
        [FormerlySerializedAs("Merging")] public bool _merging;
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private SpriteRenderer _rendererShadow;
        [SerializeField] private TextMeshPro _text;
        [SerializeField] private TextMeshPro _textFloating;
        [SerializeField] private float _spawnScaleTime;
        [FormerlySerializedAs("anim")] [SerializeField] private Animator _anim;
        private static readonly int Merged = Animator.StringToHash("Merged");
        public Vector2 Pos => transform.position;

        public void Init(BlockType type, bool mergingBlock = false){
            _value = type._value;
            _renderer.color = type._color;
            _rendererShadow.color = MakeDarkerColor(type._color);
            _text.text = type._value.ToString();
            _textFloating.text = _text.text;

            transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            transform.DOScale(new Vector3(0.9f, 0.9f, 1f), _spawnScaleTime);

            if (mergingBlock){
                _anim.SetTrigger(Merged);
            }
        }

        public void SetBlock(Node node){
            if (_node != null){
                _node._occupiedBlock = null;
            }
            _node = node;
            _node._occupiedBlock = this;
        }

        public void MergeBlock(Block blockToMergeWith){
            _mergingBlock = blockToMergeWith;
            _node._occupiedBlock = null;
            blockToMergeWith._merging = true;
        }

        public bool CanMerge(long value) => value == _value && !_merging && _mergingBlock == null;

        private Color MakeDarkerColor(Color rgbColor)
        {
            Color.RGBToHSV(rgbColor, out var h, out var s, out var v);
        
            v *= 0.8f;
        
            Color newColor = Color.HSVToRGB(h, s, v);
            return newColor;
        }
    }
}