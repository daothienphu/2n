using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;
using DG.Tweening;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager> {
    [SerializeField] private int _width = 4;
    [SerializeField] private int _height = 4;
    [SerializeField] private Node _nodePrefab;
    [SerializeField] private SpriteRenderer _boardPrefab;
    [SerializeField] private Block _blockPrefab;
    [SerializeField] private List<BlockType> _types;
    [SerializeField] private List<long> _expandThresholds;
    [SerializeField] private float _travelTime = 0.2f;
    [SerializeField] private long _targetNumToReach = 2048;
    [SerializeField] private GameObject _winScreen;
    [SerializeField] private GameObject _loseScreen;
    [SerializeField] private GameObject _pauseScreen;
    [SerializeField] private TextMeshProUGUI _winScreenStats;
    [SerializeField] private TextMeshProUGUI _loseScreenStats;
    [SerializeField] private TextMeshProUGUI _pauseScreenStats;
    [SerializeField] private TextMeshProUGUI _targetText;
    [SerializeField] private Animator _loadSceneSmootherAnimator;
    [SerializeField] private Transform _nodesRoot;
    [SerializeField] private Transform _blocksRoot;
    [SerializeField] private Transform _environmentRoot;
    [SerializeField] private AudioClip _slideSFX;
    [SerializeField] private AudioClip _mergeSFX;
    [SerializeField] private AudioClip _loseSFX;
    [SerializeField] private AudioClip _winSFX;
    [SerializeField] private Sprite _soundOn;
    [SerializeField] private Sprite _soundOff;
    [SerializeField] private Image _soundButton;

    private SpriteRenderer _board;
    private Vector2 _boardCenter;
    private List<Node> _nodes;
    private List<Block> _blocks;
    private GameState _state;
    private int _round = 0;
    private float _totalGameTime;
    private int _totalMoves = 0;
    private long _highestScore = 0;
    private float _totalPausedTime = 0;
    private Camera _camera;
    public BlockType GetBlockTypeByValue(long value){
        return _types.First(t => t.Value == value);
    }

    private void Start() {
        _camera = Camera.main;
        long currentNum = _types.Last().Value ;
        for (int i = Mathf.RoundToInt(Mathf.Log(currentNum, 2)) + 1; i < 51; ++i){
            currentNum *= 2;
            BlockType tmp;
            tmp.Value = currentNum;
            tmp.Color = _types[(i - 1) % 10].Color;
            _types.Add(tmp);
        }
        ChangeState(GameState.GenerateLevel);
    }

    private void Update(){
        if (_state == GameState.Paused){
            return;
        }

        if (_state == GameState.SceneTransition){
            var stateInfo = _loadSceneSmootherAnimator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("SceneLoadSmoother") && stateInfo.normalizedTime >= 1.0f){
                SceneManager.LoadScene("MenuScene");
            }
        }

        if (_state == GameState.Restart){
            var stateInfo = _loadSceneSmootherAnimator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("SceneLoadSmoother") && stateInfo.normalizedTime >= 1.0f){
                SceneManager.LoadScene("MainScene");
            }
        }

        if (_state == GameState.WaitingInput) {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) Shift(Vector2.left);
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) Shift(Vector2.right);
            else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) Shift(Vector2.up);
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) Shift(Vector2.down);
        } 
    }

    private void ChangeState(GameState state){
        _state = state;
        Debug.Log($"Changed state to {state}");
        switch(state){
            case GameState.GenerateLevel:
                _totalGameTime = Time.time;
                _targetNumToReach = _expandThresholds.First();
                _targetText.text = $"Target: {_targetNumToReach}";
                GenerateGrid();
                break;
            case GameState.SpawningBlock:
                SpawnBlocks(_round++ == 0 ? 2 : 1);
                break;
            case GameState.WaitingInput:
                break;
            case GameState.Moving:
                break;
            case GameState.Win:
                _totalGameTime = (Time.time - _totalGameTime) / 60;
                _winScreenStats.text = $"You beat the game in {_totalGameTime:0.##} minutes, \n using a total of {_totalMoves} moves.";
                AudioSystem.Instance.PlaySound(_winSFX, Vector3.zero);
                _winScreen.GetComponent<Animator>().SetTrigger("Won");
                break;
            case GameState.Lose:
                _loseScreenStats.text = $"You achieved a score of {_highestScore},\n using a total of {_totalMoves} moves.\nYour target was {_targetNumToReach} tho :(";
                AudioSystem.Instance.PlaySound(_loseSFX, Vector3.zero);
                _loseScreen.GetComponent<Animator>().SetTrigger("Lost");
                break;
            case GameState.SceneTransition:
                break;
            case GameState.Restart:
                break;
            case GameState.Paused:
                float currentGameTime = (Time.time - _totalGameTime) / 60;
                _pauseScreenStats.text = $"Playtime: {currentGameTime:0.##} minutes\nHigh score: {_highestScore}\nMoves: {_totalMoves}";
                break;
            case GameState.Expand:
                _targetNumToReach = _expandThresholds[_expandThresholds.IndexOf(_targetNumToReach) + 1]; 
                _targetText.text = $"Target: {_targetNumToReach}";
                Expand();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    void GenerateGrid() {
        _nodes = new List<Node>();
        _blocks = new List<Block>();
        
        _boardCenter = new Vector2((float) _width / 2 - 0.5f, (float) _height / 2 - 0.5f);
        _camera.transform.position = new Vector3(_boardCenter.x, _boardCenter.y, -10);

        _board = Instantiate(_boardPrefab, _boardCenter, Quaternion.identity, _environmentRoot);
        _board.size = new Vector2(_width, _height);
        _board.transform.localScale = Vector3.zero;
        _board.transform.DOScale(1.2f, 0.6f).OnComplete(() => {
            _board.transform.DOScale(1.025f, 0.2f).OnComplete(() => {
                for (int x = 0; x < _width; ++x){
                    for (int y = 0; y < _height; ++y){
                        var node = Instantiate(_nodePrefab, new Vector2(x, y), Quaternion.identity, _nodesRoot);
                        _nodes.Add(node);
                    }
                }

                ChangeState(GameState.SpawningBlock);
            });            
        });
    }

    void SpawnBlocks(int amount) {
        var freeNodes = _nodes.Where(n => n.OccupiedBlock == null).OrderBy(b => Random.value);

        foreach (var node in freeNodes.Take(amount)){
            SpawnBlock(node, Random.value > 0.92f ? 4 : 2);
        }

        ChangeState(CheckEndGame());
    }

    void SpawnBlock(Node node, long value, bool mergingBlock = false){
        var block = Instantiate(_blockPrefab, node.Pos, Quaternion.identity, _blocksRoot);
        block.Init(GetBlockTypeByValue(value), mergingBlock);
        block.SetBlock(node);
        _blocks.Add(block);
    }

    void Shift(Vector2 dir){
        ChangeState(GameState.Moving);
        var orderedBlocks = _blocks.OrderBy(b => b.Pos.x).ThenBy(b => b.Pos.y).ToList();
        if (dir == Vector2.right || dir == Vector2.up) orderedBlocks.Reverse();

        foreach (var block in orderedBlocks){
            var nextNode = block.Node;
            do {
                block.SetBlock(nextNode);
                var possibleNode = GetNodeAtPosition(nextNode.Pos + dir);
                if (possibleNode != null){
                    if (possibleNode.OccupiedBlock != null && possibleNode.OccupiedBlock.CanMerge(block.Value)){
                        block.MergeBlock(possibleNode.OccupiedBlock);
                    }
                    else if (possibleNode.OccupiedBlock == null){
                        nextNode = possibleNode;
                    }
                }
            } while (nextNode != block.Node);
        }

        int movableBlocks = orderedBlocks.Where(b => b.Pos != b.Node.Pos || (b.MergingBlock != null && b.MergingBlock.Node.Pos != b.Pos)).Count();
        if (movableBlocks == 0){
            ChangeState(GameState.WaitingInput);
            return;
        }

        _totalMoves += 1;

        var sequence = DOTween.Sequence();

        foreach (var block in orderedBlocks){
            var movePoint = block.MergingBlock != null ? block.MergingBlock.Node.Pos : block.Node.Pos;
            sequence.Insert(0, block.transform.DOMove(movePoint, _travelTime));
        }

        if (orderedBlocks.Where(b => b.MergingBlock != null).Count() > 0){
            AudioSystem.Instance.PlaySound(_mergeSFX, Vector3.zero);
        }
        AudioSystem.Instance.PlaySound(_slideSFX, Vector3.zero);
        sequence.OnComplete(()=>{
            foreach (var block in orderedBlocks.Where(b => b.MergingBlock != null)){
                MergeBlocks(block, block.MergingBlock);
            }

            _highestScore = _blocks.OrderBy(b => b.Value).Last().Value;

            ChangeState(GameState.SpawningBlock);
        });
    }

    void MergeBlocks(Block baseBlock, Block mergingBlock){
        SpawnBlock(mergingBlock.Node, baseBlock.Value * 2, mergingBlock:true);
        RemoveBlock(baseBlock);
        RemoveBlock(mergingBlock);
    }

    void RemoveBlock(Block block){
        _blocks.Remove(block);
        Destroy(block.gameObject);
    }

    Node GetNodeAtPosition(Vector2 pos){
        return _nodes.FirstOrDefault(n => n.Pos == pos);
    }

    GameState CheckEndGame()
    {
        if (_blocks.Any(b => b.Value == _targetNumToReach)) {
            if (_targetNumToReach == _expandThresholds.Last())
                return GameState.Win;
            else 
                return GameState.Expand;
        }
        else if (CheckLoseCondition()) return GameState.Lose;
        return GameState.WaitingInput;
    }

    Block GetBlockAtPosition(Vector2 pos){
        return _blocks.FirstOrDefault(b => b.Pos == pos);
    }

    Block GetBlockByValue(long value){
        return _blocks.FirstOrDefault(b => b.Value == value);
    }

    bool CheckLoseCondition()
    {
        var freeNodes = _nodes.Where(n => n.OccupiedBlock == null).OrderBy(b => Random.value).ToList();
        if (freeNodes.Count() > 0) return false;

        foreach (var block in _blocks)
        {   
            var leftBlock = GetBlockAtPosition(new Vector2(block.Pos.x - 1, block.Pos.y));
            var rightBlock = GetBlockAtPosition(new Vector2(block.Pos.x + 1, block.Pos.y));
            var topBlock = GetBlockAtPosition(new Vector2(block.Pos.x, block.Pos.y + 1));
            var bottomBlock = GetBlockAtPosition(new Vector2(block.Pos.x, block.Pos.y - 1));

            if (leftBlock != null && leftBlock.Value == block.Value) return false;
            if (rightBlock != null && rightBlock.Value == block.Value) return false;
            if (topBlock != null && topBlock.Value == block.Value) return false;
            if (bottomBlock != null && bottomBlock.Value == block.Value) return false;
        }
        return true;
    }

    public void OnMenuButtonClicked(){
        _loadSceneSmootherAnimator.SetTrigger("ChangeScene");
        ChangeState(GameState.SceneTransition);
    }

    public void OnRestartButtonClicked(){
        _loadSceneSmootherAnimator.SetTrigger("ChangeScene");
        ChangeState(GameState.Restart);
    }

    public void OnPauseButtonClicked(){
        _pauseScreen.GetComponent<Animator>().SetTrigger("Pause");
        _totalPausedTime = Time.time;
        ChangeState(GameState.Paused);
    }

    public void OnResumeButtonClicked(){
        _pauseScreen.GetComponent<Animator>().SetTrigger("Pause");
        _totalPausedTime = Time.time - _totalPausedTime;
        _totalGameTime += _totalPausedTime;
        ChangeState(GameState.WaitingInput);
    }

    public void OnSoundButtonClicked() {
        if (AudioSystem.Instance.IsMuted()) {
            AudioSystem.Instance.Unmute();
            _soundButton.sprite = _soundOn;
        }
        else {
            AudioSystem.Instance.Mute();
            _soundButton.sprite = _soundOff;
        }
    }

    void Expand(){
        _width++;
        _height++;
        _board.size = new Vector2(_width, _height);
        Block highestBlock = GetBlockByValue(_highestScore);
        Debug.Log(_boardCenter);
        Debug.Log($"{_width}, {_height}");
        int newX = 0;
        if (highestBlock.Pos.x >= _boardCenter.x){
            newX = Mathf.RoundToInt(_boardCenter.x - _width / 2.0f);
            _boardCenter.x -= 0.5f;
        }
        else{
            newX = Mathf.RoundToInt(_boardCenter.x + _width / 2.0f);
            _boardCenter.x += 0.5f;
        }
        for (int y = Mathf.RoundToInt(_boardCenter.y - (_height - 2.0f) / 2.0f); y <= Mathf.RoundToInt(_boardCenter.y + (_height - 2.0f) / 2.0f); y++){
            Debug.Log($"instantiating at {newX}, {y}");
            var node = Instantiate(_nodePrefab, new Vector2(newX, y), Quaternion.identity, _nodesRoot);
            _nodes.Add(node);
        }
        Debug.Log("haha");
        int newY = 0;
        if (highestBlock.Pos.y >= _boardCenter.y){
            newY = Mathf.RoundToInt(_boardCenter.y - _height / 2.0f);
            _boardCenter.y -= 0.5f;
        }
        else {
            newY = Mathf.RoundToInt(_boardCenter.y + _height / 2.0f);
            _boardCenter.y += 0.5f;
        }
        for (int x = Mathf.RoundToInt(_boardCenter.x - (_width - 1.0f) / 2.0f); x <= Mathf.RoundToInt(_boardCenter.x + (_width - 1.0f) / 2.0f); ++x){
            Debug.Log($"instantiating at {x}, {newY}");
            var node = Instantiate(_nodePrefab, new Vector2(x, newY), Quaternion.identity, _nodesRoot);
            _nodes.Add(node);
        }

        _camera.transform.position = new Vector3(_boardCenter.x, _boardCenter.y, -10);
        _camera.orthographicSize = _width / 2.0f + 0.5f;
        _board.transform.position = new Vector3(_boardCenter.x, _boardCenter.y, 0);
        

        ChangeState(GameState.WaitingInput);
    }
}

[Serializable]
public struct BlockType{
    public long Value;
    public Color Color;
}

public enum GameState{
    GenerateLevel = 0,
    SpawningBlock,
    WaitingInput,
    Moving,
    Win,
    Lose,
    SceneTransition,
    Restart,
    Paused,
    Expand
}