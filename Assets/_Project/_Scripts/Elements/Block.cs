using UnityEngine;
using TMPro;
using DG.Tweening;

public class Block : MonoBehaviour {
    public long Value;
    public Node Node;
    public Block MergingBlock;
    public bool Merging;
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private SpriteRenderer _rendererShadow;
    [SerializeField] private TextMeshPro _text;
    [SerializeField] private TextMeshPro _textFloating;
    [SerializeField] private float _spawnScaleTime;
    [SerializeField] private Animator anim;
    public Vector2 Pos => transform.position;

    public void Init(BlockType type, bool mergingBlock = false){
        Value = type.Value;
        _renderer.color = type.Color;
        _rendererShadow.color = MakeDarkerColor(type.Color);
        _text.text = type.Value.ToString();
        _textFloating.text = _text.text;

        transform.localScale = new Vector3(0.7f, 0.7f, 1f);
        transform.DOScale(new Vector3(0.9f, 0.9f, 1f), _spawnScaleTime);

        if (mergingBlock){
            anim.SetTrigger("Merged");
        }
    }

    public void SetBlock(Node node){
        if (Node != null){
            Node.OccupiedBlock = null;
        }
        Node = node;
        Node.OccupiedBlock = this;
    }

    public void MergeBlock(Block blockToMergeWith){
        MergingBlock = blockToMergeWith;
        Node.OccupiedBlock = null;
        blockToMergeWith.Merging = true;
    }

    public bool CanMerge(long value) => value == Value && !Merging && MergingBlock == null;

    public Color MakeDarkerColor(Color rgbColor)
    {
        float h, s, v;
        Color.RGBToHSV(rgbColor, out h, out s, out v);
        
        v *= 0.8f;
        
        Color newColor = Color.HSVToRGB(h, s, v);
        return newColor;
    }
}